using Moq;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.EmployeeInfo;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.EmployeeInfoService;

namespace StaffScheduling.Tests.ServiceTests
{
    [TestFixture]
    public class EmployeeInfoServiceTestsWithDb
    {
        private ApplicationDbContext _dbContext = null!;
        private IUnitOfWork _unitOfWork = null!;
        private ApplicationUserManager _userManager;
        private Web.Services.DbServices.EmployeeInfoService _employeeInfoService;

        [SetUp]
        public void Setup()
        {
            //Initialize the InMemory DB and Mock UnitOfWork
            SetupForServices.InitializeInMemoryDbAndMockUnitOfWork(ref _dbContext, ref _unitOfWork);

            //Mock UserManager
            _userManager = SetupForServices.MockUserManager();

            //Initialize the service with the UnitOfWork and UserManager
            _employeeInfoService = new Web.Services.DbServices.EmployeeInfoService(_unitOfWork, _userManager);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted(); //Clean up after each test
            _dbContext.Dispose();

            _unitOfWork.Dispose();

            _userManager.Dispose();
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldJoinCompany_WhenInputValid()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = "joining@user.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = userToJoinEmail,
                        NormalizedEmail = userToJoinEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = false
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);


            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(newCompanyEntity.Id, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsTrue(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldNotJoinCompany_WhenCompanyNotFound()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = "joining@user.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = userToJoinEmail,
                        NormalizedEmail = userToJoinEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = false
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);

            Guid randomCompanyId = Guid.NewGuid();

            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(randomCompanyId, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsFalse(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldNotJoinCompany_WhenUserTryingToJoinIsOwner()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = ownerEmail;
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = userToJoinEmail,
                        NormalizedEmail = userToJoinEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = false
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);


            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(newCompanyEntity.Id, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(OwnerCouldNotHisJoinCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsFalse(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldNotJoinCompany_WhenUserJoinedCompaniesLimitHasBeenHit()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = "joining@user.com";
            string companyName = "Test Company";

            var newCompanyEntities = new List<Company>();
            for (int i = 0; i < UserJoinedCompaniesLimit; i++)
            {
                var newCompanyEntity = new Company()
                {
                    Id = Guid.NewGuid(),
                    Name = $"{companyName} {i}",
                    NormalizedName = $"{companyName} {i}".ToUpper(),
                    OwnerId = Guid.NewGuid().ToString(),
                    MaxVacationDaysPerYear = 10,
                    CompanyEmployeesInfo = new List<EmployeeInfo>()
                    {
                        new EmployeeInfo
                        {
                            Email = userToJoinEmail,
                            NormalizedEmail = userToJoinEmail.ToUpper(),
                            Role = EmployeeRole.Employee,
                            HasJoined = true
                        }
                    }
                };

                newCompanyEntities.Add(newCompanyEntity);
            }

            var newCompanyToBeJoinedEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                    {
                        new EmployeeInfo
                        {
                            Email = userToJoinEmail,
                            NormalizedEmail = userToJoinEmail.ToUpper(),
                            Role = EmployeeRole.Employee,
                            HasJoined = false
                        }
                    }
            };

            await _dbContext.Companies.AddRangeAsync(newCompanyEntities);
            await _dbContext.Companies.AddAsync(newCompanyToBeJoinedEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyToBeJoinedEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);

            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(newCompanyToBeJoinedEntity.Id, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(JoinedCompaniesLimitHitFormat, UserJoinedCompaniesLimit)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyToBeJoinedEntity.Id);
            Assert.IsFalse(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldNotJoinCompany_WhenEmployeeInfoNotFound()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = "joining@user.com";
            string companyName = "Test Company";

            string randomEmail = "randomEmail@gmail.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = randomEmail,
                        NormalizedEmail = randomEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = false
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);


            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(newCompanyEntity.Id, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(CouldNotFindEmployeeInfoFormat, userToJoinEmail)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsFalse(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task JoinCompanyWithIdAsync_ShouldNotJoinCompany_WhenUserHasAlreadyJoined()
        {
            //Arrange
            string ownerEmail = "owner@example.com";

            string userToJoinId = Guid.NewGuid().ToString();
            string userToJoinEmail = "joining@user.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = userToJoinEmail,
                        NormalizedEmail = userToJoinEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = true
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Make it so that the mock userManager gets our fake email
            Mock.Get(_userManager)
                    .Setup(um => um.GetUserEmailFromIdAsync(newCompanyEntity.OwnerId))
                    .ReturnsAsync(ownerEmail);


            //Act
            var result = await _employeeInfoService.JoinCompanyWithIdAsync(newCompanyEntity.Id, userToJoinId, userToJoinEmail);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotJoinAlreadyJoinedCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsTrue(foundCompanyEntity.CompanyEmployeesInfo.First().HasJoined);
        }

        [Test]
        public async Task AddEmployeeManuallyAsync_ShouldAddEmployee_WhenInputValid()
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

            string newEmployeeEmail = "newemployee@test.com";

            var model = new AddEmployeeInfoManuallyInputModel() { CompanyId = newCompanyEntity.Id, Email = newEmployeeEmail };


            //Act
            var result = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Email == model.Email);
        }

        [Test]
        public async Task AddEmployeeManuallyAsync_ShouldNotAddEmployee_WhenCompanyNotFound()
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

            string newEmployeeEmail = "newemployee@test.com";
            Guid randomCompanyId = Guid.NewGuid();

            var model = new AddEmployeeInfoManuallyInputModel() { CompanyId = randomCompanyId, Email = newEmployeeEmail };


            //Act
            var result = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddEmployeeManuallyAsync_ShouldNotAddEmployee_WhenCompanyEmployeeLimitHit()
        {
            //Arrange
            string randomUserEmail = "random@user.email";

            var newEntities = new List<EmployeeInfo>();

            for (int i = 0; i < CompanyEmployeesLimit; i++)
            {
                newEntities.Add(new EmployeeInfo
                {
                    Email = randomUserEmail + i.ToString(),
                    NormalizedEmail = (randomUserEmail + i.ToString()).ToUpper(),
                    Role = EmployeeRole.Employee,
                    HasJoined = true
                });
            }

            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = newEntities
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            string newEmployeeEmail = "newemployee@test.com";

            var model = new AddEmployeeInfoManuallyInputModel() { CompanyId = newCompanyEntity.Id, Email = newEmployeeEmail };


            //Act
            var result = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(EmployeeLimitHitFormat, CompanyEmployeesLimit)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault(ef => ef.Email == newEmployeeEmail));
        }

        [Test]
        public async Task AddEmployeeManuallyAsync_ShouldNotAddEmployee_WhenEmployeeWithSameNameExists()
        {
            //Arrange
            string newEmployeeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Email = newEmployeeEmail,
                        NormalizedEmail = newEmployeeEmail.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = true
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new AddEmployeeInfoManuallyInputModel() { CompanyId = newCompanyEntity.Id, Email = newEmployeeEmail };


            //Act
            var result = await _employeeInfoService.AddEmployeeManuallyAsync(model);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(EmployeeWithEmailExistsFormat, model.Email.ToLower())));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteEmployeeAsync_ShouldDeleteEmployee_WhenInputValid()
        {
            //Arrange
            Guid employeeToDeleteId = Guid.NewGuid();

            string employeeToDeleteEmail = "newemployee@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToDeleteId,
                        Email = employeeToDeleteEmail,
                        NormalizedEmail = employeeToDeleteEmail.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteEmployeeInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToDeleteId };


            //Act
            var result = await _employeeInfoService.DeleteEmployeeAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault());
        }

