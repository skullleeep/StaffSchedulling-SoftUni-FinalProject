﻿using Microsoft.AspNetCore.Identity;
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

        public virtual async Task<List<Guid>> GetOwnedCompanyIdsFromUserEmailAsync(string email)
        {
            var user = await Users
                .Include(u => u.CompaniesOwned)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return new List<Guid>();

            return user.CompaniesOwned.Select(c => c.Id).ToList();
        }

        public virtual async Task<List<Guid>> GetJoinedCompanyIdsFromUserEmailAsync(string email)
        {
            var user = await GetUserWithEmployeeInfoInCompaniesFromEmailAsync(email);
            if (user == null)
            {
                return new List<Guid>();
            }

            return user.EmployeeInfoInCompanies.Select(c => c.CompanyId).ToList();
        }

        public virtual async Task<string> GetUserEmailFromIdAsync(string id)
        {
            var user = await Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return String.Empty;

            return user.Email ?? String.Empty;
        }

        private async Task<ApplicationUser?> GetUserWithEmployeeInfoInCompaniesFromEmailAsync(string email)
        {
            return await Users
                .Include(u => u.EmployeeInfoInCompanies)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
