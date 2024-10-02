namespace StaffSchedulling.Common
{
    public static class DataErrorMessages
    {
        public static class ApplicationUser
        {
            public const string PasswordError = "Password should have: Minimum 8 characters, 1 lower case letter, 1 upper case letter, 1 number, 1 special character";
            public const string FullNameError = "Full Name should have: Minimum 5 characters, only english letters and at least 1 surname. e.g John Doe";
        }
    }
}
