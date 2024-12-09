using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Department;
using StaffScheduling.Web.Models.ViewModels.Department;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.DepartmentService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class DepartmentService(IUnitOfWork _unitOfWork) : IDepartmentService
    {
        public async Task<StatusReport> AddDepartmentManuallyAsync(AddDepartmentManuallyInputModel model)
        {
            var entityCompany = await _unitOfWork
                            .Companies
                            .All()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == model.CompanyId);

            //Check if company exists
            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            int departmentCount = await _unitOfWork
                .Departments
                .All()
                .AsNoTracking()
                .Where(d => d.CompanyId == model.CompanyId)
                .CountAsync();

            //Check if department limit has been hit
            if (departmentCount >= CompanyDepatmentsLimit)
            {
                return new StatusReport { Ok = false, Message = String.Format(DepartmentLimitHitFormat, CompanyEmployeesLimit) };
            }

            var entityFound = await _unitOfWork
            .Departments
            .All()
            .AsNoTracking()
            .Where(d => d.CompanyId == model.CompanyId)
            .FirstOrDefaultAsync(d => d.NormalizedName == model.Name.ToUpper());

            //Check if department with same name already exists
            if (entityFound != null)
            {
                return new StatusReport { Ok = false, Message = String.Format(DepartmentWithNameExistsFormat, entityFound.Name) };
            }

            try
            {
                var newEntity = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    NormalizedName = model.Name.ToUpper(),
                    CompanyId = model.CompanyId,
                };

                await _unitOfWork.Departments.AddAsync(newEntity);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteDepartmentAsync(DeleteDepartmentInputModel model)
        {
            //Get roles which need to have a department
            //Used to check if department can be removed or not
            var rolesWhichNeedDepartment = GetRolesWhichNeedDepartment();

            var entityCompany = await _unitOfWork
                                        .Companies
                                        .All()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.Id == model.CompanyId);

            //Check if company exists
            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            var entity = await _unitOfWork
                .Departments
                .All()
                .Include(c => c.DepartmentEmployeesInfo)
                .FirstOrDefaultAsync(d => d.Id == model.DepartmentId && d.CompanyId == model.CompanyId);

            //Check if department exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindDepartment };
            }

            //Check if there is an employee in the department that has a role which needs a department to work
            if (entity.DepartmentEmployeesInfo.Any(d => rolesWhichNeedDepartment.Contains(d.Role)))
            {
                return new StatusReport
                {
                    Ok = false,
                    Message = String.Format(DepartmentCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment,
                    String.Join(", ", rolesWhichNeedDepartment.Select(role => $"'{role.ToString()}'")))
                };
            }

            try
            {
                //Change the department of employees to null thus making their department 'None'
                foreach (var entityEmployeeInfo in entity.DepartmentEmployeesInfo)
                {
                    entityEmployeeInfo.Department = null;
                    entityEmployeeInfo.DepartmentId = null;
                }

                _unitOfWork.Departments.Delete(entity);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteAllDepartmentsAsync(DeleteAllDepartmentsInputModel model)
        {
            //Get roles which need to have a department
            //Used to check if department can be removed or not
            var rolesWhichNeedDepartment = GetRolesWhichNeedDepartment();

            var entityCompany = await _unitOfWork
                                        .Companies
                                        .All()
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.Id == model.CompanyId);

            //Check if company exists
            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            IQueryable<Department> entitiesBase = _unitOfWork
                .Departments
                .All()
                .Include(d => d.DepartmentEmployeesInfo)
                .Where(d => d.CompanyId == model.CompanyId);

            //Check if there are any departments to delete
            //and if there aren't just make it seem like they were succesfully deleted
            if (await entitiesBase.AnyAsync() == false)
            {
                return new StatusReport { Ok = true };
            }

            List<string> departmentsThatHaveEmployeesWhichNeedDepartment = await entitiesBase
                .SelectMany(d =>
                    d.DepartmentEmployeesInfo
                        .Where(ef => rolesWhichNeedDepartment.Contains(ef.Role) && ef.DepartmentId.HasValue)
                        .Select(ef => ef.Department!.Name)
                )
                .ToListAsync();

            //Check if there is an employee in any of the departments that has a role which needs a department to work
            if (departmentsThatHaveEmployeesWhichNeedDepartment.Any())
            {
                return new StatusReport
                {
                    Ok = false,
                    Message = String.Format(DepartmentsCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment,
                    String.Join(", ", departmentsThatHaveEmployeesWhichNeedDepartment.Select(d => $"'{d}'")))
                };
            }

            try
            {
                //Change the department of the employees to null thus making their department 'None'
                foreach (var entityEmployeeInfo in await entitiesBase.SelectMany(d => d.DepartmentEmployeesInfo).ToListAsync())
                {
                    entityEmployeeInfo.Department = null;
                    entityEmployeeInfo.DepartmentId = null;
                }

                _unitOfWork.Departments.DeleteRange(await entitiesBase.ToArrayAsync());

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<ManageDepartmentsViewModel?> GetCompanyManageDepartmentsModel(Guid companyId, int page)
        {
            var entityCompany = await _unitOfWork
                            .Companies
                            .All()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company exists
            if (entityCompany == null)
            {
                return null;
            }


            //Get IQueryable of the most basic needed results
            IQueryable<Department> selectedDepartments = _unitOfWork
                .Departments
                .All()
                .Include(d => d.DepartmentEmployeesInfo)
                .Where(d => d.CompanyId == companyId);

            //Calculate total departments and pages
            int totalDepartments = await selectedDepartments.CountAsync();
            int totalPages = (int)Math.Ceiling(totalDepartments / (double)ManageDepartmentsPageSize);

            List<DepartmentManageViewModel> departmentModels = await selectedDepartments
                .Select(d => new DepartmentManageViewModel()
                {
                    Id = d.Id,
                    Name = d.Name,
                    EmployeeCount = d.DepartmentEmployeesInfo.Count,
                })
                .OrderBy(d => d.Name) //Order by name
                .ThenByDescending(d => d.EmployeeCount) // Then order by employee count
                .Skip((page - 1) * ManageDepartmentsPageSize)
                .Take(ManageDepartmentsPageSize)
                .AsNoTracking()
                .ToListAsync();

            return new ManageDepartmentsViewModel()
            {
                CompanyId = companyId,
                CompanyName = entityCompany.Name,
                CurrentPage = page,
                TotalPages = totalPages,
                Departments = departmentModels
            };
        }
    }
}
