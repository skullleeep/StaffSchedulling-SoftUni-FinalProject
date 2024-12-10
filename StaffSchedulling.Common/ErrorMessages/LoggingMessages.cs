namespace StaffScheduling.Common.ErrorMessages
{
    public static class LoggingMessages
    {
        public static class VacationCleanupJob
        {
            public const string StartedFormat = "VacationCleanupJob started at {0}!";
            public const string DeletedVacationRequests = "Deleted {0} outdated vacation requests with Status: '{1}'!";
            public const string ChangedStatusToDenied = "Change Status of {0} vacation requests from 'Pending' to 'Denied'!";
            public const string ErrorOccured = "Error occurred while executing VacationCleanupJob at {0}";
            public const string Completed = "VacationCleanupJob completed successfully at {0}!";
        }
    }
}
