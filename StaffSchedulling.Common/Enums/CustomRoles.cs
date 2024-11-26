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
            Manager = 2,
            Editor = 3,
            Owner = 4
        }

        //Used to 'Parse' EmployeeRole roles to PermissionRole roles
        public static readonly Dictionary<EmployeeRole, PermissionRole> RoleMapping = new()
        {
            { EmployeeRole.Employee, PermissionRole.Visitor },
            { EmployeeRole.Supervisor, PermissionRole.Manager },
            { EmployeeRole.Admin, PermissionRole.Editor }
        };

        //Get EmployeeRoles that user with some permission level can manage ( PermissionRoles < User's PermissionRole )
        public static List<EmployeeRole> GetManagableRoles(PermissionRole userPermissionRole)
        {
            return RoleMapping
                .Where(rm => rm.Value < userPermissionRole)
                .Select(rm => rm.Key)
                .ToList();
        }

        //Get EmployeeRoles that can access page up to needed PermissionRole
        public static List<EmployeeRole> GetRolesWithAccess(PermissionRole userPermissionRole)
        {
            return RoleMapping
                .Where(rm => rm.Value >= userPermissionRole)
                .Select(rm => rm.Key)
                .ToList();
        }
    }
}
