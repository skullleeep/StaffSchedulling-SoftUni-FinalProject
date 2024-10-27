using Microsoft.EntityFrameworkCore;
using StaffScheduling.Common;
using StaffScheduling.Data;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Models.Dtos;
using StaffScheduling.Web.Models.ViewModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;

namespace StaffScheduling.Web.Services.DbServices
{
    public class CompanyService(ApplicationDbContext _dbContext, ApplicationUserManager _userManager) : ICompanyService
    {
        public async Task<StatusReport> AddCompanyAsync(CompanyDto model)
        {
            var newEntity = new Company()
            {
                Name = model.Name,
                OwnerId = model.OwnerId,
            };

            try
            {
                await _dbContext.Companies.AddAsync(newEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return new StatusReport() { Ok = false, Message = "Could not add new Company" };
            }

            return new StatusReport() { Ok = true };
        }

        public async Task<CompanyViewModel?> GetCompanyFromInviteLinkAsync(Guid invite)
        {
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Invite == invite);
            if (entity == null)
            {
                return null;
            }

            return new CompanyViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Invite = entity.Invite
            };
        }

        public async Task<DashboardCompaniesViewModel> GetOwnedAndJoinedCompaniesFromUserEmailAsync(string email)
        {
            var ownedCompanies = new List<CompanyViewModel>();

            var ownedCompanyIds = await _userManager.GetOwnedCompanyIdsFromUserEmailAsync(email);
            if (ownedCompanyIds != null)
            {
                ownedCompanies = await _dbContext
                                            .Companies
                                            .Where(c => ownedCompanyIds.Contains(c.Id))
                                            .Select(c => new CompanyViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();
            }

            var joinedCompanies = new List<CompanyViewModel>();

            var joinedCompanyIds = await _userManager.GetJoinedCompanyIdsFromUserEmailAsync(email);
            if (joinedCompanyIds != null)
            {
                joinedCompanies = await _dbContext
                                            .Companies
                                            .Where(c => joinedCompanyIds.Contains(c.Id))
                                            .Select(c => new CompanyViewModel()
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Invite = c.Invite
                                            })
                                            .AsNoTracking()
                                            .ToListAsync();
            }

            return new DashboardCompaniesViewModel
            {
                OwnedCompanies = ownedCompanies,
                JoinedCompanies = joinedCompanies
            };
        }

        public async Task<string> GetCompanyOwnerEmailFromIdAsync(int id)
        {
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
            {
                return String.Empty;
            }

            string ownerEmail = await _userManager.GetUserEmailFromIdAsync(entity.OwnerId);

            return ownerEmail;
        }

        public async Task<bool> HasCompanyWithIdAsync(int id)
        {
            //Check if company with this GUID exists
            var entity = await _dbContext
                .Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                return false;
            }

            return true;
        }
    }
}
