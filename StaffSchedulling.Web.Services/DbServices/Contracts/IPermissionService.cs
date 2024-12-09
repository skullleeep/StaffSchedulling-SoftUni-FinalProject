using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IPermissionService
    {
        Task<PermissionRole> GetUserPermissionInCompanyAsync(Guid companyId, string userEmail);

        Task<Guid?> GetUserNeededDepartmentId(Guid companyId, string userEmail);
    }

}
