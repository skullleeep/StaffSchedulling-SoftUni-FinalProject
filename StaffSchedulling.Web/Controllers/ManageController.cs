﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.ViewModels.Vacation;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class ManageController(IPermissionService _permissionService, ICompanyService _companyService, IEmployeeInfoService _employeeInfoService,
        IDepartmentService _departmentService, IVacationService _vacationService) : BaseController
    {
        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Company(string id)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }


            bool canUserEdit = permissionRole >= PermissionRole.Editor ? true : false; //Check for edit permission
            bool canUserDelete = permissionRole == PermissionRole.Owner ? true : false; //Check for delete permission

            var model = await _companyService.GetCompanyFromIdAsync(companyGuid, canUserEdit, canUserDelete);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Employees(string id, string? searchQuery, EmployeeSearchFilter? searchFilter, int currentPage = 1)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(companyGuid, searchQuery, searchFilter, currentPage, permissionRole);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Departments(string id, int currentPage = 1)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Editor)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _departmentService.GetCompanyManageDepartmentsModel(companyGuid, currentPage);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Vacations(string id, string? searchQuery, VacationSortFilter? sortFilter, int currentPage = 1)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Manager)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //TODO: Create the real working function. Dont forget that e.x Supervisor can only manage vacations of employees in his department
            //var model = await _vacationService.GetCompanyManageVacationsModel(companyGuid, searchQuery, sortFilter, currentPage, permissionRole);

            var model = new ManageVacationsViewModel()
            {
                CompanyId = companyGuid,
                CurrentPage = currentPage,
                SearchQuery = searchQuery,
                SortFilter = sortFilter,
                TotalPages = 1,
                Vacations = new List<VacationViewModel>()
                {
                    new VacationViewModel()
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = companyGuid,
                        CreatedOn = DateTime.Now,
                        Days = 1,
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(1),
                        EmployeeEmail = "test@email.com",
                        EmployeeName = "John Test Doe"
                    }
                }
            };

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        [HttpGet("[controller]/[action]/{id?}")]
        public async Task<IActionResult> Schedule(string id, VacationSortFilter? sortFilter, int currentPage = 1)
        {
            Guid companyGuid = Guid.Empty;

            //Check for non-valid string or guid
            if (IsGuidValid(id, ref companyGuid) == false)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            //Get user email
            string userEmail = GetCurrentUserEmail();

            PermissionRole permissionRole = await _permissionService.GetUserPermissionInCompanyAsync(companyGuid, userEmail);

            //Check for access permission
            if (permissionRole < PermissionRole.Visitor || permissionRole == PermissionRole.Owner) //Don't allow owner as the owner doesn't have a schedule
            {
                return RedirectToAction("Index", "Dashboard");
            }

            string userId = GetCurrentUserId();

            var model = await _vacationService.GetCompanyManageScheduleModel(companyGuid, sortFilter, currentPage, userId);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}
