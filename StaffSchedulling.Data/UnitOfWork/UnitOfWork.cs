using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;
using StaffScheduling.Data.UnitOfWork.Contracts;

namespace StaffScheduling.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IGenericRepository<Company, Guid> _companies;
        private readonly IGenericRepository<Department, Guid> _departments;
        private readonly IGenericRepository<EmployeeInfo, Guid> _employeesInfo;
        private readonly IGenericRepository<Vacation, Guid> _vacations;
        private readonly ApplicationDbContext _context;

        // Inject repositories into the constructor
        public UnitOfWork(
            IGenericRepository<Company, Guid> companies,
            IGenericRepository<Department, Guid> departments,
            IGenericRepository<EmployeeInfo, Guid> employeesInfo,
            IGenericRepository<Vacation, Guid> vacations,
            ApplicationDbContext context)
        {
            _companies = companies;
            _departments = departments;
            _employeesInfo = employeesInfo;
            _vacations = vacations;
            _context = context;
        }

        public IGenericRepository<Company, Guid> Companies => _companies;
        public IGenericRepository<Department, Guid> Departments => _departments;
        public IGenericRepository<EmployeeInfo, Guid> EmployeesInfo => _employeesInfo;
        public IGenericRepository<Vacation, Guid> Vacations => _vacations;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
