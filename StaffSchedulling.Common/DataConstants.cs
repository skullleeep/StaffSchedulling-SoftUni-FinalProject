namespace StaffSchedulling.Common
{
    public static class DataConstants
    {
        public static class ApplicationUser
        {
            public const int EmailMinLength = 5;
            public const int EmailMaxLength = 320;

            public const int FullNameMinLength = 3;
            public const int FullNameMaxLength = 128;

            public const string PasswordRegexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";
        }

        public static class Department
        {
            public const int NameMinLength = 3;
            public const int NameMaxLength = 50;
        }

        public static class EmployeeInfo
        {
            public const int EmailMinLength = 5;
            public const int EmailMaxLength = 320;

            public const int FullNameMinLength = 3;
            public const int FullNameMaxLength = 128;
        }

        public class Web
        {
            public const string DefaultAdminEmail = "admin@admin.com";
            public const string DefaultAdminPassword = "Test1234!";
        }
    }
}
