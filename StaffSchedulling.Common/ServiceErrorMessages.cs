namespace StaffScheduling.Common
{
    public static class ServiceErrorMessages
    {
        public static class EmployeeInfoService
        {
            //public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindUser = "Couldn't find user!";
            public const string CouldNotFindUserEmail = "Couldn't find user's email!";
            public const string CouldNotJoinAlreadyJoinedCompany = "Couldn't join company because you have already joined!";
            public const string OwnerCouldNotHisJoinCompany = "You can't join the company because you are it's owner!";

            public const string CouldNotFindEmployeeInfoFormat = "Couldn't find employee with email ({0}) in company database. Tell the company's admin to add you into the company's employee database!";
            public const string DatabaseErrorFormat = "Database Error: {0}";
        }
    }
}
