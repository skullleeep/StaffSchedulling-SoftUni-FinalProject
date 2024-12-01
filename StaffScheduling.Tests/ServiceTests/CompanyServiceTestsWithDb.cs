using Moq;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.CompanyService;

namespace StaffScheduling.Tests.ServiceTests
{
    public class CompanyServiceTestsWithDb
    {
        private ApplicationDbContext _dbContext = null!;
        private IUnitOfWork _unitOfWork = null!;
        private ApplicationUserManager _userManager;
        private Web.Services.DbServices.CompanyService _companyService;

        [SetUp]
        public void Setup()
        {
            //Initialize the InMemory DB and Mock UnitOfWork
            SetupForServices.InitializeInMemoryDbAndMockUnitOfWork(ref _dbContext, ref _unitOfWork);

            //Mock UserManager
            _userManager = SetupForServices.MockUserManager();

            //Initialize the service with the UnitOfWork and UserManager
            _companyService = new Web.Services.DbServices.CompanyService(_unitOfWork, _userManager);
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
        public async Task CreateCompanyAsync_ShouldCreateCompany_WhenValidInput()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();
            var model = new CompanyCreateInputModel { Name = "Test Company", MaxVacationDaysPerYear = 20 };

            //Act
            var result = await _companyService.CreateCompanyAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var createdEntity = _dbContext.Companies.FirstOrDefault();
            Assert.IsNotNull(createdEntity);
            Assert.That(createdEntity.Name, Is.EqualTo("Test Company"));
            Assert.That(createdEntity.OwnerId, Is.EqualTo(userId));
        }

        [Test]
        public async Task CreateCompanyAsync_ShouldNotCreateCompany_WhenCompanyWithSameNameExists()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            var model = new CompanyCreateInputModel { Name = "Test Company", MaxVacationDaysPerYear = 20 };

