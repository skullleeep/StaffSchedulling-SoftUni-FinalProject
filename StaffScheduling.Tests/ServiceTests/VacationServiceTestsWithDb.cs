using StaffScheduling.Common.Enums;
using StaffScheduling.Common.Enums.Filters;
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

            _unitOfWork.Companies.Add(newCompanyEntity);
            _unitOfWork.SaveChangesAsync();
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

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldDeleteAllPendingVacations_WhenInputValid()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Pending,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Pending,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Approved,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationStatusToDelete = VacationStatus.Pending,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().Status, Is.EqualTo(VacationStatus.Approved));
        }

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldDeleteAllApprovedVacations_WhenInputValid()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().Status, Is.EqualTo(VacationStatus.Denied));
        }

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldDeleteOnlyNotStartedVacations_WhenInputValid()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(-5),
                    EndDate = DateTime.Today.AddDays(-4),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(-3),
                    EndDate = DateTime.Today.AddDays(-2),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Approved,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(2));
            Assert.IsFalse(foundCompanyEntity.Vacations.Any(v => v.StartDate == DateTime.Today.AddDays(2)));
        }

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldNotDeleteAllVacations_WhenStatusToDeleteIsDenied()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                VacationStatusToDelete = VacationStatus.Denied,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotDeleteAllDeniedVacations));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldNotDeleteAllVacations_WhenCompanyNotFound()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = randomCompanyId,
                EmployeeId = employeeId,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task DeleteAllVacationsOfEmployeeAsync_ShouldNotDeleteAllVacations_WhenEmployeeNotFound()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            Guid randomEmployeeId = Guid.NewGuid();

            var model = new DeleteAllVacationsOfEmployeeInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                EmployeeId = randomEmployeeId,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            //Act
            var result = await _vacationService.DeleteAllVacationsOfEmployeeAsync(model, userId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindEmployee));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldChangeStatus_WhenInputValid()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Approved;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(statusToChangeTo));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldChangeStatus_WhenUserIsInRoleWhichNeedsDepartmentAndEmployeeToChangeStatusIsInSameDepartment()
        {
            //Arrange
            Guid? neededDepartmentId = Guid.NewGuid();

            newCompanyEntity.CompanyEmployeesInfo.First().DepartmentId = neededDepartmentId;

            await _unitOfWork.SaveChangesAsync();

            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus,
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Approved;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(statusToChangeTo));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenUserIsInRoleWhichNeedsDepartmentAndEmployeeToChangeStatusIsInAnotherDepartment()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus,
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Approved;
            Guid? neededDepartmentId = Guid.NewGuid();

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenNewStatusIsPending()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Denied;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus,
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Pending;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotChangeVacationStatusToPending));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenCompanyNotFound()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();
            VacationStatus statusToChangeTo = VacationStatus.Approved;

            var model = new ChangeStatusInputModel { CompanyId = randomCompanyId, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenVacationNotFound()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            Guid randomVacationId = Guid.NewGuid();
            VacationStatus statusToChangeTo = VacationStatus.Approved;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = randomVacationId, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenVacationIsToday()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Approved;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Denied;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotChangeStatufOfVacationThatHasAlreadyStarted));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenVacationIsInThePast()
        {
            //Arrange
            VacationStatus startingStatus = VacationStatus.Approved;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(-2),
                EndDate = DateTime.Today.AddDays(-1),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Denied;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotChangeStatufOfVacationThatHasAlreadyStarted));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task ChangeStatusAsync_ShouldNotChangeStatus_WhenUserDoesNotHaveNeededPermissionToManageEmployee()
        {
            //Arrange
            newCompanyEntity.CompanyEmployeesInfo.First().Role = EmployeeRole.Admin;

            await _unitOfWork.SaveChangesAsync();

            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntity = new Vacation()
            {
                Id = Guid.NewGuid(),
                CompanyId = newCompanyEntity.Id,
                EmployeeId = employeeId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                Status = startingStatus
            };

            await _unitOfWork.Vacations.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus statusToChangeTo = VacationStatus.Denied;

            var model = new ChangeStatusInputModel { CompanyId = newCompanyEntity.Id, VacationId = newEntity.Id, Status = statusToChangeTo };

            PermissionRole userPermissionRole = PermissionRole.Manager;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.ChangeStatusAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotManageEmployeeVacationAsLowerPermission));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            var foundVacation = foundCompanyEntity.Vacations.First();
            Assert.That(foundVacation.Status, Is.EqualTo(startingStatus));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldDeleteVacation_WhenInputValid()
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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationId = newEntity.Id,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldDeleteVacation_WhenUserIsInRoleWhichNeedsDepartmentAndEmployeeToDeleteVacationIsInSameDepartment()
        {
            //Arrange
            Guid? neededDepartmentId = Guid.NewGuid();

            newCompanyEntity.CompanyEmployeesInfo.First().DepartmentId = neededDepartmentId;

            await _unitOfWork.SaveChangesAsync();

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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationId = newEntity.Id,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldNotDeleteVacation_WhenUserIsInRoleWhichNeedsDepartmentAndEmployeeToDeleteVacationIsInAnotherDepartment()
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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationId = newEntity.Id,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = Guid.NewGuid();

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldNotDeleteVacation_WhenCompanyNotFound()
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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = randomCompanyId,
                VacationId = newEntity.Id,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldNotDeleteVacation_WhenVacationNotFound()
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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationId = randomVacationId,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindVacation));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteVacationOfCompanyAsync_ShouldNotDeleteVacation_WhenUserDoesNotHaveNeededPermissionToManageEmployee()
        {
            //Arrange
            newCompanyEntity.CompanyEmployeesInfo.First().Role = EmployeeRole.Admin;

            await _unitOfWork.SaveChangesAsync();

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

            var model = new DeleteVacationOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationId = newEntity.Id,
            };

            PermissionRole userPermissionRole = PermissionRole.Manager;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteVacationOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CanNotManageEmployeeVacationAsLowerPermission));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldDeleteAllApprovedVacations_WhenInputValid()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().Status, Is.EqualTo(VacationStatus.Denied));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldDeleteAllDeniedVacations_WhenInputValid()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Denied,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationStatusToDelete = VacationStatus.Denied,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().Status, Is.EqualTo(VacationStatus.Approved));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldDeleteAllPendingVacations_WhenInputValid()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Pending,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Pending,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationStatusToDelete = VacationStatus.Pending,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().Status, Is.EqualTo(VacationStatus.Approved));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldDeleteOnlyCorrectVacations_WhenUserIsInRoleWhichNeedsDepartmentAndVacationToDeleteEmployeesAreInSameDepartment()
        {
            //Arrange
            Guid? neededDepartmentId = Guid.NewGuid();

            Guid newEmployeeId = Guid.NewGuid();

            var newEmployeeInfoEntity = new EmployeeInfo()
            {
                Id = newEmployeeId,
                Email = employeeEmail + "test",
                NormalizedEmail = employeeEmail.ToUpper() + "test",
                HasJoined = true,
                Role = EmployeeRole.Employee,
                UserId = Guid.NewGuid().ToString(),
                DepartmentId = neededDepartmentId,
            };

            await _unitOfWork.EmployeesInfo.AddAsync(newEmployeeInfoEntity);
            await _unitOfWork.SaveChangesAsync();

            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Approved,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            PermissionRole userPermissionRole = PermissionRole.Manager;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.That(foundCompanyEntity.Vacations.First().EmployeeId, Is.EqualTo(employeeId));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldDeleteOnlyCorrectVacations_WhenUserDoesNotHaveNeededPermissionToManageAllEmployees()
        {
            //Arrange
            Guid newEmployeeId = Guid.NewGuid();

            var newEmployeeInfoEntity = new EmployeeInfo()
            {
                Id = newEmployeeId,
                Email = employeeEmail + "test",
                NormalizedEmail = employeeEmail.ToUpper() + "test",
                HasJoined = true,
                Role = EmployeeRole.Admin,
                UserId = Guid.NewGuid().ToString(),
            };

            await _unitOfWork.EmployeesInfo.AddAsync(newEmployeeInfoEntity);
            await _unitOfWork.SaveChangesAsync();

            VacationStatus startingStatus = VacationStatus.Pending;

            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Pending,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Pending,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Pending,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = newCompanyEntity.Id,
                VacationStatusToDelete = VacationStatus.Pending,
            };

            PermissionRole userPermissionRole = PermissionRole.Manager;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsTrue(result.Ok);
            Assert.IsNull(result.Message);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(1));
            Assert.IsFalse(foundCompanyEntity.Vacations.Any(v => v.EmployeeId == employeeId));
        }

        [Test]
        public async Task DeleteAllVacationsOfCompanyAsync_ShouldNotDeleteAllVacations_WhenCompanyNotFound()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            var model = new DeleteAllVacationsOfCompanyInputModel()
            {
                CompanyId = randomCompanyId,
                VacationStatusToDelete = VacationStatus.Approved,
            };

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var result = await _vacationService.DeleteAllVacationsOfCompanyAsync(model, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsFalse(result.Ok);
            Assert.That(result.Message, Is.EqualTo(CouldNotFindCompany));

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(foundCompanyEntity.Vacations.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetCompanyManageScheduleModelAsync_ShouldReturnModel_WhenInputValid()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            //Act
            var model = await _vacationService.GetCompanyManageScheduleModelAsync(newCompanyEntity.Id, null, 1, userId, true);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(foundCompanyEntity.Vacations.Count));
        }

        [Test]
        public async Task GetCompanyManageScheduleModelAsync_ShouldReturnModelWithCorrectVacations_WhenUserSortsByStatus()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            VacationSortFilter? sortFilter = VacationSortFilter.OnlyApproved;

            //Act
            var model = await _vacationService.GetCompanyManageScheduleModelAsync(newCompanyEntity.Id, sortFilter, 1, userId, true);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(2));
            Assert.That(model.Vacations.Where(v => v.Status == VacationStatus.Approved).Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageScheduleModelAsync_ShouldNotReturnModel_WhenCompanyNotFound()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            //Act
            var model = await _vacationService.GetCompanyManageScheduleModelAsync(randomCompanyId, null, 1, userId, true);

            //Assert
            Assert.IsNull(model);
        }

        [Test]
        public async Task GetCompanyManageScheduleModelAsync_ShouldNotReturnModel_WhenEmployeeInfoNotFound()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            string randomUserId = Guid.NewGuid().ToString();

            //Act
            var model = await _vacationService.GetCompanyManageScheduleModelAsync(newCompanyEntity.Id, null, 1, randomUserId, true);

            //Assert
            Assert.IsNull(model);
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldReturnModel_WhenInputValid()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(newCompanyEntity.Id, null, null, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(foundCompanyEntity.Vacations.Count));
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldReturnModelWithCorrectVacations_WhenUserSortsByStatus()
        {
            //Arrange
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Denied,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            VacationSortFilter? sortFilter = VacationSortFilter.OnlyDenied;
            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(newCompanyEntity.Id, null, sortFilter, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(2));
            Assert.That(model.Vacations.Where(v => v.Status == VacationStatus.Denied).Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldReturnModelWithCorrectVacations_WhenUserSearchesByName()
        {
            //Arrange
            string userName = "Johnny Test";

            newCompanyEntity.CompanyEmployeesInfo.First().User = new ApplicationUser { FullName = userName };

            await _unitOfWork.SaveChangesAsync();

            Guid newEmployeeInfoId = Guid.NewGuid();
            string newEmployeeEmail = "johhny@mail.com";
            string userName2 = "Kristiqn Ivanov";

            var newEntity = new EmployeeInfo()
            {
                Id = newEmployeeInfoId,
                CompanyId = newCompanyEntity.Id,
                Email = newEmployeeEmail,
                NormalizedEmail = newEmployeeEmail.ToUpper(),
                User = new ApplicationUser { FullName = userName2 },
            };

            await _unitOfWork.EmployeesInfo.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeInfoId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            string searchQuery = userName.Substring(0, 4).ToLower();
            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            var user = _dbContext.ApplicationUsers.Last();

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(newCompanyEntity.Id, searchQuery, null, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(2));
            Assert.That(model.Vacations.Where(v => v.Status == VacationStatus.Approved).Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldReturnModelWithCorrectVacations_WhenUserSearchesByEmail()
        {
            //Arrange
            string userName = "Johnny Test";

            newCompanyEntity.CompanyEmployeesInfo.First().User = new ApplicationUser { FullName = userName };

            await _unitOfWork.SaveChangesAsync();

            Guid newEmployeeInfoId = Guid.NewGuid();
            string newEmployeeEmail = "johhny@mail.com";
            string userName2 = "Kristiqn Ivanov";

            var newEntity = new EmployeeInfo()
            {
                Id = newEmployeeInfoId,
                CompanyId = newCompanyEntity.Id,
                Email = newEmployeeEmail,
                NormalizedEmail = newEmployeeEmail.ToUpper(),
                User = new ApplicationUser { FullName = userName2 },
            };

            await _unitOfWork.EmployeesInfo.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeInfoId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            string searchQuery = "employee@";
            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            var user = _dbContext.ApplicationUsers.Last();

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(newCompanyEntity.Id, searchQuery, null, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(2));
            Assert.That(model.Vacations.Where(v => v.Status == VacationStatus.Approved).Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldReturnModelWithCorrectVacations_WhenUserIsInRoleWhichNeedsDepartmentAndEmployeesOfVacationsAreInSameDepartment()
        {
            //Arrange
            Guid? neededDepartmentId = Guid.NewGuid();

            newCompanyEntity.CompanyEmployeesInfo.First().DepartmentId = neededDepartmentId;

            await _unitOfWork.SaveChangesAsync();

            Guid newEmployeeInfoId = Guid.NewGuid();
            string newEmployeeEmail = "johhny@mail.com";
            string userName2 = "Kristiqn Ivanov";

            var newEntity = new EmployeeInfo()
            {
                Id = newEmployeeInfoId,
                CompanyId = newCompanyEntity.Id,
                Email = newEmployeeEmail,
                NormalizedEmail = newEmployeeEmail.ToUpper(),
                User = new ApplicationUser { FullName = userName2 },
            };

            await _unitOfWork.EmployeesInfo.AddAsync(newEntity);
            await _unitOfWork.SaveChangesAsync();

            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = newEmployeeInfoId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            PermissionRole userPermissionRole = PermissionRole.Owner;

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(newCompanyEntity.Id, null, null, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNotNull(model);

            var foundCompanyEntity = _dbContext.Companies.First(c => c.Id == newCompanyEntity.Id);
            Assert.That(model.Vacations.Count, Is.EqualTo(2));
            Assert.That(model.Vacations.Where(v => v.Status == VacationStatus.Approved).Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompanyManageVacationsModelAsync_ShouldNotReturnModel_WhenCompanyNotFound()
        {
            var newEntities = new List<Vacation>()
            {
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(1),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    Status = VacationStatus.Approved,
                },
                new Vacation()
                {
                    Id = Guid.NewGuid(),
                    CompanyId = newCompanyEntity.Id,
                    EmployeeId = employeeId,
                    StartDate = DateTime.Today.AddDays(4),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = VacationStatus.Denied,
                }
            };

            await _unitOfWork.Vacations.AddRangeAsync(newEntities.ToArray());
            await _unitOfWork.SaveChangesAsync();

            Guid randomCompanyId = Guid.NewGuid();

            PermissionRole userPermissionRole = PermissionRole.Owner;
            Guid? neededDepartmentId = null;

            //Act
            var model = await _vacationService.GetCompanyManageVacationsModelAsync(randomCompanyId, null, null, 1, userPermissionRole, neededDepartmentId);

            //Assert
            Assert.IsNull(model);
        }
    }
}