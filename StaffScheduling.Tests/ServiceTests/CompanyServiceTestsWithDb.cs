using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Services.UserServices;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.CompanyService;

namespace StaffScheduling.Tests.ServiceTests
{
    public class CompanyServiceTestsWithDb
    {
        private ApplicationDbContext _dbContext;
        private IUnitOfWork _unitOfWork;
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

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.AddAsync(newEntity);
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

            var newEntities = new List<Company>();

            for (int i = 0; i < UserCreatedCompaniesLimit; i++)
            {
                var newEntity = new Company()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Company {i}",
                    OwnerId = userId,
                    MaxVacationDaysPerYear = 10
                };

                newEntities.Add(newEntity);
            }

            await _dbContext.AddRangeAsync(newEntities);
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
            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.AddAsync(newEntity);
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

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.AddAsync(newEntity);
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
            Assert.That(editedEntity.MaxVacationDaysPerYear, Is.EqualTo(model.MaxVacationDaysPerYear));
        }

        [Test]
        public async Task EditCompanyAsync_ShouldNotEditCompany_WhenCompanyWithSameNameExists()
        {
            //Arrange
            string userId = Guid.NewGuid().ToString();

            var newEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };


            var newEntityToBeEdited = new Company()
            {
                Id = Guid.NewGuid(),
                Name = "Company To Be Edited",
                OwnerId = userId,
                MaxVacationDaysPerYear = 10
            };

            await _dbContext.AddAsync(newEntity);
            await _dbContext.AddAsync(newEntityToBeEdited);
            await _dbContext.SaveChangesAsync();

            var model = new CompanyEditInputModel { Id = newEntityToBeEdited.Id, Name = newEntity.Name, MaxVacationDaysPerYear = 25 };

            //Act
            var result = await _companyService.EditCompanyAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(string.Format(CanNotEditCompanyWithSameNameFormat, newEntity.Name)));

            var companyEntityAfterEdit = await _dbContext.Companies.FindAsync(newEntityToBeEdited.Id);
            Assert.That(companyEntityAfterEdit!.Name, Is.EqualTo(newEntityToBeEdited.Name));
            Assert.That(companyEntityAfterEdit.MaxVacationDaysPerYear, Is.EqualTo(newEntityToBeEdited.MaxVacationDaysPerYear));
        }
    }
}
