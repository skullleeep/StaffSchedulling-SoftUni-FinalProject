namespace StaffScheduling.Common.ErrorMessages
{
    public static class ServiceErrorMessages
    {
        public const string DatabaseErrorFormat = "Database Error: {0}";

        public static class EmployeeInfoService
        {
            public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindUser = "Couldn't find user!";
            //public const string CouldNotFindUserEmail = "Couldn't find user's email!";
            public const string CouldNotJoinAlreadyJoinedCompany = "Couldn't join company because you have already joined!";
            public const string OwnerCouldNotHisJoinCompany = "You can't join the company because you are it's owner!";

            public const string JoinedCompaniesLimitHitFormat = "You can't join the company because you have already joined {0} companies! Leave some if you want to join new ones!";
            public const string CouldNotFindEmployeeInfoFormat = "Couldn't find employee with email ({0}) in company database. Tell the company's admin to add you into the company's employee database!";
            public const string EmployeeWithEmailExistsFormat = "You can't add employee because one with such email ({0}) already already exists!";
            public const string EmployeeLimitHitFormat = "You can't add more employees because you have hit the employee limit (Limit: {0})!";
        }

        public static class CompanyService
        {
            public const string CouldNotFindCompany = "Couldn't find company!";

            //public const string CouldNotDeleteDepartments = "Couldn't delete company's departments!";
            //public const string CouldNotDeleteVacations = "Couldn't delete company's vacations!";
            //public const string CouldNotDeleteEmployeesInfo = "Couldn't delete company's employees!";
            public const string CouldNotDeleteCompany = "Couldn't delete company!";

            public const string CanNotEditCompanyWithSameNameFormat = "You can't save change to company because you already have a company with the same name!";
            public const string CanNotCreateCompanyWithSameNameFormat = "You can't create company '{0}' because you already have a company with the same name!";
            public const string CreatedCompaniesLimitHitFormat = "You can't create company because you have already created {0} companies! Delete some if you want to create new ones!";
        }
    }
}
