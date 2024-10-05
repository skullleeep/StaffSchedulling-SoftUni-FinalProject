using System.ComponentModel.DataAnnotations;
using static StaffScheduling.Common.DataConstants.Company;

namespace StaffScheduling.Web.Models.Dtos
{
    public class CompanyDto
    {
        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public string OwnerId { get; set; } = null!;
    }
}
