namespace StaffScheduling.Common
{
    public static class ServiceErrorMessages
    {
        public const string DatabaseErrorFormat = "Database Error: {0}";

        public static class EmployeeInfoService
        {
            //public const string CouldNotFindCompany = "Couldn't find company!";
            //public const string CouldNotFindUser = "Couldn't find user!";
            public const string CouldNotFindUserEmail = "Couldn't find user's email!";
            public const string CouldNotJoinAlreadyJoinedCompany = "Couldn't join company because you have already joined!";
            public const string OwnerCouldNotHisJoinCompany = "You can't join the company because you are it's owner!";

            public const string JoinedCompaniesLimitHitFormat = "You can't join the company because you have already joined {0} companies! Leave some if you want to join another!";
            public const string CouldNotFindEmployeeInfoFormat = "Couldn't find employee with email ({0}) in company database. Tell the company's admin to add you into the company's employee database!";
        }

        public static class CompanyService
        {
            public const string CanNotCreateCompanyWithSameNameFormat = "You can't create company '{0}' because you already have a company with the same name!";
        }
    }
}
