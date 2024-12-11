// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using StaffScheduling.Common;
using StaffScheduling.Common.Enums;
using StaffScheduling.Data.Models;
using StaffScheduling.Web.Areas.Identity.InputModels;
using StaffScheduling.Web.Extensions;
using StaffScheduling.Web.Models.InputModels.Company;
using StaffScheduling.Web.Services.DbServices.Contracts;
using StaffScheduling.Web.Services.UserServices;
using System.Text;
using System.Text.Encodings.Web;


namespace StaffScheduling.Web.Areas.Identity.Pages.Account
{
    public class RegisterCompanyModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationUserManager _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterCompanyModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICompanyService _companyService;

        public RegisterCompanyModel(
            ApplicationUserManager userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterCompanyModel> logger,
            IEmailSender emailSender,
            ICompanyService companyService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _companyService = companyService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public CompanyRegisterInputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                //Update the full name variable
                user.FullName = Input.FullName;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //Add FullName Claim
                    var resultClaimAdd = await _userManager.AddUpdateUserClaimAsync(user, CustomClaimType.FullName, Input.FullName);
                    if (!resultClaimAdd.Ok)
                    {
                        //Delete user
                        await _userManager.DeleteAsync(user);

                        ModelState.AddModelError(String.Empty, resultClaimAdd.Message);
                        return Page();
                    }

                    //Add User to role User
                    var resultAssignment = await _userManager.AddToRoleAsync(user, UserRole.User.ToString());
                    if (!resultAssignment.Succeeded)
                    {
                        //Delete user
                        await _userManager.DeleteAsync(user);

                        ModelState.AddModelError(String.Empty, $"Couldn't add role {UserRole.User} to newely created user!");
                        return Page();
                    }

                    //Get user id
                    var userId = await _userManager.GetUserIdAsync(user);

                    //Create new Company and add it to DB
                    var newCompany = new CompanyCreateInputModel()
                    {
                        Name = Input.CompanyName,
                        MaxVacationDaysPerYear = Input.MaxVacationDaysPerYear,
                    };
                    StatusReport resultCreatingCompany = await _companyService.CreateCompanyAsync(newCompany, userId);
                    if (!resultCreatingCompany.Ok)
                    {
                        //Delete user
                        await _userManager.DeleteAsync(user);

                        ModelState.AddModelError(String.Empty, resultCreatingCompany.Message);
                        return Page();
                    }


                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        //Redirect to 'Dashboard' action
                        return RedirectToAction("Index", "Dashboard");
                        //return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
