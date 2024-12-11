// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Services.DbServices.Contracts;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeInfoService _employeeInfoService;

        public EmailModel(UserManager<ApplicationUser> userManager, IEmployeeInfoService employeeInfoService)
        {
            _userManager = userManager;
            _employeeInfoService = employeeInfoService;
        }

        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;
            Input = new InputModel
            {
                NewEmail = email,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail.ToLower() != email)
            {
                string oldEmail = email;

                //Check if there is already a user with the same email
                var existingUser = await _userManager.FindByEmailAsync(Input.NewEmail);
                if (existingUser != null)
                {
                    //If a user with the same email exists, return an error message
                    ModelState.AddModelError(string.Empty, "Email is already taken.");
                    await LoadAsync(user);
                    return Page();
                }

                //Update both Email and UserName to the new email
                user.UserName = Input.NewEmail.ToLower();  //Set username to new email
                user.Email = Input.NewEmail.ToLower();     //Set email to new email

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    //Update employeeInfos also
                    await _employeeInfoService.ChangeEmailOfUser(oldEmail, Input.NewEmail.ToLower());

                    StatusMessage = "Your email and username have been updated.";
                    return RedirectToPage();
                }

                StatusMessage = "There was an error updating your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email and username are unchanged.";
            return RedirectToPage();
        }


    }
}
