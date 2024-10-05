using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Services.DbServices.Contracts;

namespace StaffScheduling.Web.Services.UserServices
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private IEmployeeInfoService _employeeInfoService;

        public ApplicationUserManager(IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }
    }
}
