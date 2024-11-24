using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.Constants.DataConstants.ApplicationUser;
using static StaffScheduling.Common.ErrorMessages.DataErrorMessages.ApplicationUser;

namespace StaffScheduling.Web.Areas.Identity.InputModels
{
    public class RegisterInputModel
    {
        [Required]
        [MinLength(FullNameMinLength)]
        [MaxLength(FullNameMaxLength)]
        [Display(Name = "Full Name")]
        [RegularExpression(FullNameRegexPattern, ErrorMessage = FullNameError)]
        public string FullName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(PasswordRegexPattern, ErrorMessage = PasswordError)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
