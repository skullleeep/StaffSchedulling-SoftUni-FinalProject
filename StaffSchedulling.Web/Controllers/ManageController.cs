﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class ManageController(IPermissionService _permissionService, ICompanyService _companyService, IEmployeeInfoService _employeeInfoService,
        IDepartmentService _departmentService, IVacationService _vacationService) : BaseController
    {
        //Get request to get Manage Company page of an employee who has managing rights in company
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
                return Forbid();
            }

            var model = await _companyService.GetManageCompanyModel(companyGuid, permissionRole);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        //Get request to get Manage Employees page of an employee who has managing rights in company
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
                return Forbid();
            }

            var model = await _employeeInfoService.GetCompanyManageEmployeeInfoModel(companyGuid, searchQuery, searchFilter, currentPage, permissionRole);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        //Get request to get Manage Departments page of an employee who has managing rights in company
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
                return Forbid();
            }

            var model = await _departmentService.GetCompanyManageDepartmentsModel(companyGuid, currentPage);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        //Get request to get Manage Vacations page of an employee who has managing rights in company
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
                return Forbid();
            }

            //Check if the user's employee role needs to have a department
            //And if it does just get it
            Guid? userNeededDepartmentId = await _permissionService.GetUserNeededDepartmentId(companyGuid, userEmail);

            var model = await _vacationService.GetCompanyManageVacationsModelAsync(companyGuid, searchQuery, sortFilter, currentPage, permissionRole, userNeededDepartmentId);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }

        //Get request to get Manage Schedule page of an employee
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
            if (permissionRole < PermissionRole.Visitor)
            {
                return Forbid();
            }
            else if (permissionRole >= PermissionRole.Owner) //Don't allow owner as the owner doesn't have a schedule)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            string userId = GetCurrentUserId();

            var model = await _vacationService.GetCompanyManageScheduleModelAsync(companyGuid, sortFilter, currentPage, userId);

            //Check if entity exists
            if (model == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}