            //Act
            var result = await _companyService.CreateCompanyAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(string.Format(CanNotCreateCompanyWithSameNameFormat, newEntity.Name)));
        }

        [Test]
        public async Task CreateCompanyAsync_ShouldNotCreateCompany_WhenCompanyLimitHasBeenHit()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();
            string companyName = "Test Company";

            var newEntities = new List<Company>();

            for (int i = 0; i < UserCreatedCompaniesLimit; i++)
            {
                var newEntity = new Company()
                {
                    Id = Guid.NewGuid(),
                    Name = companyName + i.ToString(),
                    NormalizedName = (companyName + i.ToString()).ToUpper(),
                    OwnerId = userId,
                    MaxVacationDaysPerYear = 10
                };

                newEntities.Add(newEntity);
            }

            await _dbContext.Companies.AddRangeAsync(newEntities);
            await _dbContext.SaveChangesAsync();

            var model = new CompanyCreateInputModel { Name = "Test Company", MaxVacationDaysPerYear = 20 };

            //Act
            var result = await _companyService.CreateCompanyAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(string.Format(CreatedCompaniesLimitHitFormat, UserCreatedCompaniesLimit)));
        }

        [Test]
        public async Task DeleteCompanyAsync_ShouldDeleteCompany_WhenCompanyExists()
        {
            //Arrange
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var result = await _companyService.DeleteCompanyAsync(newEntity.Id);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var createdEntity = _dbContext.Companies.FirstOrDefault();
            Assert.IsNull(createdEntity);
        }

        [Test]
        public async Task DeleteCompanyAsync_ShouldNotDeleteCompany_WhenCompanyDoesNotExist()
        {
            //Arrange
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            Guid notExistingCompanyId = Guid.NewGuid();

            //Act
            var result = await _companyService.DeleteCompanyAsync(notExistingCompanyId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany)); ;
        }

        [Test]
        public async Task EditCompanyAsync_ShouldEditCompany_WhenValidInput()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            var model = new CompanyEditInputModel { Id = newEntity.Id, Name = "Test Company Edited", MaxVacationDaysPerYear = 25 };

            //Act
            var result = await _companyService.EditCompanyAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var editedEntity = _dbContext.Companies.FirstOrDefault();
            Assert.IsNotNull(editedEntity);
            Assert.That(editedEntity.Name, Is.EqualTo(model.Name));
            Assert.That(editedEntity.NormalizedName, Is.EqualTo(model.Name.ToUpper()));
            Assert.That(editedEntity.MaxVacationDaysPerYear, Is.EqualTo(model.MaxVacationDaysPerYear));
        }

        [Test]
        public async Task EditCompanyAsync_ShouldNotEditCompany_WhenCompanyWithSameNameExists()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };


            string companyToBeEditedName = "Company To Be Edited";
            var newEntityToBeEdited = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyToBeEditedName,
                NormalizedName = companyToBeEditedName.ToUpper(),
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddRangeAsync(newEntity, newEntityToBeEdited);
            await _dbContext.SaveChangesAsync();

            var model = new CompanyEditInputModel { Id = newEntityToBeEdited.Id, Name = newEntity.Name, MaxVacationDaysPerYear = 25 };

            //Act
            var result = await _companyService.EditCompanyAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(string.Format(CanNotEditCompanyWithSameNameFormat, newEntity.Name)));

            var companyEntityAfterEdit = await _dbContext.Companies.FindAsync(newEntityToBeEdited.Id);
            Assert.That(companyEntityAfterEdit!.Name, Is.EqualTo(newEntityToBeEdited.Name));
            Assert.That(companyEntityAfterEdit!.NormalizedName, Is.EqualTo(newEntityToBeEdited.Name.ToUpper()));
            Assert.That(companyEntityAfterEdit.MaxVacationDaysPerYear, Is.EqualTo(newEntityToBeEdited.MaxVacationDaysPerYear));
        }

        [Test]
        public async Task GetCompanyFromInviteLinkAsync_ShouldReturnCompany_WhenValidInput()
        {
            //Arrange
            Guid invite = Guid.NewGuid();
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = invite,
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _companyService.GetCompanyFromInviteLinkAsync(invite);

            //Assert
            Assert.IsNotNull(model);
            Assert.That(model.Id, Is.EqualTo(newEntity.Id));
        }

        [Test]
        public async Task GetCompanyFromInviteLinkAsync_ShouldReturnNull_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomInvite = Guid.NewGuid();

            //Act
            var model = await _companyService.GetCompanyFromInviteLinkAsync(randomInvite);

            //Assert
            Assert.IsNull(model);
        }

        [Test]
        public async Task GetOwnedAndJoinedCompaniesFromUserEmailAsync_ShouldReturnCorrectCompanies_WhenInputValid()
        {
            //Arrange
            string email = "test@example.com";
            Guid ownedCompanyId = Guid.NewGuid();
            Guid joinedCompanyId = Guid.NewGuid();

            //Make it so that the mock userManager gets our newly made Ids
            Mock.Get(_userManager)
                    .Setup(um => um.GetOwnedCompanyIdsFromUserEmailAsync(email))
                    .ReturnsAsync(new List<Guid> { ownedCompanyId });

            Mock.Get(_userManager)
                .Setup(um => um.GetJoinedCompanyIdsFromUserEmailAsync(email))
                .ReturnsAsync(new List<Guid> { joinedCompanyId });

            string ownedCompanyName = "Owned Company";

            var entityOwned = new Company
            {
                Id = ownedCompanyId,
                Name = ownedCompanyName,
                NormalizedName = ownedCompanyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString()
            };

            string joinedCompanyName = "Joined Company";

            var entityJoined = new Company
            {
                Id = joinedCompanyId,
                Name = joinedCompanyName,
                NormalizedName = joinedCompanyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                CompanyEmployeesInfo = new List<EmployeeInfo>
                {
                    new EmployeeInfo
                    {
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        Role = EmployeeRole.Employee
                    }
                }
            };

            string randomCompanyName = "Test Company";

            var randomEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = randomCompanyName,
                NormalizedName = randomCompanyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
            };

            _dbContext.Companies.AddRange(entityOwned, entityJoined, randomEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(email, null);

            //Assert
            Assert.That(model.OwnedCompanies.Count, Is.EqualTo(1));
            Assert.That(model.JoinedCompanies.Count, Is.EqualTo(1));
            Assert.That(model.OwnedCompanies.First().Name, Is.EqualTo(entityOwned.Name));
            Assert.That(model.JoinedCompanies.First().Name, Is.EqualTo(entityJoined.Name));
        }

        [Test]
        public async Task GetOwnedAndJoinedCompaniesFromUserEmailAsync_ShouldReturnEmptyCompaniesLists_WhenUserHasNotOwnedAndJoinedCompanies()
        {
            //Arrange
            string email = "test@example.com";
            string randomEmail = "randomemail@gmail.com";

            //Make it so that the mock userManager gets empty owned and joined lists
            Mock.Get(_userManager)
                    .Setup(um => um.GetOwnedCompanyIdsFromUserEmailAsync(email))
                    .ReturnsAsync(new List<Guid>());

            Mock.Get(_userManager)
                .Setup(um => um.GetJoinedCompanyIdsFromUserEmailAsync(email))
                .ReturnsAsync(new List<Guid>());

            string randomCompanyName1 = "Test Company 1";

            var randomEntity1 = new Company
            {
                Id = Guid.NewGuid(),
                Name = randomCompanyName1,
                NormalizedName = randomCompanyName1.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                CompanyEmployeesInfo = new List<EmployeeInfo>
                {
                    new EmployeeInfo
                    {
                        Email = randomEmail,
                        NormalizedEmail = randomEmail.ToUpper(),
                        Role = EmployeeRole.Admin
                    }
                }
            };

            string randomCompanyName2 = "Test Company 2";

            var randomEntity2 = new Company
            {
                Id = Guid.NewGuid(),
                Name = randomCompanyName2,
                NormalizedName = randomCompanyName2.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                CompanyEmployeesInfo = new List<EmployeeInfo>
                {
                    new EmployeeInfo
                    {
                        Email = randomEmail,
                        NormalizedEmail = randomEmail.ToUpper(),
                        Role = EmployeeRole.Employee
                    }
                }
            };

            _dbContext.Companies.AddRange(randomEntity1, randomEntity2);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _companyService.GetOwnedAndJoinedCompaniesFromUserEmailAsync(email, null);

            //Assert
            Assert.That(model.OwnedCompanies.Count, Is.EqualTo(0));
            Assert.That(model.JoinedCompanies.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetCompanyFromIdAsync_ShouldReturnCompany_WhenValidInput()
        {
            //Arrange
            Guid companyId = Guid.NewGuid();

            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = companyId,
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _companyService.GetCompanyFromIdAsync(companyId, true, true);

            //Assert
            Assert.IsNotNull(model);
            Assert.That(model.Id, Is.EqualTo(newEntity.Id));
        }

        [Test]
        public async Task GetCompanyFromIdAsync_ShouldReturnNull_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomId = Guid.NewGuid();

            //Act
            var model = await _companyService.GetCompanyFromIdAsync(randomId, true, true);

            //Assert
            Assert.IsNull(model);
        }

        [Test]
        public async Task GetCompanyEditInputModelAsync_ShouldReturnCompany_WhenValidInput()
        {
            //Arrange
            Guid companyId = Guid.NewGuid();

            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = companyId,
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            //Act
            var model = await _companyService.GetCompanyEditInputModelAsync(companyId);

            //Assert
            Assert.IsNotNull(model);
            Assert.That(model.Id, Is.EqualTo(newEntity.Id));
        }

        [Test]
        public async Task GetCompanyEditInputModelAsync_ShouldReturnNull_WhenCompanyNotFound()
        {
            //Arrange
            string companyName = "Test Company";

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                Invite = Guid.NewGuid(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.Companies.AddAsync(newEntity);
            await _dbContext.SaveChangesAsync();

            Guid randomId = Guid.NewGuid();

            //Act
            var model = await _companyService.GetCompanyEditInputModelAsync(randomId);

            //Assert
            Assert.IsNull(model);
        }

    }
}
