﻿namespace StaffScheduling.Common
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
            public const string FullNameRegexPattern = @"^\S*[A-Za-z]+\s[A-Za-z]+$";
        }

        public static class Company
        {
            public const int NameMinLength = 5;
            public const int NameMaxLength = 160;

            public const string NameRegexPattern = @"^[A-Z]([a-zA-Z0-9]|[\s\-@.,\'#&£$€¥])*$";
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

            public const string SiteName = "Staff Scheduling";
            public const string InviteLinkEmptyFormat = "{0}://{1}/Dashboard/Join/";
        }
    }
}