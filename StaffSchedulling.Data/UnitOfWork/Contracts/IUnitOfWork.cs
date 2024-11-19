using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;

namespace StaffScheduling.Data.UnitOfWork.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Company, Guid> Companies { get; }
        IGenericRepository<Department, Guid> Departments { get; }
        IGenericRepository<EmployeeInfo, Guid> EmployeesInfo { get; }
        IGenericRepository<Vacation, Guid> Vacations { get; }
        Task<int> SaveChangesAsync();
    }

}
