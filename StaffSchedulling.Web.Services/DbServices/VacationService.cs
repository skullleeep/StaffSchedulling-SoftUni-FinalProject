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
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.VacationService;

namespace StaffScheduling.Web.Services.DbServices
{
    public class VacationService(IUnitOfWork _unitOfWork) : IVacationService
    {
        public async Task<StatusReport> AddVacationOfEmployeeAsync(AddVacationOfEmployeeInputModel model, string userId)
        {
            //Check if Model's Vacation Dates are valid
            StatusReport status = AreVacationDatesValid(model.StartDate, model.EndDate);
            if (status.Ok == false)
            {
                return status;
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
            IQueryable<Vacation> selectedVacations = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == model.CompanyId && v.EmployeeId == entityEmployeeInfo.Id);

            var entityFound = await selectedVacations
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

            int vacationDaysLeftForCurrentYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, currentYear, selectedVacations);
            int vacationDaysLeftForNextYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, nextYear, selectedVacations);

            Dictionary<int, int> vacationDaysNeededPerYear = CalculateVacationDaysYearSplit(model.StartDate, model.EndDate);

            int totalVacationDays = vacationDaysNeededPerYear[currentYear] + vacationDaysNeededPerYear[nextYear];

            //Check if there are not enough vacation days left for the employee's vacation
            if (vacationDaysNeededPerYear[currentYear] > vacationDaysLeftForCurrentYear || vacationDaysNeededPerYear[nextYear] > vacationDaysLeftForNextYear)
            {
                return new StatusReport { Ok = false, Message = String.Format(NotEnoughVacationDaysLeftFormat, totalVacationDays) };
            }

            int vacationPendingCount = await selectedVacations
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
                .FirstOrDefaultAsync(v => v.Id == model.VacationId && v.EmployeeId == entityEmployeeInfo.Id);

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

        public Task<StatusReport> DeleteAllVacationsOfEmployeeAsync(DeleteAllVacationsOfEmployeeInputModel model, string userId)
        {
            throw new NotImplementedException();
        }


        public async Task<ManageScheduleViewModel?> GetCompanyManageScheduleModel(Guid companyId, VacationSortFilter? sortFilter, int page, string userId)
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
            IQueryable<Vacation> selectedVacations = _unitOfWork
                .Vacations
                .All()
                .Include(v => v.Employee)
                .Where(v => v.CompanyId == companyId && v.Employee.UserId! == userId);

            //Calculate vacation days left before any filtering is done
            int currentYear = DateTime.Now.Year;
            int nextYear = DateTime.Now.AddYears(1).Year;

            int vacationDaysLeftForCurrentYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, currentYear, selectedVacations);
            int vacationDaysLeftForNextYear = await CalculateVacationDaysLeftForYear(entityCompany.MaxVacationDaysPerYear, nextYear, selectedVacations);

            //Filter vacations up to VacationSortFilter
            //Not checking for VacationSortFilter.All because when All we just show the results without sorting
            if (sortFilter.Value == VacationSortFilter.OnlyPending)
            {
                selectedVacations = selectedVacations
                    .Where(v => v.Status == VacationStatus.Pending);
            }
            else if (sortFilter.Value == VacationSortFilter.OnlyDenied)
            {
                selectedVacations = selectedVacations
                    .Where(v => v.Status == VacationStatus.Denied);
            }
            else if (sortFilter.Value == VacationSortFilter.OnlyApproved)
            {
                selectedVacations = selectedVacations
                    .Where(v => v.Status == VacationStatus.Approved);
            }

            //Calculate total vacations and pages
            int totalVacations = await selectedVacations.CountAsync();
            int totalPages = (int)Math.Ceiling(totalVacations / (double)ManageSchedulePageSize);

            List<VacationViewModel> vacationModels = await selectedVacations
                .Select(v => new VacationViewModel()
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
                .OrderBy(v => v.CreatedOn)
                .Skip((page - 1) * ManageSchedulePageSize)
                .Take(ManageSchedulePageSize)
                .AsNoTracking()
                .ToListAsync();

            return new ManageScheduleViewModel()
            {
                CompanyId = companyId,
                SortFilter = sortFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                Vacations = vacationModels,
                EmployeeId = entityEmployeeInfo.Id,
                VacationDaysLeftCurrentYear = vacationDaysLeftForCurrentYear,
                VacationDaysLeftNextYear = vacationDaysLeftForNextYear
            };
        }

        public Dictionary<int, int> CalculateVacationDaysYearSplit(DateTime startDate, DateTime endDate)
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

        private async Task<int> CalculateVacationDaysLeftForYear(int maxVacationDaysPerYear, int year, IQueryable<Vacation> selectedVacations)
        {
            int usedDays = await selectedVacations
                .Where(v => v.Status == VacationStatus.Pending || v.Status == VacationStatus.Approved) //Count the days only from approved or pending vacations
                .Where(v => v.StartDate.Year <= year && v.EndDate.Year >= year)
                .SumAsync(v =>
                    EF.Functions.DateDiffDay(
                        v.StartDate.Year < year ? new DateTime(year, 1, 1) : v.StartDate,
                        v.EndDate.Year > year ? new DateTime(year, 12, 31) : v.EndDate
                         ) + 1);

            return maxVacationDaysPerYear - usedDays;
        }

        private StatusReport AreVacationDatesValid(DateTime startDate, DateTime endDate)
        {
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

            return new StatusReport { Ok = true };
        }
    }
}
