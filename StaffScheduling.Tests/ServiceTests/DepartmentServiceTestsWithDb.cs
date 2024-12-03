using Microsoft.EntityFrameworkCore;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Department;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.DepartmentService;

namespace StaffScheduling.Tests.ServiceTests
{
    public class DepartmentServiceTestsWithDb
    {
        private ApplicationDbContext _dbContext = null!;
        private IUnitOfWork _unitOfWork = null!;
        private Web.Services.DbServices.DepartmentService _departmentService;

        [SetUp]
        public void Setup()
        {
            //Initialize the InMemory DB and Mock UnitOfWork
            SetupForServices.InitializeInMemoryDbAndMockUnitOfWork(ref _dbContext, ref _unitOfWork);

            //Initialize the service with the UnitOfWork and UserManager
            _departmentService = new Web.Services.DbServices.DepartmentService(_unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted(); //Clean up after each test
            _dbContext.Dispose();

            _unitOfWork.Dispose();
        }

        [Test]
        public async Task AddDepartmentManuallyAsync_ShouldAddDepartment_WhenInputValid()
        {
            //Arrange
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            string newDepartmentName = "Test Department";

            var model = new AddDepartmentManuallyInputModel() { CompanyId = newCompanyEntity.Id, Name = newDepartmentName };


            //Act
            var result = await _departmentService.AddDepartmentManuallyAsync(model);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies
                .Include(c => c.Departments)
                .First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Departments.First().Name == model.Name);
        }

        [Test]
        public async Task AddDepartmentManuallyAsync_ShouldNotAddDepartment_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            string newDepartmentName = "Test Department";

            Guid randomCompanyId = Guid.NewGuid();

            var model = new AddDepartmentManuallyInputModel() { CompanyId = randomCompanyId, Name = newDepartmentName };

            //Act
            var result = await _departmentService.AddDepartmentManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies
                .Include(c => c.Departments)
                .First(c => c.Id == newCompanyEntity.Id);

            Assert.IsNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task AddDepartmentManuallyAsync_ShouldNotAddDepartment_WhenCompanyDepartmentLimitHit()
        {
            //Arrange
            Guid companyId = Guid.NewGuid();
            string companyName = "Test Company";
            string newDepartmentName = "Test Department";

            var departments = new List<Department>();
            for (int i = 0; i < CompanyDepatmentsLimit; i++)
            {
                departments.Add(new Department
                {
                    Id = Guid.NewGuid(),
                    Name = $"{newDepartmentName} {i}",
                    NormalizedName = $"{newDepartmentName} {i}".ToUpper(),
                    CompanyId = companyId
                });
            }

            var newCompanyEntity = new Company()
            {
                Id = companyId,
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = departments
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new AddDepartmentManuallyInputModel() { CompanyId = newCompanyEntity.Id, Name = newDepartmentName };

            //Act
            var result = await _departmentService.AddDepartmentManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(DepartmentLimitHitFormat, CompanyEmployeesLimit)));

            var foundCompanyEntity = _dbContext.Companies
                .Include(c => c.Departments)
                .First(c => c.Id == newCompanyEntity.Id);

            Assert.IsNull(foundCompanyEntity.Departments.FirstOrDefault(d => d.Name == newDepartmentName));
        }

