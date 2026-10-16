using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Employees.Controllers
{
    public class EmployeeController : Controller
    {
        [HttpGet("admin/employees")]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý nhân viên";
            return View("~/Modules/Employees/Views/Employees.cshtml");
        }

        [HttpGet("admin/employees/create")]
        [Authorize(Policy = "Permission:EMPLOYEES.CREATE")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm nhân viên";
            return View("~/Modules/Employees/Views/CreateUpdate.cshtml");
        }

        [HttpGet("admin/employees/{id:int}/edit")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Cập nhật nhân viên";
            ViewData["EmployeeId"] = id;
            return View("~/Modules/Employees/Views/CreateUpdate.cshtml");
        }
    }
}
