using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Data.Repository;
using StaffScheduling.Data.UnitOfWork;
using StaffScheduling.Data.UnitOfWork.Contracts;
using StaffScheduling.Web.Services.UserServices;

namespace StaffScheduling.Tests
{
    public static class SetupForServices
    {
        public static void InitializeInMemoryDbAndMockUnitOfWork(ref ApplicationDbContext dbContext, ref IUnitOfWork unitOfWork)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) //Unique DB for each test
                .Options;

            dbContext = new ApplicationDbContext(options);

            //Set up generic repositories with the InMemory database context
            var companiesRepo = new GenericRepository<Company, Guid>(dbContext);
            var departmentsRepo = new GenericRepository<Department, Guid>(dbContext);
            var employeesInfoRepo = new GenericRepository<EmployeeInfo, Guid>(dbContext);
            var vacationsRepo = new GenericRepository<Vacation, Guid>(dbContext);

            //Initialize the UnitOfWork
            unitOfWork = new UnitOfWork(
                companiesRepo,
                departmentsRepo,
                employeesInfoRepo,
                vacationsRepo,
                dbContext
            );
        }

        public static ApplicationUserManager MockUserManager()
        {
            //Mock dependencies
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidatorsMock = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
            var passwordValidatorsMock = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var identityErrorDescriberMock = new Mock<IdentityErrorDescriber>();
            var servicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

            //Create Mock ApplicationUserManager Instance
            var userManager = new Mock<ApplicationUserManager>(
                userStoreMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidatorsMock,
                passwordValidatorsMock,
                keyNormalizerMock.Object,
                identityErrorDescriberMock.Object,
                servicesMock.Object,
                loggerMock.Object
                );

            return userManager.Object;
        }
    }
}
