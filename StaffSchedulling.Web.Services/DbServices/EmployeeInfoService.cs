using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Models.ViewModels.Department;
using StaffScheduling.Web.Models.ViewModels.EmployeeInfo;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.EmployeeInfoService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class EmployeeInfoService(IUnitOfWork _unitOfWork, ApplicationUserManager _userManager) : IEmployeeInfoService
    {
        public async Task<StatusReport> JoinCompanyWithIdAsync(Guid companyId, string userId, string userEmail)
        {
            var entityCompany = await _unitOfWork
                            .Companies
                            .All()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == companyId);

            //Check if company with id exists
            if (entityCompany == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindCompany };
            }

            //Get company owner's email
            string companyOwnerEmail = await _userManager.GetUserEmailFromIdAsync(entityCompany.OwnerId);

            //Check if user trying to join is the company's owner
            if (companyOwnerEmail == userEmail)
            {
                return new StatusReport { Ok = false, Message = OwnerCouldNotHisJoinCompany };
            }

            int joinedCompaniesCount = await _unitOfWork
                .EmployeesInfo
                .All()
                .Where(e => e.Email == userEmail && e.HasJoined == true)
                .AsNoTracking()
                .CountAsync();

            //Check if user has hit joined companies limit
            if (joinedCompaniesCount >= UserJoinedCompaniesLimit)
            {
                return new StatusReport { Ok = false, Message = String.Format(JoinedCompaniesLimitHitFormat, joinedCompaniesCount) };
            }

            var employeeInfo = await _unitOfWork
                .EmployeesInfo
                .All()
                .Where(e => e.CompanyId == companyId && e.Email == userEmail)
                .FirstOrDefaultAsync();

            //Check for wrong employeeInfo
            if (employeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = String.Format(CouldNotFindEmployeeInfoFormat, userEmail) };
            }

            //Check if employee has already joined
            if (employeeInfo.HasJoined == true)
            {
                return new StatusReport { Ok = false, Message = CouldNotJoinAlreadyJoinedCompany };
            }

            try
            {
                employeeInfo.HasJoined = true;
                employeeInfo.UserId = userId;
                await _unitOfWork.SaveChangesAsync();

                return new StatusReport { Ok = true };
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }
        }

        public async Task<StatusReport> AddEmployeeManuallyAsync(AddEmployeeInfoManuallyInputModel model)
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

            int employeeCount = await _unitOfWork
                .EmployeesInfo
                .All()
                .AsNoTracking()
                .Where(ef => ef.CompanyId == model.CompanyId)
                .CountAsync();

            //Check if employee limit has been hit
            if (employeeCount >= CompanyEmployeesLimit)
            {
                return new StatusReport { Ok = false, Message = String.Format(EmployeeLimitHitFormat, CompanyEmployeesLimit) };
            }

            var entityFound = await _unitOfWork
            .EmployeesInfo
            .All()
            .AsNoTracking()
            .Where(ef => ef.CompanyId == model.CompanyId)
            .FirstOrDefaultAsync(ef => ef.NormalizedEmail == model.Email.ToUpper());

            //Check if employee with same email already exists
            if (entityFound != null)
            {
                return new StatusReport { Ok = false, Message = String.Format(EmployeeWithEmailExistsFormat, entityFound.Email) };
            }

            try
            {
                var newEntity = new EmployeeInfo
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email.ToLower(),
                    NormalizedEmail = model.Email.ToUpper(),
                    CompanyId = model.CompanyId,
                };

                await _unitOfWork.EmployeesInfo.AddAsync(newEntity);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };

        }

        public async Task<StatusReport> DeleteEmployeeAsync(DeleteEmployeeInputModel model, PermissionRole userPermissionRole)
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

            var entity = await _unitOfWork
                .EmployeesInfo
                .All()
                .Include(c => c.Vacations)
                .FirstOrDefaultAsync(ef => ef.Id == model.EmployeeId && ef.CompanyId == model.CompanyId);

            //Check if employee exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            //Check if user has the required permission level to manage this employee
            if (userPermissionRole <= RoleMapping[entity.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotManageEmployeeAsLowerPermission };
            }

            try
            {
                _unitOfWork.Vacations.DeleteRange(entity.Vacations.ToArray());

                _unitOfWork.EmployeesInfo.Delete(entity);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteAllEmployeesAsync(DeleteAllEmployeesInputModel model, PermissionRole userPermissionRole)
        {
            //Get EmployeeRole roles which have < PermissionRole that the current employees can manage
            //For example if I am an Admin I won't be able to delete other admins
            //Doing this because RoleMapping[EmployeeRole] can't be translated into SQL from entity
            List<EmployeeRole> managableRoles = GetManageableRoles(userPermissionRole);

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

            IQueryable<EmployeeInfo> entitiesBase = _unitOfWork
                .EmployeesInfo
                .All()
                .Include(c => c.Vacations)
                .Where(ef => ef.CompanyId == model.CompanyId && managableRoles.Contains(ef.Role)); //Get only employees that user can manage

            //Check if there are any employees to delete
            //and if there aren't just make it seem like they were succesfully deleted
            if (await entitiesBase.AnyAsync() == false)
            {
                return new StatusReport { Ok = true };
            }

            try
            {
                _unitOfWork.Vacations.DeleteRange(await entitiesBase.SelectMany(ef => ef.Vacations).ToArrayAsync());

                _unitOfWork.EmployeesInfo.DeleteRange(await entitiesBase.ToArrayAsync());

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> ChangeRoleAsync(ChangeRoleInputModel model, PermissionRole userPermissionRole)
        {
            //Get roles which need to have a department
            //Used to check if employee's role can be changed to one of them
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
                .EmployeesInfo
                .All()
                .Include(ef => ef.Department)
                .FirstOrDefaultAsync(ef => ef.Id == model.EmployeeId && ef.CompanyId == model.CompanyId);

            //Check if employee exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            //Check if user has the required permission level to manage this employee
            if (userPermissionRole <= RoleMapping[entity.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotManageEmployeeAsLowerPermission };
            }

            //Check if user has permission level needed to assign this role
            if (userPermissionRole <= RoleMapping[model.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotChangeEmployeeRoleToHigher };
            }

            //If new role is the same as old role just skip database changes and say that we successfully changed role
            if (entity.Role == model.Role)
            {
                return new StatusReport { Ok = true };
            }

            //Check if new role needs a department and if it is does if employee has department
            if (rolesWhichNeedDepartment.Contains(model.Role))
            {
                if (entity.Department == null)
                {
                    return new StatusReport { Ok = false, Message = String.Format(CanNotChangeEmployeeRoleWithoutDepartmentFormat, model.Role.ToString()) };
                }
            }

            try
            {
                entity.Role = model.Role;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> ChangeDepartmentAsync(ChangeDepartmentInputModel model, PermissionRole userPermissionRole)
        {
            //Get roles which need to have a department
            //Used to check if employee's deparment can be changed to 'None'
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
                .EmployeesInfo
                .All()
                .Include(ef => ef.Department)
                .Where(ef => ef.CompanyId == model.CompanyId)
                .FirstOrDefaultAsync(ef => ef.Id == model.EmployeeId);


            //Check if employee exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            //Check if department is different than 'None'
            if (model.SelectedDepartmentId != Guid.Empty)
            {
                var entityDepartment = await _unitOfWork
                .Departments
                .All()
                .AsNoTracking()
                .Where(ef => ef.CompanyId == model.CompanyId)
                .FirstOrDefaultAsync(d => d.Id == model.SelectedDepartmentId);

                //Check if department exists
                if (entityDepartment == null)
                {
                    return new StatusReport { Ok = false, Message = CouldNotFindDepartment };
                }
            }
            //if department is 'None' then just nullify employee's DepartmentId
            //and say that we sucessfully changed department
            else
            {
                //Check if employee role is one which need department
                //and if it is don't allow department change to 'None'
                if (rolesWhichNeedDepartment.Contains(entity.Role))
                {
                    return new StatusReport { Ok = false, Message = String.Format(CanNotChangeEmployeeDepartmentToNoneBecauseOfRoleFormat, entity.Role) };
                }

                try
                {
                    entity.DepartmentId = null;

                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
                }

                return new StatusReport { Ok = true };
            }

            //Check if user has the required permission level to manage this employee
            if (userPermissionRole <= RoleMapping[entity.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotManageEmployeeAsLowerPermission };
            }

            //If new department is the same as old department just skip database changes and say that we successfully changed department
            if (entity.DepartmentId == model.SelectedDepartmentId)
            {
                return new StatusReport { Ok = true };
            }

            try
            {
                entity.DepartmentId = model.SelectedDepartmentId;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<ManageEmployeesInfoViewModel?> GetCompanyManageEmployeeInfoModel(Guid companyId, string? searchQuery, EmployeeSearchFilter? searchFilter, int page, PermissionRole userPermissionRole)
        {
            //Search by Email by default
            if (searchFilter.HasValue == false)
            {
                searchFilter = EmployeeSearchFilter.Email;
            }

            //Get EmployeeRole roles which have < PermissionRole that the current employee can manage
            //For example if I am an Admin I won't be able to manage other admins
            //Doing this because RoleMapping[EmployeeRole] can't be translated into SQL from entity
            List<EmployeeRole> managableRoles = GetManageableRoles(userPermissionRole);

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
            IQueryable<EmployeeInfo> selectedEmployeesInfo = _unitOfWork
                .EmployeesInfo
                .All()
                .Include(ef => ef.Department)
                .Include(ef => ef.User)
                .Where(ef => ef.CompanyId == companyId)
                .Where(ef => managableRoles.Contains(ef.Role));

            //Check if searchQuery has value then filter employees up to SearchFilter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (searchFilter.Value == EmployeeSearchFilter.Email)
                {
                    selectedEmployeesInfo = selectedEmployeesInfo
                        .Where(ef => ef.Email.ToLower().Contains(searchQuery.ToLower()));
                }
                else if (searchFilter.Value == EmployeeSearchFilter.Name)
                {
                    selectedEmployeesInfo = selectedEmployeesInfo
                        .Where(ef => ef.User != null)
                        .Where(ef => ef.User!.FullName!.ToLower().Contains(searchQuery.ToLower()));
                }
            }
            else //We have no search query but we can still sort for some filters
            {
                //Here I get only joined users, because we are searching by name
                if (searchFilter.Value == EmployeeSearchFilter.Name)
                {
                    selectedEmployeesInfo = selectedEmployeesInfo
                        .Where(ef => ef.User != null);
                }
            }

            //Calculate total employees and pages
            int totalEmployees = await selectedEmployeesInfo.CountAsync();
            int totalPages = (int)Math.Ceiling(totalEmployees / (double)ManageEmployeesPageSize);

            List<EmployeeInfoViewModel> employeeInfoModels = await selectedEmployeesInfo
                .Select(ef => new EmployeeInfoViewModel()
                {
                    Id = ef.Id,
                    Name = ef.User == null ? null : ef.User.FullName,
                    Email = ef.Email,
                    Department = ef.Department == null ? null : ef.Department.Name,
                    HasJoined = ef.HasJoined,
                    Role = ef.Role,
                })
                .OrderByDescending(e => e.HasJoined)    //Show joined first
                .ThenBy(e => e.Name)
                .ThenBy(e => e.Email)
                .Skip((page - 1) * ManageEmployeesPageSize)
                .Take(ManageEmployeesPageSize)
                .AsNoTracking()
                .ToListAsync();

            //Get company departments
            List<DepartmentViewModel> departmentModels = await _unitOfWork
                .Departments
                .All()
                .Where(d => d.CompanyId == companyId)
                .Select(d => new DepartmentViewModel()
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .AsNoTracking()
                .ToListAsync();

            return new ManageEmployeesInfoViewModel()
            {
                CompanyId = companyId,
                CurrentUserPermission = userPermissionRole,
                SearchQuery = searchQuery,
                SearchFilter = searchFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                Employees = employeeInfoModels,
                Departments = departmentModels
            };
        }
    }
}
