namespace StaffScheduling.Common.ErrorMessages
{
    public static class ServiceErrorMessages
    {
        public const string DatabaseErrorFormat = "Database Error: {0}";

        public static class CompanyService
        {
            public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotDeleteCompany = "Couldn't delete company!";

            public const string CanNotEditCompanyWithSameNameFormat = "You can't save changes to company because you already have a company with the name '{0}'!";
            public const string CanNotCreateCompanyWithSameNameFormat = "You can't create company '{0}' because you already have a company with the same name!";
            public const string CreatedCompaniesLimitHitFormat = "You can't create company because you have already created {0} companies! Delete some if you want to create new ones!";
        }

        public static class EmployeeInfoService
        {
            public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindDepartment = "Couldn't find department!";
            /*            public const string CouldNotFindUser = "Couldn't find user!";*/
            public const string CouldNotFindEmployee = "Couldn't find employee!";
            //public const string CouldNotFindAnyEmployeesToDelete = "Couldn't find any employees to delete!";
            //public const string CouldNotFindUserEmail = "Couldn't find user's email!";
            public const string CouldNotJoinAlreadyJoinedCompany = "Couldn't join company because you have already joined!";
            public const string OwnerCouldNotHisJoinCompany = "You can't join the company because you are it's owner!";
            public const string CanNotManageEmployeeAsLowerPermission = "You can't manage an employee with the same or higher permission level than you!";
            public const string CanNotChangeEmployeeRoleToHigher = "You can't change an employee's role to one that has same or higher permission than your role!";
            public const string CanNotAddOwnerToTheEmployees = "You can't add the company's owner to the employee list!";

            public const string JoinedCompaniesLimitHitFormat = "You can't join the company because you have already joined {0} companies! Leave some if you want to join new ones!";
            public const string CouldNotFindEmployeeInfoFormat = "Couldn't find employee with email ({0}) in company database. Tell the company's admin to add you into the company's employee database!";
            public const string EmployeeWithEmailExistsFormat = "You can't add employee because one with such email ({0}) already exists!";
            public const string EmployeeLimitHitFormat = "You can't add more employees because you have hit the employee limit (Limit: {0})! Delete some if you want to create new ones!";
            public const string CanNotChangeEmployeeRoleWithoutDepartmentFormat = "You can't change an employee's role to '{0}' if he has no 'Department'! Add employee to 'Department' first!";
            public const string CanNotChangeEmployeeDepartmentToNoneBecauseOfRoleFormat = "You can't change an employee's department to 'None' when he has a role '{0}' that needs a department! Change employee role first!";
        }

        public static class DepartmentService
        {
            public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindDepartment = "Couldn't find department!";
            //public const string CouldNotFindAnyDepartmentsToDelete = "Couldn't find any departments to delete!";

            public const string DepartmentLimitHitFormat = "You can't add more departments because you have hit the department limit (Limit: {0})!";
            public const string DepartmentWithNameExistsFormat = "You can't add department because one with such name ({0}) already exists!";
            public const string DepartmentCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment = "You can't delete department because there are employees with roles that need to have a department! Delete those first! Roles that need departments are: {0}";
            public const string DepartmentsCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment = "You can't delete departments because in some departments there are employees with roles that need to have a department! Delete those employees first! Departments that have them are: {0}";
        }

        public static class VacationService
        {
            //Date errors
            public const string EndDateCanNotBeAfterStartDate = "End date must be after Start Date!";
            public const string StartDateCanNotBeTodayOrInThePast = "Start Date can't be today or in the past!";
            public const string DatesCanNotBeMoreThanXMonthsFromTomorrowFormat = "Start Date or End Date can't be more than {0} months away from tomorrow";
            public const string StartDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat = "You can't add vacation request with Start Date: '{0}' because there is already another one with same Start Or End Date with Status: '{1}'!";
            public const string EndDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat = "You can't add vacation request with End Date: '{0}' because there is already another one with same Start Or End Date with Status: '{1}'!";
            public const string CanNotAddVacationAsItIsInRangeOfAnotherVacation = "You can't add this vacation request because it is in the range of another one with Start Date: '{0}', End Date: '{1}' and Status: '{2}'!";

            //Other errors
            public const string CouldNotFindCompany = "Couldn't find company!";
            public const string CouldNotFindEmployee = "Couldn't find employee!";
            public const string CouldNotFindVacation = "Couldn't find vacation request!";
            public const string CanNotDeleteDeniedVacation = "You can't delete a vacation request which has status 'Denied'! Only higher-ups can delete it for you!";
            public const string CanNotDeleteAllDeniedVacations = "You can't delete vacation requests which have status 'Denied'! Only higher-ups can delete them for you!";
            public const string CanNotManageEmployeeVacationAsLowerPermission = "You can't manage an employee's vacation with the same or higher permission level than you!";
            public const string CanNotChangeVacationStatusToPending = "You can't change a vacation's status to 'Pending'!";
            public const string CanNotChangeApproveVacationThatHasAlreadyStarted = "You can't change status of a vacation request which has Start Date: Today or In The Past! You can only delete it!";

            public const string VacationPendingLimitHitFormat = "You can't add more vacation requests because you have hit the pending vacation request limit (Limit: {0})! Delete some if you want to create new ones or wait for a company admin to change the status of one!";
            public const string NotEnoughVacationDaysLeftFormat = "You can't add vacation request because you don't have enough vacation days left! Total vacation days needed: {0}";
            public const string VacationWithSameDatesExistsFormat = "You can't add vacation request because you have already made one with same Start Date ({0}) and same End Date ({1}) which has status '{2}'!";
        }
    }
}
