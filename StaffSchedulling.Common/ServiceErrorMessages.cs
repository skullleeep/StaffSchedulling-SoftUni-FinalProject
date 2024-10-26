namespace StaffScheduling.Common
{
    public static class ServiceErrorMessages
    {
        public static class EmployeeInfoService
        {
            //public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindUser = "Couldn't find user!";
            public const string CouldNotFindEmployeeInfoFormat = "Couldn't find employee with email ({0}) in company database. Tell the company's admin to add you into the company's employee database!";

            public const string DatabaseErrorFormat = "Database Error: {0}";
        }
    }
}
