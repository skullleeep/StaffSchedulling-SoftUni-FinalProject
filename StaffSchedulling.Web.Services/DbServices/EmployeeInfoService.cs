using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Services.DbServices
{
    public class EmployeeInfoService(ApplicationDbContext _dbContext) : IEmployeeInfoService
    {
        public async Task<List<int>> GetJoinedCompanyIdsFromEmailAsync(string email)
        {
            return await _dbContext.EmployeesInfo
                .Where(e => e.Email == email && e.HasJoined == true)
                .Select(e => e.CompanyId)
                .ToListAsync();
        }
    }
}
