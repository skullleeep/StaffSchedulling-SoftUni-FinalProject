﻿using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Services.UserServices;
using System.Security.Claims;

namespace StaffScheduling.Web.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<StatusReport> CreateWithRoleAsync(this ApplicationUserManager userManager, string email, string password, string fullName, UserRole roleEnum)
        {
            var user = new ApplicationUser();
            user.UserName = email;
            user.Email = email;
            user.FullName = fullName;

            var resultCreation = await userManager.CreateAsync(user, password);
            if (!resultCreation.Succeeded)
            {
                return new StatusReport { Ok = false, Message = "Could not create user!" };
            }

            var resultAssignment = await userManager.AddToRoleAsync(user, roleEnum.ToString());
            if (!resultAssignment.Succeeded)
            {
                return new StatusReport { Ok = false, Message = $"Could not assign role '{roleEnum.ToString()}' to user!" };
            }

            //Add Claim to be able to get the 'FullName' custom property without database requests
            var statusClaimCreation = await userManager.AddUpdateUserClaimAsync(user, CustomClaimType.FullName, fullName);
            if (!statusClaimCreation.Ok)
            {
                return statusClaimCreation;
            }

            return new StatusReport { Ok = true };
        }


        public static async Task<StatusReport> AddUpdateUserClaimAsync(this ApplicationUserManager userManager, ApplicationUser user, CustomClaimType claimType, string claimValue)
        {
            //Get existing claims
            var existingClaims = await userManager.GetClaimsAsync(user);

            //Check if the claim already exists
            var existingClaim = existingClaims.FirstOrDefault(c => c.Type == claimType.ToString());

            if (existingClaim != null)
            {
                //Remove the existing claim
                var resultClaimDeletion = await userManager.RemoveClaimAsync(user, existingClaim);
                if (!resultClaimDeletion.Succeeded)
                {
                    return new StatusReport { Ok = false, Message = $"Could not delete '{claimType.ToString()}' claim for user before updating it!" };
                }
            }

            //Add the new claim
            var resultClaimCreation = await userManager.AddClaimAsync(user, new Claim(claimType.ToString(), claimValue));
            if (!resultClaimCreation.Succeeded)
            {
                return new StatusReport { Ok = false, Message = $"Could not create '{claimType.ToString()}' claim for user!" };
            }

            return new StatusReport { Ok = true };
        }
    }
}
