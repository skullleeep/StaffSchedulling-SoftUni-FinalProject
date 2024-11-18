using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository.Contracts;
using StaffScheduling.Data.UnitOfWork.Contracts;

namespace StaffScheduling.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IGuidRepository<Company> _companies;
        private readonly IGuidRepository<Department> _departments;
        private readonly IGuidRepository<EmployeeInfo> _employeesInfo;
        private readonly IGuidRepository<Vacation> _vacations;
        private readonly ApplicationDbContext _context;

        // Inject repositories into the constructor
        public UnitOfWork(
            IGuidRepository<Company> companies,
            IGuidRepository<Department> departments,
            IGuidRepository<EmployeeInfo> employeesInfo,
            IGuidRepository<Vacation> vacations,
            ApplicationDbContext context)
        {
            _companies = companies;
            _departments = departments;
            _employeesInfo = employeesInfo;
            _vacations = vacations;
            _context = context;
        }

        public IGuidRepository<Company> Companies => _companies;
        public IGuidRepository<Department> Departments => _departments;
        public IGuidRepository<EmployeeInfo> EmployeesInfo => _employeesInfo;
        public IGuidRepository<Vacation> Vacations => _vacations;

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
