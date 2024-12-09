using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Vacation;
using StaffScheduling.Web.Models.ViewModels.Vacation;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.VacationService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class VacationService(IUnitOfWork _unitOfWork) : IVacationService
    {
        public async Task<StatusReport> AddVacationOfEmployeeAsync(AddVacationOfEmployeeInputModel model, string userId)
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

            var entityEmployeeInfo = await _unitOfWork
                            .EmployeesInfo
                            .All()
                            .AsNoTracking()
                            .Where(ef => ef.Id == model.EmployeeId && ef.CompanyId == model.CompanyId && String.IsNullOrEmpty(ef.UserId) == false)
                            .FirstOrDefaultAsync(ef => ef.UserId! == userId);

            //Check if employee exists
            if (entityEmployeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            //Check if Model's Vacation Dates are valid
            StatusReport status = await AreVacationDatesValid(model.StartDate, model.EndDate, model.EmployeeId);
            if (status.Ok == false)
            {
                return status;
            }

            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == model.CompanyId && v.EmployeeId == entityEmployeeInfo.Id);

            var entityFound = await entitiesBase
                .FirstOrDefaultAsync(v => v.StartDate == model.StartDate && v.EndDate == model.EndDate);

            //Check if employee already has made a vacation request with the same dates
            if (entityFound != null)
            {
                return new StatusReport
                {
                    Ok = false,
                    Message =
                    String.Format(VacationWithSameDatesExistsFormat, entityFound.StartDate.ToString(VacationDateFormat), entityFound.EndDate.ToString(VacationDateFormat), entityFound.Status.ToString())
                };
            }

            //Calculate vacation days left
            int currentYear = DateTime.Now.Year;
            int nextYear = DateTime.Now.AddYears(1).Year;

            int vacationDaysLeftForCurrentYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, currentYear, entitiesBase);
            int vacationDaysLeftForNextYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, nextYear, entitiesBase);

            Dictionary<int, int> vacationDaysNeededPerYear = CalculateVacationDaysYearSplit(model.StartDate, model.EndDate);

            int totalVacationDays = vacationDaysNeededPerYear[currentYear] + vacationDaysNeededPerYear[nextYear];

            //Check if there are not enough vacation days left for the employee's vacation
            if (vacationDaysNeededPerYear[currentYear] > vacationDaysLeftForCurrentYear || vacationDaysNeededPerYear[nextYear] > vacationDaysLeftForNextYear)
            {
                return new StatusReport { Ok = false, Message = String.Format(NotEnoughVacationDaysLeftFormat, totalVacationDays) };
            }

            int vacationPendingCount = await entitiesBase
                .Where(v => v.Status == VacationStatus.Pending)
                .CountAsync();

            //Check if pending vacation request limit has been hit
            if (vacationPendingCount >= entityCompany.MaxVacationDaysPerYear)
            {
                return new StatusReport { Ok = false, Message = String.Format(VacationPendingLimitHitFormat, entityCompany.MaxVacationDaysPerYear) };
            }

            try
            {
                var newEntity = new Vacation
                {
                    Id = Guid.NewGuid(),
                    CompanyId = model.CompanyId,
                    EmployeeId = model.EmployeeId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedOn = DateTime.Now,
                    Days = totalVacationDays,
                    Status = VacationStatus.Pending,
                };

                await _unitOfWork.Vacations.AddAsync(newEntity);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteVacationOfEmployeeAsync(DeleteVacationOfEmployeeInputModel model, string userId)
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

            var entityEmployeeInfo = await _unitOfWork
                            .EmployeesInfo
                            .All()
                            .AsNoTracking()
                            .Where(ef => ef.Id == model.EmployeeId && ef.CompanyId == model.CompanyId && String.IsNullOrEmpty(ef.UserId) == false)
                            .FirstOrDefaultAsync(ef => ef.UserId! == userId);

            //Check if employee exists
            if (entityEmployeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            var entity = await _unitOfWork
                .Vacations
                .FirstOrDefaultAsync(v => v.Id == model.VacationId && v.CompanyId == entityCompany.Id && v.EmployeeId == entityEmployeeInfo.Id);

            //Check if vacation exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindVacation };
            }

            //Check if vacation status is 'Denied'
            //If it is 'Denied' than the employee can not delete it
            //Because he will be able to make a vacation with same Start Date and End Date and could end up spamming the higher ups
            if (entity.Status == VacationStatus.Denied)
            {
                return new StatusReport { Ok = false, Message = CanNotDeleteDeniedVacation };
            }

            try
            {
                _unitOfWork.Vacations.Delete(entity);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteAllVacationsOfEmployeeAsync(DeleteAllVacationsOfEmployeeInputModel model, string userId)
        {
            //Check if vacation status to delete is 'Denied'
            if (model.VacationStatusToDelete == VacationStatus.Denied)
            {
                return new StatusReport { Ok = false, Message = CanNotDeleteAllDeniedVacations };
            }

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

            var entityEmployeeInfo = await _unitOfWork
                            .EmployeesInfo
                            .All()
                            .AsNoTracking()
                            .Where(ef => ef.Id == model.EmployeeId && ef.CompanyId == model.CompanyId && String.IsNullOrEmpty(ef.UserId) == false)
                            .FirstOrDefaultAsync(ef => ef.UserId! == userId);

            //Check if employee exists
            if (entityEmployeeInfo == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindEmployee };
            }

            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == model.CompanyId && v.EmployeeId == entityEmployeeInfo.Id && v.Status == model.VacationStatusToDelete);

            try
            {
                _unitOfWork.Vacations.DeleteRange(await entitiesBase.ToArrayAsync());

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> ChangeStatusAsync(ChangeStatusInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId)
        {
            //Check if new status is 'Pending'
            //And if it is then don't allow change
            if (model.Status == VacationStatus.Pending)
            {
                return new StatusReport { Ok = false, Message = CanNotChangeVacationStatusToPending };
            }

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

            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.Id == model.VacationId && v.CompanyId == model.CompanyId);

            //Check if user is in role which needs department
            //If it is then get only vacations of employees in that department
            if (userNeededDepartmentId.HasValue)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Employee.DepartmentId == userNeededDepartmentId.Value);
            }

            var entity = await entitiesBase
                .FirstOrDefaultAsync();

            //Check if vacation exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindVacation };
            }

            //Check if user has the required permission level to manage this employee's vacations
            if (userPermissionRole <= RoleMapping[entity.Employee.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotManageEmployeeVacationAsLowerPermission };
            }

            //If new status is the same as old status just skip database changes and say that we successfully changed status
            if (entity.Status == model.Status)
            {
                return new StatusReport { Ok = true };
            }

            try
            {
                entity.Status = model.Status;

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteVacationOfCompanyAsync(DeleteVacationOfCompanyInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId)
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

            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.Id == model.VacationId && v.CompanyId == model.CompanyId);

            //Check if user is in role which needs department
            //If it is then get only vacations of employees in that department
            if (userNeededDepartmentId.HasValue)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Employee.DepartmentId == userNeededDepartmentId.Value);
            }

            var entity = await entitiesBase
                .FirstOrDefaultAsync();

            //Check if vacation exists
            if (entity == null)
            {
                return new StatusReport { Ok = false, Message = CouldNotFindVacation };
            }

            //Check if user has the required permission level to manage this employee's vacations
            if (userPermissionRole <= RoleMapping[entity.Employee.Role])
            {
                return new StatusReport { Ok = false, Message = CanNotManageEmployeeVacationAsLowerPermission };
            }

            try
            {
                _unitOfWork.Vacations.Delete(entity);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<StatusReport> DeleteAllVacationsOfCompanyAsync(DeleteAllVacationsOfCompanyInputModel model, PermissionRole userPermissionRole, Guid? userNeededDepartmentId)
        {
            //Get EmployeeRole roles which have<PermissionRole that the current employee can manage
            //For example if I am an Admin I won't be able to manage other admins
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

            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == model.CompanyId && v.Status == model.VacationStatusToDelete && managableRoles.Contains(v.Employee.Role)); //Get vacations of employees that user can manage

            //Check if user is in role which needs department
            //If it is then get only vacations of employees in that department
            if (userNeededDepartmentId.HasValue)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Employee.DepartmentId == userNeededDepartmentId.Value);
            }

            //Check if there are any employees to delete
            //and if there aren't just make it seem like they were succesfully deleted
            if (await entitiesBase.AnyAsync() == false)
            {
                return new StatusReport { Ok = true };
            }

            try
            {
                _unitOfWork.Vacations.DeleteRange(await entitiesBase.ToArrayAsync());

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusReport { Ok = false, Message = String.Format(DatabaseErrorFormat, ex.Message) };
            }

            return new StatusReport { Ok = true };
        }

        public async Task<ManageScheduleViewModel?> GetCompanyManageScheduleModelAsync(Guid companyId, VacationSortFilter? sortFilter, int page, string userId)
        {
            //Show All by default
            if (sortFilter.HasValue == false)
            {
                sortFilter = VacationSortFilter.All;
            }

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

            var entityEmployeeInfo = await _unitOfWork
                .EmployeesInfo
                .All()
                .AsNoTracking()
                .Where(ef => ef.CompanyId == companyId && String.IsNullOrEmpty(ef.UserId) == false)
                .FirstOrDefaultAsync(ef => ef.UserId! == userId);

            //Check if employee exists
            if (entityEmployeeInfo == null)
            {
                return null;
            }


            //Get IQueryable of the most basic needed results
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == companyId && v.Employee.UserId! == userId);

            //Calculate vacation days left before any filtering is done
            int currentYear = DateTime.Now.Year;
            int nextYear = DateTime.Now.AddYears(1).Year;

            int vacationDaysLeftForCurrentYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, currentYear, entitiesBase);
            int vacationDaysLeftForNextYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, nextYear, entitiesBase);

            //Filter vacations up to VacationSortFilter
            SortByVacationSortFilter(sortFilter.Value, ref entitiesBase);

            //Calculate total vacations and pages
            int totalVacations = await entitiesBase.CountAsync();
            int totalPages = (int)Math.Ceiling(totalVacations / (double)ManageSchedulePageSize);

            List<VacationScheduleViewModel> vacationModels = await entitiesBase
                .Select(v => new VacationScheduleViewModel()
                {
                    Id = v.Id,
                    CompanyId = v.CompanyId,
                    EmployeeId = v.EmployeeId,
                    Status = v.Status,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    CreatedOn = v.CreatedOn,
                    Days = v.Days,
                })
                .OrderByDescending(v => v.CreatedOn)
                .Skip((page - 1) * ManageSchedulePageSize)
                .Take(ManageSchedulePageSize)
                .AsNoTracking()
                .ToListAsync();

            return new ManageScheduleViewModel()
            {
                CompanyId = companyId,
                CompanyName = entityCompany.Name,
                SortFilter = sortFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                Vacations = vacationModels,
                EmployeeId = entityEmployeeInfo.Id,
                VacationDaysLeftCurrentYear = vacationDaysLeftForCurrentYear,
                VacationDaysLeftNextYear = vacationDaysLeftForNextYear
            };
        }

        public async Task<ManageVacationsViewModel?> GetCompanyManageVacationsModelAsync(Guid companyId, string? searchQuery, VacationSortFilter? sortFilter, int page, PermissionRole userPermissionRole, Guid? userNeededDepartmentId)
        {
            //Show All by default
            if (sortFilter.HasValue == false)
            {
                sortFilter = VacationSortFilter.All;
            }

            //Get EmployeeRole roles which have<PermissionRole that the current employee can manage
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
            IQueryable<Vacation> entitiesBase = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == entityCompany.Id)
                .Where(v => managableRoles.Contains(v.Employee.Role));

            //Check if user is in role which needs department
            //If it is then get only vacations of employees in that department
            if (userNeededDepartmentId.HasValue)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Employee.DepartmentId == userNeededDepartmentId.Value);
            }

            //Check if searchQuery has value
            //And if it does then show only vacations of employees
            //Which contain the searchQuery in their FullName or Email
            if (!string.IsNullOrEmpty(searchQuery))
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Employee.User!.FullName!.ToLower().Contains(searchQuery.ToLower())
                                || v.Employee.NormalizedEmail.Contains(searchQuery.ToUpper())
                          );
            }

            //Filter vacations up to VacationSortFilter
            SortByVacationSortFilter(sortFilter.Value, ref entitiesBase);

            //Calculate total vacations and pages
            int totalVacations = await entitiesBase.CountAsync();
            int totalPages = (int)Math.Ceiling(totalVacations / (double)ManageVacationsPageSize);

            List<VacationViewModel> vacationModels = await entitiesBase
                .Select(v => new VacationViewModel()
                {
                    Id = v.Id,
                    CompanyId = v.CompanyId,
                    EmployeeName = v.Employee.User!.FullName!,
                    EmployeeEmail = v.Employee.Email,
                    Status = v.Status,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    CreatedOn = v.CreatedOn,
                    Days = v.Days,
                })
                .OrderByDescending(v => v.CreatedOn)
                .Skip((page - 1) * ManageVacationsPageSize)
                .Take(ManageVacationsPageSize)
                .AsNoTracking()
                .ToListAsync();

            return new ManageVacationsViewModel()
            {
                CompanyId = companyId,
                CompanyName = entityCompany.Name,
                SearchQuery = searchQuery,
                SortFilter = sortFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                Vacations = vacationModels,
            };
        }

        private async Task<int> CalculateVacationDaysLeftForYear(int maxVacationDaysPerYear, int year, IQueryable<Vacation> entitiesBase)
        {
            int usedDays = await entitiesBase
                .Where(v => v.Status == VacationStatus.Pending || v.Status == VacationStatus.Approved) //Count the days only from approved or pending vacations
                .Where(v => v.StartDate.Year <= year && v.EndDate.Year >= year)
                .SumAsync(v =>
                    EF.Functions.DateDiffDay(
                        v.StartDate.Year < year ? new DateTime(year, 1, 1) : v.StartDate,
                        v.EndDate.Year > year ? new DateTime(year, 12, 31) : v.EndDate
                         ) + 1);

            return maxVacationDaysPerYear - usedDays;
        }

        private async Task<StatusReport> AreVacationDatesValid(DateTime startDate, DateTime endDate, Guid employeeToAddToId)
        {
            //Logic Checks

            DateTime tomorrow = DateTime.Today.AddDays(1);
            DateTime xMonthsAfterTomorrow = tomorrow.AddMonths(VacationMaxMonthsFromDates);

            //Check if EndDate is before StartDate
            if (startDate > endDate)
            {
                return new StatusReport { Ok = false, Message = String.Format(ModelErrorMessages.InvalidModelStateFormat, EndDateCanNotBeAfterStartDate) };
            }

            //Check if StartDate is today or in the past
            if (startDate < tomorrow)
            {
                return new StatusReport { Ok = false, Message = String.Format(ModelErrorMessages.InvalidModelStateFormat, StartDateCanNotBeTodayOrInThePast) };
            }

            //Check if start day or end date are more than X month away from tomorrow
            if (startDate > xMonthsAfterTomorrow || endDate > xMonthsAfterTomorrow)
            {
                return new StatusReport
                {
                    Ok = false,
                    Message = String.Format(ModelErrorMessages.InvalidModelStateFormat,
                    String.Format(DatesCanNotBeMoreThanXMonthsFromTomorrowFormat, VacationMaxMonthsFromDates))
                };
            }

            //Database Checks

            //Get all vacations for the employee that have status 'Pending' or 'Approved'
            var existingVacations = await _unitOfWork
                .Vacations
                .All()
                .AsNoTracking()
                .Where(v => v.EmployeeId == employeeToAddToId &&
                            (v.Status == VacationStatus.Pending || v.Status == VacationStatus.Approved))
                .ToListAsync();

            foreach (var vacation in existingVacations)
            {
                //Check if the new start or end date matches any existing start or end date
                if (startDate == vacation.StartDate || startDate == vacation.EndDate)
                {
                    return new StatusReport
                    {
                        Ok = false,
                        Message = String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(StartDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat, startDate.ToString(VacationDateFormat), vacation.Status.ToString()))
                    };
                }
                else if (endDate == vacation.StartDate || endDate == vacation.EndDate)
                {
                    return new StatusReport
                    {
                        Ok = false,
                        Message = String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(EndDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat, endDate.ToString(VacationDateFormat), vacation.Status.ToString()))
                    };
                }

                //Check if the new dates overlap with any existing vacation's range
                if ((startDate >= vacation.StartDate && startDate <= vacation.EndDate) ||
                    (endDate >= vacation.StartDate && endDate <= vacation.EndDate) ||
                    (startDate <= vacation.StartDate && endDate >= vacation.EndDate))
                {
                    return new StatusReport
                    {
                        Ok = false,
                        Message = String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(CanNotAddVacationAsItIsInRangeOfAnotherVacation, vacation.StartDate.ToString(VacationDateFormat), vacation.EndDate.ToString(VacationDateFormat), vacation.Status.ToString()))
                    };
                }
            }

            return new StatusReport { Ok = true };
        }

        private void SortByVacationSortFilter(VacationSortFilter sortFilter, ref IQueryable<Vacation> entitiesBase)
        {
            //Not checking for VacationSortFilter.All because when All we just show the results without sorting
            if (sortFilter == VacationSortFilter.OnlyPending)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Status == VacationStatus.Pending);
            }
            else if (sortFilter == VacationSortFilter.OnlyDenied)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Status == VacationStatus.Denied);
            }
            else if (sortFilter == VacationSortFilter.OnlyApproved)
            {
                entitiesBase = entitiesBase
                    .Where(v => v.Status == VacationStatus.Approved);
            }
        }

        private Dictionary<int, int> CalculateVacationDaysYearSplit(DateTime startDate, DateTime endDate)
        {
            var daysPerYear = new Dictionary<int, int>();
            var currentDate = startDate;

            int currentYear = DateTime.Now.Year;
            int nextYear = DateTime.Now.AddYears(1).Year;

            daysPerYear[currentYear] = 0;
            daysPerYear[nextYear] = 0;

            while (currentDate <= endDate)
            {
                int year = currentDate.Year;

                daysPerYear[year]++;
                currentDate = currentDate.AddDays(1);
            }

            return daysPerYear;
        }
    }
}
