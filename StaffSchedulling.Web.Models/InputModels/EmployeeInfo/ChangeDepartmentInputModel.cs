﻿using StaffScheduling.Web.Models.ViewModels.Department;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.EmployeeInfo
{
    public class ChangeDepartmentInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        public List<ManageEmployeesInfoDepartmentViewModel> Departments { get; set; } = new List<ManageEmployeesInfoDepartmentViewModel>();

        [Required]
        public Guid EmployeeId { get; set; }

        public string CurrentDepartmentName { get; set; } = String.Empty;

        public Guid SelectedDepartmentId { get; set; } = Guid.Empty;
    }
}