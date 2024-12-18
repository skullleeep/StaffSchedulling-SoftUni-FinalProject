﻿using StaffScheduling.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace StaffScheduling.Web.Models.InputModels.Vacation
{
    public class DeleteAllVacationsOfEmployeeInputModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public VacationStatus VacationStatusToDelete { get; set; }
    }
}
