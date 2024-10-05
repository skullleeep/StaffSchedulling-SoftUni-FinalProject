// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Extensions;
using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.ApplicationUser;
using static StaffScheduling.Common.DataConstants.Web;
using static StaffScheduling.Common.DataErrorMessages.ApplicationUser;

namespace StaffScheduling.Web.Areas.Identity.Pages.Account
{
    public class AdminChangeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminChangeModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MinLength(FullNameMinLength)]
            [MaxLength(FullNameMaxLength)]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [MinLength(EmailMinLength)]
            [MaxLength(EmailMaxLength)]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [RegularExpression(PasswordRegexPattern, ErrorMessage = PasswordError)]
            public string Password { get; set; }

        }

        public async Task OnGetAsync()
        {

            //If user is singed in this should not work
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }

            string adminPassword = TempData["AdminPassword"] as string;

            //Check if client is admin
            if (adminPassword != DefaultAdminPassword)
            {
                Response.Redirect("/");
            }

            var admin = _userManager.Users.FirstOrDefault(u => u.Email == DefaultAdminEmail);
            if (admin == null)
            {
                Response.Redirect("/");
            }

            //Check if password is right
            var resultPass = await _signInManager.CheckPasswordSignInAsync(admin, DefaultAdminPassword, false);
            if (!resultPass.Succeeded)
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var admin = _userManager.Users.FirstOrDefault(u => u.Email == DefaultAdminEmail);

                if (admin == null)
                {
                    ModelState.AddModelError(string.Empty, "Could not find admin user!");

                    return Page();
                }

                admin.FullName = Input.FullName;
                admin.UserName = Input.Email;
                admin.Email = Input.Email;

                var resultUpdate = await _userManager.UpdateAsync(admin);
                if (!resultUpdate.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not change admin FullName, Email and/or Username variables!");

                    return Page();
                }

                admin = _userManager.Users.FirstOrDefault(u => u.Email == Input.Email);

                //Update FullName claim as FullName has changed
                var statusClaimUpdate = await _userManager.AddUpdateUserClaimAsync(admin, ClaimType.FullName, Input.FullName);
                if (!statusClaimUpdate.Ok)
                {
                    ModelState.AddModelError(string.Empty, statusClaimUpdate.Message);

                    return Page();
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
                var resultPass = await _userManager.ResetPasswordAsync(admin, token, Input.Password);
                if (!resultPass.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Could not change admin password!");

                    return Page();
                }

                return RedirectToPage("./Login");
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
