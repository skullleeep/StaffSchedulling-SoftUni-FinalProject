using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.UnitOfWork.Contracts;
using static StaffScheduling.Common.ErrorMessages.LoggingMessages.VacationCleanupJob;

namespace StaffScheduling.Jobs
{
    public class VacationCleanupJob(IUnitOfWork _unitOfWork, ILogger<VacationCleanupJob> _logger) : IJob
    {
        const int BatchSize = 100;

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation(StartedFormat, DateTime.UtcNow);

                //Change status of past pending requests to denied
                while (true)
                {
                    var outdatedPendingRequests = await _unitOfWork
                        .Vacations
                        .All()
                        .Where(v => v.StartDate <= DateTime.Today && v.Status == VacationStatus.Pending)
                        .Take(BatchSize)
                        .ToArrayAsync();

                    if (!outdatedPendingRequests.Any())
                    {
                        break;
                    }

                    foreach (var entity in outdatedPendingRequests)
                    {
                        entity.Status = VacationStatus.Denied;
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(DeletedVacationRequests, outdatedPendingRequests.Count(), VacationStatus.Denied.ToString());
                }

                //Delete all denied request which have StartDate more than 1 day in the past
                while (true)
                {
                    var outdatedDeniedRequests = await _unitOfWork
                        .Vacations
                        .All()
                        .Where(v => v.StartDate <= DateTime.Today.AddDays(-1) && v.Status == VacationStatus.Denied)
                        .Take(BatchSize)
                        .ToArrayAsync();

                    if (!outdatedDeniedRequests.Any())
                    {
                        break;
                    }

                    _unitOfWork.Vacations.DeleteRange(outdatedDeniedRequests);

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(DeletedVacationRequests, outdatedDeniedRequests.Count(), VacationStatus.Denied.ToString());
                }

                _logger.LogInformation(Completed, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorOccured, DateTime.UtcNow);
            }
        }
    }
}
