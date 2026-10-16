using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Users.Controllers
{
    public class RolePagesController : Controller
    {
        [HttpGet("admin/roles")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý phân quyền";
            return View("~/Modules/Users/Views/Roles.cshtml");
        }

        [HttpGet("admin/employee-roles")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        public IActionResult EmployeeRoles()
        {
            ViewData["Title"] = "Gán vai trò nhân viên";
            return View("~/Modules/Users/Views/EmployeeRoles.cshtml");
        }
    }
}
