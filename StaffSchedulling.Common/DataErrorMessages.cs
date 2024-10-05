namespace StaffScheduling.Common
{
    public static class DataErrorMessages
    {
        public static class ApplicationUser
        {
            public const string PasswordError = "Password can have: Minimum 8 characters, 1 lower case letter, 1 upper case letter, 1 number, 1 special character";
            public const string FullNameError = "Full Name can have: Minimum 5 characters, Maximum 128 characters, only english letters and at least 1 surname. e.g John Doe";
        }

        public static class Company
        {
            public const string NameError = "Company Name can have: Minimum 5 characters, Maximum 160 characters, should start with big letter and allowed symbols (- @ . , # & £ $ € ¥)";
        }
    }
}