        [Test]
        public async Task DeleteEmployeeAsync_ShouldNotDeleteEmployee_WhenCompanyNotFound()
        {
            //Arrange
            Guid employeeToDeleteId = Guid.NewGuid();

            string employeeToDeleteEmail = "newemployee@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToDeleteId,
                        Email = employeeToDeleteEmail,
                        NormalizedEmail = employeeToDeleteEmail.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteEmployeeInputModel() { CompanyId = randomCompanyId, EmployeeId = employeeToDeleteId };


            //Act
            var result = await _employeeInfoService.DeleteEmployeeAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault());
        }

        [Test]
        public async Task DeleteEmployeeAsync_ShouldNotDeleteEmployee_WhenEmployeeNotFound()
        {
            //Arrange
            Guid employeeToDeleteId = Guid.NewGuid();

            string employeeToDeleteEmail = "newemployee@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToDeleteId,
                        Email = employeeToDeleteEmail,
                        NormalizedEmail = employeeToDeleteEmail.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomEmployeeId = Guid.NewGuid();

            var model = new DeleteEmployeeInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = randomEmployeeId };


            //Act
            var result = await _employeeInfoService.DeleteEmployeeAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault());
        }

        [Test]
        public async Task DeleteEmployeeAsync_ShouldNotDeleteEmployee_WhenUserDoesntHaveNeededPermission()
        {
            //Arrange
            Guid employeeToDeleteId = Guid.NewGuid();

            EmployeeRole adminRole = EmployeeRole.Admin;

            string employeeToDeleteEmail = "newemployee@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToDeleteId,
                        Email = employeeToDeleteEmail,
                        NormalizedEmail = employeeToDeleteEmail.ToUpper(),
                        Role = adminRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteEmployeeInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToDeleteId };


            //Act
            var result = await _employeeInfoService.DeleteEmployeeAsync(model, RoleMapping[adminRole]);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotManageEmployeeAsLowerPermission));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNotNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault());
        }

        [Test]
        public async Task DeleteAllEmployeesAsync_ShouldDeleteEmployees_WhenInputValid()
        {
            //Arrange
            string companyName = "Test Company";

            string employeeEmail1 = "employee@email.com";
            string employeeEmail2 = "employee2@email.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteAllEmployeesInputModel() { CompanyId = newCompanyEntity.Id };


            //Act
            var result = await _employeeInfoService.DeleteAllEmployeesAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.IsNull(foundCompanyEntity.CompanyEmployeesInfo.FirstOrDefault());
        }

        [Test]
        public async Task DeleteAllEmployeesAsync_ShouldOnlyDeleteAllowedByPermissionEmployees_WhenUserPermissionIsNotOwner()
        {
            //Arrange
            string companyName = "Test Company";

            string employeeEmail1 = "employee@email.com";
            string employeeEmail2 = "employee2@email.com";

            EmployeeRole adminRole = EmployeeRole.Admin;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = adminRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new DeleteAllEmployeesInputModel() { CompanyId = newCompanyEntity.Id };


            //Act
            var result = await _employeeInfoService.DeleteAllEmployeesAsync(model, RoleMapping[adminRole]);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Email, Is.EqualTo(employeeEmail2));
        }

        [Test]
        public async Task DeleteAllEmployeesAsync_ShouldNotDeleteEmployees_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            string employeeEmail1 = "employee@email.com";
            string employeeEmail2 = "employee2@email.com";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteAllEmployeesInputModel() { CompanyId = randomCompanyId };


            //Act
            var result = await _employeeInfoService.DeleteAllEmployeesAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldChangeRole_WhenInputValid()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            EmployeeRole newRole = EmployeeRole.Admin;

            var model = new ChangeRoleInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, Role = newRole };


            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(newRole));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldNotChangeRole_WhenCompanyNotFound()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            EmployeeRole newRole = EmployeeRole.Admin;

            var model = new ChangeRoleInputModel() { CompanyId = randomCompanyId, EmployeeId = employeeToChangeId, Role = newRole };


            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(startingRole));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldNotChangeRole_WhenEmployeeNotFound()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomEmployeeId = Guid.NewGuid();

            EmployeeRole newRole = EmployeeRole.Admin;

            var model = new ChangeRoleInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = randomEmployeeId, Role = newRole };


            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(startingRole));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldNotChangeRole_WhenUserDoesntHaveRequiredPermissionToManageEmployee()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Admin;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            EmployeeRole newRole = EmployeeRole.Admin;

            var model = new ChangeRoleInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, Role = newRole };

            PermissionRole userPermissionRole = RoleMapping[startingRole];

            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, userPermissionRole);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotManageEmployeeAsLowerPermission));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(startingRole));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldNotChangeRole_WhenUserDoesntHaveRequiredPermissionToAssignThatRole()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            EmployeeRole newRole = EmployeeRole.Admin;

            var model = new ChangeRoleInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, Role = newRole };

            PermissionRole userPermissionRole = RoleMapping[newRole];

            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, userPermissionRole);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotChangeEmployeeRoleToHigher));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(startingRole));
        }

        [Test]
        public async Task ChangeRoleAsync_ShouldNotChangeRole_WhenNewRoleIsOneWhichNeedsDepartmentAndEmployeeHasNoAssignedDepartment()
        {
            //Arrange
            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            EmployeeRole newRole = GetRolesWhichNeedDepartment().First();

            var model = new ChangeRoleInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, Role = newRole };


            //Act
            var result = await _employeeInfoService.ChangeRoleAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(CanNotChangeEmployeeRoleWithoutDepartmentFormat, newRole.ToString())));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Role, Is.EqualTo(startingRole));
        }

        [Test]
        public async Task ChangeDepartmentAsync_ShouldChangeDepartment_WhenInputValid()
        {
            //Arrange
            Guid newCompanyId = Guid.NewGuid();

            string departmentName = "Test Department";

            var newDepartmentEntity = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentName,
                NormalizedName = departmentName.ToUpper()
            };

            string departmentToChangeToName = "Changed Department";

            var newDepartmentToChangeTo = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentToChangeToName,
                NormalizedName = departmentToChangeToName.ToUpper()
            };

            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole,
                        DepartmentId = newDepartmentEntity.Id
                    }
                },
                Departments = new List<Department>()
                {
                    newDepartmentEntity,
                    newDepartmentToChangeTo
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            var model = new ChangeDepartmentInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, SelectedDepartmentId = newDepartmentToChangeTo.Id };


            //Act
            var result = await _employeeInfoService.ChangeDepartmentAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().DepartmentId, Is.EqualTo(newDepartmentToChangeTo.Id));
        }

        [Test]
        public async Task ChangeDepartmentAsync_ShouldNotChangeDepartment_WhenCompanyNotFound()
        {
            //Arrange
            Guid newCompanyId = Guid.NewGuid();

            string departmentName = "Test Department";

            var newDepartmentEntity = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentName,
                NormalizedName = departmentName.ToUpper()
            };

            string departmentToChangeToName = "Changed Department";

            var newDepartmentToChangeTo = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentToChangeToName,
                NormalizedName = departmentToChangeToName.ToUpper()
            };

            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole,
                        DepartmentId = newDepartmentEntity.Id
                    }
                },
                Departments = new List<Department>()
                {
                    newDepartmentEntity,
                    newDepartmentToChangeTo
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new ChangeDepartmentInputModel() { CompanyId = randomCompanyId, EmployeeId = employeeToChangeId, SelectedDepartmentId = newDepartmentToChangeTo.Id };


            //Act
            var result = await _employeeInfoService.ChangeDepartmentAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().DepartmentId, Is.EqualTo(newDepartmentEntity.Id));
        }

        [Test]
        public async Task ChangeDepartmentAsync_ShouldNotChangeDepartment_WhenEmployeeNotFound()
        {
            //Arrange
            Guid newCompanyId = Guid.NewGuid();

            string departmentName = "Test Department";

            var newDepartmentEntity = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentName,
                NormalizedName = departmentName.ToUpper()
            };

            string departmentToChangeToName = "Changed Department";

            var newDepartmentToChangeTo = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentToChangeToName,
                NormalizedName = departmentToChangeToName.ToUpper()
            };

            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole,
                        DepartmentId = newDepartmentEntity.Id
                    }
                },
                Departments = new List<Department>()
                {
                    newDepartmentEntity,
                    newDepartmentToChangeTo
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomEmployeeId = Guid.NewGuid();

            var model = new ChangeDepartmentInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = randomEmployeeId, SelectedDepartmentId = newDepartmentToChangeTo.Id };


            //Act
            var result = await _employeeInfoService.ChangeDepartmentAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().DepartmentId, Is.EqualTo(newDepartmentEntity.Id));
        }

        [Test]
        public async Task ChangeDepartmentAsync_ShouldNotChangeDepartment_WhenDepartmentNotFound()
        {
            //Arrange
            Guid newCompanyId = Guid.NewGuid();

            string departmentName = "Test Department";

            var newDepartmentEntity = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentName,
                NormalizedName = departmentName.ToUpper()
            };

            string departmentToChangeToName = "Changed Department";

            var newDepartmentToChangeTo = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentToChangeToName,
                NormalizedName = departmentToChangeToName.ToUpper()
            };

            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Employee;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole,
                        DepartmentId = newDepartmentEntity.Id
                    }
                },
                Departments = new List<Department>()
                {
                    newDepartmentEntity,
                    newDepartmentToChangeTo
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomDepartmentId = Guid.NewGuid();

            var model = new ChangeDepartmentInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, SelectedDepartmentId = randomDepartmentId };


            //Act
            var result = await _employeeInfoService.ChangeDepartmentAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindDepartment));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().DepartmentId, Is.EqualTo(newDepartmentEntity.Id));
        }

        [Test]
        public async Task ChangeDepartmentAsync_ShouldNotChangeDepartment_WhenNewDepartmentIsNoneAndEmployeeHasRoleWhichNeedsDepartment()
        {
            //Arrange
            Guid newCompanyId = Guid.NewGuid();

            string departmentName = "Test Department";

            var newDepartmentEntity = new Department()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyId,
                Name = departmentName,
                NormalizedName = departmentName.ToUpper()
            };

            Guid employeeToChangeId = Guid.NewGuid();

            string employeeToChangeEmail = "newemployee@test.com";
            string companyName = "Test Company";

            EmployeeRole startingRole = EmployeeRole.Supervisor;

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = employeeToChangeId,
                        Email = employeeToChangeEmail,
                        NormalizedEmail = employeeToChangeEmail.ToUpper(),
                        Role = startingRole,
                        DepartmentId = newDepartmentEntity.Id
                    }
                },
                Departments = new List<Department>()
                {
                    newDepartmentEntity
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid noneDepartment = Guid.Empty;

            var model = new ChangeDepartmentInputModel() { CompanyId = newCompanyEntity.Id, EmployeeId = employeeToChangeId, SelectedDepartmentId = noneDepartment };


            //Act
            var result = await _employeeInfoService.ChangeDepartmentAsync(model, PermissionRole.Owner);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(CanNotChangeEmployeeDepartmentToNoneBecauseOfRoleFormat, newCompanyEntity.CompanyEmployeesInfo.First().Role)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().DepartmentId, Is.EqualTo(newDepartmentEntity.Id));
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldReturnModel_WhenInputValid()
        {
            //Arrange
            string employeeEmail1 = "employee1@test.com";
            string employeeEmail2 = "employee2@test.com";
            string departmentName = "Test Department";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = EmployeeRole.Admin
                    }
                },
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName,
                        NormalizedName = departmentName.ToUpper()
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(newCompanyEntity.Id, null, null, 1, PermissionRole.Owner);

            //Assert
            Assert.IsNotNull(model);

            Assert.That(model.Employees.Count, Is.EqualTo(2));
            Assert.That(model.Employees.First().Email == employeeEmail1);
            Assert.That(model.Employees.Last().Email == employeeEmail2);

            Assert.That(model.Departments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldReturnModelWithEmployeesAllowedToManage_WhenInputValidAndUserPermissionNotOwner()
        {
            //Arrange
            string employeeEmail1 = "employee1@test.com";
            string employeeEmail2 = "employee2@test.com";
            string departmentName = "Test Department";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = EmployeeRole.Admin
                    }
                },
                Departments = new List<Department>()
                {
                    new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = departmentName,
                        NormalizedName = departmentName.ToUpper()
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(newCompanyEntity.Id, null, null, 1, RoleMapping[EmployeeRole.Admin]);

            //Assert
            Assert.IsNotNull(model);

            Assert.That(model.Employees.Count, Is.EqualTo(1));
            Assert.That(model.Employees.First().Email == employeeEmail1);

            Assert.That(model.Departments.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldReturnModelWithCorrectEmployees_WhenUserSearchesByEmail()
        {
            //Arrange
            string employeeEmail1 = "employee1@test.com";
            string employeeEmail2 = "employee2@test.com";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = EmployeeRole.Admin
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            string searchQuery = "employee2";
            EmployeeSearchFilter searchFilter = EmployeeSearchFilter.Email;

            //Act
            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(newCompanyEntity.Id, searchQuery, searchFilter, 1, PermissionRole.Owner);

            //Assert
            Assert.IsNotNull(model);

            Assert.That(model.Employees.Count, Is.EqualTo(1));
            Assert.That(model.Employees.First().Email == employeeEmail2);
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldReturnModelWithCorrectEmployees_WhenUserSearchesByName()
        {
            //Arrange
            string employeeEmail1 = "employee1@test.com";
            string employeeEmail2 = "employee2@test.com";
            string userName1 = "John Doe";
            string userName2 = "Vanka Cvetanev";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = true,
                        User = new ApplicationUser { FullName = userName1 }
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = EmployeeRole.Admin,
                        HasJoined = true,
                        User = new ApplicationUser { FullName = userName2 }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            string searchQuery = "cvetanev";
            EmployeeSearchFilter searchFilter = EmployeeSearchFilter.Name;

            //Act
            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(newCompanyEntity.Id, searchQuery, searchFilter, 1, PermissionRole.Owner);

            //Assert
            Assert.IsNotNull(model);

            Assert.That(model.Employees.Count, Is.EqualTo(1));
            Assert.That(model.Employees.First().Name == userName2);
        }

        [Test]
        public async Task GetCompanyManageEmployeeInfoModel_ShouldNotReturnModel_WhenCompanyNotFound()
        {
            //Arrange
            string employeeEmail1 = "employee1@test.com";
            string employeeEmail2 = "employee2@test.com";
            string userName1 = "John Doe";
            string userName2 = "Vanka Cvetanev";
            string companyName = "Test Company";

            var newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail1,
                        NormalizedEmail = employeeEmail1.ToUpper(),
                        Role = EmployeeRole.Employee,
                        HasJoined = true,
                        User = new ApplicationUser { FullName = userName1 }
                    },
                    new EmployeeInfo
                    {
                        Id = Guid.NewGuid(),
                        Email = employeeEmail2,
                        NormalizedEmail = employeeEmail2.ToUpper(),
                        Role = EmployeeRole.Admin,
                        HasJoined = true,
                        User = new ApplicationUser { FullName = userName2 }
                    }
                }
            };

            await _dbContext.Companies.AddAsync(newCompanyEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            //Act
            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(randomCompanyId, null, null, 1, PermissionRole.Owner);

            //Assert
            Assert.IsNull(model);
        }
    }
}
