namespace StaffScheduling.Web.Models.ViewModels.Company
{
    public class CompanyJoinViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? ErrorMessage { get; set; }
    }
}