        [Test]
        public async Task AddDepartmentManuallyAsync_ShouldNotAddDepartment_WhenDepartmentWithSameNameExists()
        {
            //Arrange
            string companyName = "Test Company";
            string newDepartmentName = "Test Department";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = newDepartmentName,
                        NormalizedName = newDepartmentName.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new AddDepartmentManuallyInputModel() { CompanyId = newCompanyEntity.Id, Name = newDepartmentName };

            //Act
            var result = await _departmentService.AddDepartmentManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(DepartmentWithNameExistsFormat, newDepartmentName)));

            var foundCompanyEntity = _dbContext.Companies
                .Include(c => c.Departments)
                .First(c => c.Id == newCompanyEntity.Id);

            Assert.That(foundCompanyEntity.Departments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteDepartmentAsync_ShouldDeleteDepartment_WhenInputValid()
        {
            //Arrange
            Guid departmentToDeleteId = Guid.NewGuid();

            string departmentToDeleteName = "Test Department";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = departmentToDeleteId,
                        Name = departmentToDeleteName,
                        NormalizedName = departmentToDeleteName.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteDepartmentInputModel() { CompanyId = newCompanyEntity.Id, DepartmentId = departmentToDeleteId };


            //Act
            var result = await _departmentService.DeleteDepartmentAsync(model);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task DeleteDepartmentAsync_ShouldNotDeleteDepartment_WhenCompanyNotFound()
        {
            //Arrange
            Guid departmentToDeleteId = Guid.NewGuid();

            string departmentToDeleteName = "Test Department";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = departmentToDeleteId,
                        Name = departmentToDeleteName,
                        NormalizedName = departmentToDeleteName.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteDepartmentInputModel() { CompanyId = randomCompanyId, DepartmentId = departmentToDeleteId };


            //Act
            var result = await _departmentService.DeleteDepartmentAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task DeleteDepartmentAsync_ShouldNotDeleteDepartment_WhenDepartmentNotFound()
        {
            //Arrange
            string departmentToDeleteName = "Test Department";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentToDeleteName,
                        NormalizedName = departmentToDeleteName.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomDepartmentId = Guid.NewGuid();

            var model = new DeleteDepartmentInputModel() { CompanyId = newCompanyEntity.Id, DepartmentId = randomDepartmentId };


            //Act
            var result = await _departmentService.DeleteDepartmentAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindDepartment));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task DeleteDepartmentAsync_ShouldNotDeleteDepartment_WhenDepartmentHasEmployeeWithRoleWhichNeedsDepartment()
        {
            //Arrange
            Guid departmentToDeleteId = Guid.NewGuid();

            string departmentToDeleteName = "Test Department";
            string companyName = "Test Company";

            string employeeEmail = "employee@email.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = departmentToDeleteId,
                        Name = departmentToDeleteName,
                        NormalizedName = departmentToDeleteName.ToUpper(),
                        DepartmentEmployeesInfo = new List<EmployeeInfo>()
                        {
                            new EmployeeInfo
                            {
                                Id = Guid.NewGuid(),
                                Email = employeeEmail,
                                NormalizedEmail = employeeEmail.ToUpper(),
                                Role = EmployeeRole.Supervisor,
                            }
                        }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteDepartmentInputModel() { CompanyId = newCompanyEntity.Id, DepartmentId = departmentToDeleteId };

            var rolesWhichNeedDepartment = GetRolesWhichNeedDepartment();

            //Act
            var result = await _departmentService.DeleteDepartmentAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(DepartmentCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment,
                    String.Join(", ", rolesWhichNeedDepartment.Select(role => $"'{role.ToString()}'")))));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task DeleteAllDepartmentsAsync_ShouldDeleteDepartments_WhenInputValid()
        {
            //Arrange
            string companyName = "Test Company";

            string departmentName1 = "Test Department 1";
            string departmentName2 = "Test Department 2";

            string employeeEmail = "employee@test.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName1,
                        NormalizedName = departmentName1.ToUpper(),
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName2,
                        NormalizedName = departmentName2.ToUpper(),
                        DepartmentEmployeesInfo = new List<EmployeeInfo>()
                        {
                            new EmployeeInfo
                            {
                                Id = Guid.NewGuid(),
                                Email = employeeEmail,
                                NormalizedEmail = employeeEmail.ToUpper(),
                                Role = EmployeeRole.Employee,
                            }
                        }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteAllDepartmentsInputModel() { CompanyId = newCompanyEntity.Id };


            //Act
            var result = await _departmentService.DeleteAllDepartmentsAsync(model);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNull(foundCompanyEntity.Departments.FirstOrDefault());
        }

        [Test]
        public async Task DeleteAllDepartmentsAsync_ShouldNotDeleteDepartments_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            string departmentName1 = "Test Department 1";
            string departmentName2 = "Test Department 2";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName1,
                        NormalizedName = departmentName1.ToUpper(),
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName2,
                        NormalizedName = departmentName2.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteAllDepartmentsInputModel() { CompanyId = randomCompanyId };


            //Act
            var result = await _departmentService.DeleteAllDepartmentsAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Departments.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteAllDepartmentsAsync_ShouldNotDeleteDepartments_WhenCompanyHasEmployeesWithRoleWhichNeedsDepartment()
        {
            //Arrange
            string companyName = "Test Company";

            string departmentName1 = "Test Department 1";
            string departmentName2 = "Test Department 2";

            string employeeEmail = "employee@test.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName1,
                        NormalizedName = departmentName1.ToUpper(),
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName2,
                        NormalizedName = departmentName2.ToUpper(),
                        DepartmentEmployeesInfo = new List<EmployeeInfo>()
                        {
                            new EmployeeInfo
                            {
                                Id = Guid.NewGuid(),
                                Email = employeeEmail,
                                NormalizedEmail = employeeEmail.ToUpper(),
                                Role = EmployeeRole.Supervisor,
                            }
                        }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteAllDepartmentsInputModel() { CompanyId = newCompanyEntity.Id };

            var rolesWhichNeedDepartment = GetRolesWhichNeedDepartment();


            //Act
            var result = await _departmentService.DeleteAllDepartmentsAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(DepartmentsCanNotBeDeletedBecauseOfEmployeesWhichNeedDepartment, $"'{departmentName2}'")));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Departments.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldReturnModelWithCorrectEmployees_WhenUserSearchesByEmail()
        {
            //Arrange
            string companyName = "Test Company";

            string departmentName1 = "Test Department 1";
            string departmentName2 = "Test Department 2";

            string employeeEmail = "employee@test.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName1,
                        NormalizedName = departmentName1.ToUpper(),
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName2,
                        NormalizedName = departmentName2.ToUpper(),
                        DepartmentEmployeesInfo = new List<EmployeeInfo>()
                        {
                            new EmployeeInfo
                            {
                                Id = Guid.NewGuid(),
                                Email = employeeEmail,
                                NormalizedEmail = employeeEmail.ToUpper(),
                                Role = EmployeeRole.Supervisor,
                            }
                        }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _departmentService.GetCompanyManageDepartmentsModel(newCompanyEntity.Id, 1);

            //Assert
            Assert.IsNotNull(model);

            Assert.That(model.Departments.Count, Is.EqualTo(2));
            Assert.That(model.Departments.First().EmployeeCount == 0);
            Assert.That(model.Departments.Last().EmployeeCount == 1);
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldNotReturnModel_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            string departmentName1 = "Test Department 1";
            string departmentName2 = "Test Department 2";

            string employeeEmail = "employee@test.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName1,
                        NormalizedName = departmentName1.ToUpper(),
                    },
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName2,
                        NormalizedName = departmentName2.ToUpper(),
                        DepartmentEmployeesInfo = new List<EmployeeInfo>()
                        {
                            new EmployeeInfo
                            {
                                Id = Guid.NewGuid(),
                                Email = employeeEmail,
                                NormalizedEmail = employeeEmail.ToUpper(),
                                Role = EmployeeRole.Supervisor,
                            }
                        }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            //Act
            var model = await _departmentService.GetCompanyManageDepartmentsModel(randomCompanyId, 1);

            //Assert
            Assert.IsNull(model);
        }
    }
}
