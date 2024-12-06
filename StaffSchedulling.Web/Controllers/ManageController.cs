using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffScheduling.Common.Enums.Filters;
using StaffScheduling.Web.Models.ViewModels.Vacation;
using StaffScheduling.Web.Services.DbServices.Contracts;
using static StaffScheduling.Common.Enums.CustomRoles;

namespace StaffScheduling.Web.Controllers
{
    [Authorize]
    public class ManageController(IPermissionService _permissionService, ICompanyService _companyService, IEmployeeInfoService _employeeInfoService, IDepartmentService _departmentService) : BaseController
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
                return RedirectToAction("Index", "Dashboard");
            }

            //TODO: CREATE MANAGE SCHEDULE GET MODEL FUNCTION
            //var model = await _vacationService.GetManageScheduleModel(companyGuid, sortFilter, currentPage);

            var model = new ManageScheduleViewModel()
            {
                CompanyId = companyGuid,
                EmployeeId = Guid.NewGuid(),
                CurrentPage = 1,
                TotalPages = 2,
                SortFilter = VacationSortFilter.All,
                VacationDaysLeft = 15,
                Vacations = new List<VacationViewModel>
                {
                    new VacationViewModel
                    {
                        CompanyId = Guid.NewGuid(),
                        EmployeeId = Guid.NewGuid(),
                        Days = 5,
                        StartDate = DateOnly.FromDateTime(DateTime.Now),
                        EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
                        Id = Guid.NewGuid(),
                        Status = Common.Enums.VacationStatus.Pending
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
    }
}
