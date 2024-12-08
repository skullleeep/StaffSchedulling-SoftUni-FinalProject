using StaffScheduling.Common.Enums.Filters;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.ViewModels.Vacation
{
    public class ManageVacationsViewModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public List<VacationViewModel> Vacations { get; set; } = new List<VacationViewModel>();

        public string? SearchQuery { get; set; }

        public VacationSortFilter? SortFilter { get; set; }

        [Required]
        public int CurrentPage { get; set; } = 1;

        [Required]
        public int TotalPages { get; set; }
    }
}
