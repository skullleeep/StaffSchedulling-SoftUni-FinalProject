using StaffScheduling.Common.Enums;
using StaffScheduling.Common.ErrorMessages;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Models.InputModels.Vacation;
using static StaffScheduling.Common.Constants.ApplicationConstants;
using static StaffScheduling.Common.Enums.CustomRoles;
using static StaffScheduling.Common.ErrorMessages.ServiceErrorMessages.VacationService;

namespace StaffScheduling.Tests.ServiceTests
{
    [TestFixture]
    public class VacationServiceTestsWithDb
    {
        private ApplicationDbContext _dbContext = null!;
        private IUnitOfWork _unitOfWork = null!;
        private Web.Services.DbServices.VacationService _vacationService;

        private string companyName;
        private Guid employeeId;
        private string employeeEmail;
        private string userId;
        private Company newCompanyEntity;

        [SetUp]
        public void Setup()
        {
            //Initialize the InMemory DB and Mock UnitOfWork
            SetupForServices.InitializeInMemoryDbAndMockUnitOfWork(ref _dbContext, ref _unitOfWork);

            //Initialize the service with the UnitOfWork and UserManager
            _vacationService = new Web.Services.DbServices.VacationService(_unitOfWork);

            //Initialize some entities
            companyName = "Test Company";

            employeeId = Guid.NewGuid();
            employeeEmail = "employee@test.com";

            userId = Guid.NewGuid().ToString();

            newCompanyEntity = new Company()
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                NormalizedName = companyName.ToUpper(),
                OwnerId = Guid.NewGuid().ToString(),
                MaxVacationDaysPerYear = 10,
                CompanyEmployeesInfo = new List<EmployeeInfo>()
                {
                    new EmployeeInfo()
                    {
                        Id = employeeId,
                        Email = employeeEmail,
                        NormalizedEmail = employeeEmail.ToUpper(),
                        HasJoined = true,
                        Role = EmployeeRole.Employee,
                        UserId = userId,
                    }
                }
            };

            _dbContext.Companies.Add(newCompanyEntity);
            _dbContext.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted(); //Clean up after each test
            _dbContext.Dispose();

            _unitOfWork.Dispose();
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldAddVacation_WhenInputValid()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNotNull(foundVacation);
            Assert.That(foundVacation.StartDate, Is.EqualTo(startDate));
            Assert.That(foundVacation.EndDate, Is.EqualTo(endDate));
            Assert.That(foundVacation.Days, Is.EqualTo((endDate - startDate).Days + 1));
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenCompanyNotFound()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            Guid randomCompanyId = Guid.NewGuid();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = randomCompanyId,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenEmployeeInfoNotFound()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            string randomUserId = Guid.NewGuid().ToString();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, randomUserId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenEndDateBeforeStartDate()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = startDate.AddDays(-2);

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat, EndDateCanNotBeAfterStartDate)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenStartDateToday()
        {
            //Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(2);

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat, StartDateCanNotBeTodayOrInThePast)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenStartDateInThePast()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(-1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat, StartDateCanNotBeTodayOrInThePast)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenStartDateOrEndDateMoreThanXMonthsAwayFromTomorrow()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddMonths(VacationMaxMonthsFromDates).AddDays(1);
            DateTime endDate = DateTime.Today.AddMonths(VacationMaxMonthsFromDates).AddDays(2);

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat,
                    String.Format(DatesCanNotBeMoreThanXMonthsFromTomorrowFormat, VacationMaxMonthsFromDates))));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.FirstOrDefault();
            Assert.IsNull(foundVacation);
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenVacationWithStartDateExists()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate.AddDays(1),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(StartDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat, startDate.ToString(VacationDateFormat), newEntity.Status.ToString()))));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenVacationWithEndDateExists()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate.AddDays(1),
                EndDate = endDate,
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(EndDateCanNotBeSameAsStartOrEndDateOfAnotherVacationFormat, endDate.ToString(VacationDateFormat), newEntity.Status.ToString()))));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenVacationOverlapsWithExisting()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(5);

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate.AddDays(1),
                EndDate = endDate.AddDays(-2),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(ModelErrorMessages.InvalidModelStateFormat,
                        String.Format(CanNotAddVacationAsItIsInRangeOfAnotherVacation, newEntity.StartDate.ToString(VacationDateFormat), newEntity.EndDate.ToString(VacationDateFormat), newEntity.Status.ToString()))));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenVacationWithSameStartAndEndDateExists()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                Status = VacationStatus.Denied
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(
                String.Format(VacationWithSameDatesExistsFormat, newEntity.StartDate.ToString(VacationDateFormat), newEntity.EndDate.ToString(VacationDateFormat), newEntity.Status.ToString())));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddVacationOfEmployeeAsync_ShouldNotAddVacation_WhenEmployeeDoesNotHaveEnoughVacationDaysLeft()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = endDate.AddDays(1),
                EndDate = endDate.AddDays(newCompanyEntity.MaxVacationDaysPerYear),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new AddVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
            };

            //Act
            var result = await _vacationService.AddVacationOfEmployeeAsync(model, userId, true);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(String.Format(NotEnoughVacationDaysLeftFormat, (endDate - startDate).Days + 1)));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.CompanyEmployeesInfo.First().Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldDeleteVacation_WhenInputValid()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationId = newEntity.Id,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldNotDeleteVacation_WhenCompanyNotFound()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = randomCompanyId,
                EmployeeId = employeeId,
                VacationId = newEntity.Id,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldNotDeleteVacation_WhenEmployeeInfoNotFound()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            string randomUserId = Guid.NewGuid().ToString();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationId = newEntity.Id,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, randomUserId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldNotDeleteVacation_WhenVacationNotFound()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            Guid randomVacationId = Guid.NewGuid();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationId = randomVacationId,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldNotDeleteVacation_WhenVacationStatusIsDenied()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = VacationStatus.Denied
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationId = newEntity.Id,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotDeleteDeniedVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfEmployeeAsync_ShouldNotDeleteVacation_WhenVacationIsStarted()
        {
            //Arrange
            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today.AddDays(1),
                Status = VacationStatus.Approved
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteVacationOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationId = newEntity.Id,
            };

            //Act
            var result = await _vacationService.DeleteVacationOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotDeleteStartedVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }
    }
}
