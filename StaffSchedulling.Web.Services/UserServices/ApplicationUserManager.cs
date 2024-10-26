using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffScheduling.Data.Models;

namespace StaffScheduling.Web.Services.UserServices
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
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

        public async Task<List<int>> GetOwnedCompanyIdsFromUserEmailAsync(string email)
        {
            var user = await Users
                .Include(u => u.CompaniesOwned)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            return user.CompaniesOwned.Select(c => c.Id).ToList();
        }

        public async Task<bool> HasUserWithEmailAsync(string email)
        {
            var user = await Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return false;

            return true;
        }
    }
}
