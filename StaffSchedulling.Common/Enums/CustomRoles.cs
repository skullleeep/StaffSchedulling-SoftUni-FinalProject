namespace StaffScheduling.Common.Enums
{
    public static class CustomRoles
    {
        public enum EmployeeRole
        {
            Employee = 0,
            Supervisor = 1,
            Admin = 2
        }

        public enum PermissionRole
        {
            None = 0,
            Visitor = 1,
            Editor = 2,
            Manager = 3,
            Owner = 4
        }

        //Used to 'Parse' EmployeeRole roles to PermissionRole roles
        public static readonly Dictionary<EmployeeRole, PermissionRole> RoleMapping = new()
        {
            { EmployeeRole.Employee, PermissionRole.Visitor },
            { EmployeeRole.Supervisor, PermissionRole.Editor },
            { EmployeeRole.Admin, PermissionRole.Manager }
        };
    }
}
