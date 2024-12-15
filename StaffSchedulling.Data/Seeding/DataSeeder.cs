using Microsoft.AspNetCore.Identity;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.Models;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Data.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedData(ApplicationDbContext _context, UserManager<ApplicationUser> _userManager)
        {
            if (!_context.Users.Any())
            {
                var user1 = new ApplicationUser
                {
                    UserName = "alexandros@staff.com",
                    Email = "alexandros@staff.com",
                    FullName = "Alexandros Alexandros"
                };
                var user2 = new ApplicationUser
                {
                    UserName = "jane.doe@staff.com",
                    Email = "jane.doe@staff.com",
                    FullName = "Jane Doe"
                };

                await _userManager.CreateAsync(user1, "Password123!");
                await _userManager.CreateAsync(user2, "Password123!");

                //Create companies
                var company1 = new Company
                {
                    Name = "Tech Innovators",
                    OwnerId = user1.Id,
                    MaxVacationDaysPerYear = 20
                };
                var company2 = new Company
                {
                    Name = "Green Solutions",
                    OwnerId = user2.Id,
                    MaxVacationDaysPerYear = 15
                };

                _context.Companies.AddRange(company1, company2);

                //Create departments for company1
                var dept1 = new Department
                {
                    Name = "Engineering",
                    CompanyId = company1.Id
                };
                var dept2 = new Department
                {
                    Name = "HR",
                    CompanyId = company1.Id
                };

                //Create departments for company2
                var dept3 = new Department
                {
                    Name = "Research",
                    CompanyId = company2.Id
                };

                _context.Departments.AddRange(dept1, dept2, dept3);

                //Create employee info for company1
                var employee1 = new EmployeeInfo
                {
                    Email = user2.Email,
                    Role = EmployeeRole.Employee,
                    HasJoined = true,
                    CompanyId = company1.Id,
                    DepartmentId = dept1.Id,
                    UserId = user2.Id
                };

                var employee2 = new EmployeeInfo
                {
                    Email = "employee2@tech.com",
                    Role = EmployeeRole.Employee,
                    HasJoined = false,
                    CompanyId = company1.Id,
                    DepartmentId = dept2.Id
                };

                //Create employee info for company2
                var employee3 = new EmployeeInfo
                {
                    Email = user1.Email,
                    Role = EmployeeRole.Employee,
                    HasJoined = true,
                    CompanyId = company2.Id,
                    DepartmentId = dept3.Id,
                    UserId = user1.Id
                };

                _context.EmployeesInfo.AddRange(employee1, employee2, employee3);

                //Create vacation requests for employee1
                var vacation1 = new Vacation
                {
                    EmployeeId = employee1.Id,
                    CompanyId = company1.Id,
                    StartDate = DateTime.Now.AddDays(5),
                    EndDate = DateTime.Now.AddDays(10),
                    CreatedOn = DateTime.Now,
                    Days = 5,
                    Status = VacationStatus.Pending
                };

                var vacation2 = new Vacation
                {
                    EmployeeId = employee2.Id,
                    CompanyId = company1.Id,
                    StartDate = DateTime.Now.AddDays(15),
                    EndDate = DateTime.Now.AddDays(20),
                    CreatedOn = DateTime.Now,
                    Days = 5,
                    Status = VacationStatus.Pending
                };

                _context.Vacations.AddRange(vacation1, vacation2);

                await _context.SaveChangesAsync();
            }
        }
    }

}
