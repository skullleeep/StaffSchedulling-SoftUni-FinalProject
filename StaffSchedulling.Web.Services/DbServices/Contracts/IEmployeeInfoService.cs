namespace StaffScheduling.Web.Services.DbServices.Contracts
{
    public interface IEmployeeInfoService
    {
        public Task<List<int>> GetJoinedCompanyIdsFromEmailAsync(string email);
    }
}
