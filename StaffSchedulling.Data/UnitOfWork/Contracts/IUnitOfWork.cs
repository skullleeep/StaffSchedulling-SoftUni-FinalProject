using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;

namespace StaffScheduling.Data.UnitOfWork.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGuidRepository<Company> Companies { get; }
        IGuidRepository<Department> Departments { get; }
        IGuidRepository<EmployeeInfo> EmployeesInfo { get; }
        IGuidRepository<Vacation> Vacations { get; }
        Task<int> SaveChangesAsync();
    }

}
