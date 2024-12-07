namespace StaffScheduling.Common.Constants
{
    public static class ApplicationConstants
    {
        public const string DefaultAdminEmail = "admin@admin.com";
        public const string DefaultAdminPassword = "Test1234!";

        public const string SiteName = "Staff Scheduling";
        public const string InviteLinkEmptyFormat = "{0}://{1}/Company/Join/";

        public const int ManageEmployeesPageSize = 10;
        public const int ManageDepartmentsPageSize = 10;
        public const int ManageSchedulePageSize = 10;

        public const int UserJoinedCompaniesLimit = 20;
        public const int UserCreatedCompaniesLimit = 20;

        public const int CompanyEmployeesLimit = 500;
        public const int CompanyDepatmentsLimit = 100;

        public const int VacationMaxMonthsFromDates = 6;
        public const string VacationDateFormat = "dd MMMM yyyy";
    }
}
