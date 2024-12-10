using StaffScheduling.Web.Models.FilterModels;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class ManageVacationsViewModel : ManageVacationsFilters
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required]
        public List<VacationViewModel> Vacations { get; set; } = new List<VacationViewModel>();

        [Required]
        public int TotalPages { get; set; }
    }
}
