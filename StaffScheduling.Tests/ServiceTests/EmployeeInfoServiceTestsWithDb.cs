using Moq;
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddRangeAsync(newCompanyEntities);
            await _dbContext.AddAsync(newCompanyToBeJoinedEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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

            await _dbContext.AddAsync(newCompanyEntity);
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
    }
}
