
using JtechnApi.Shares.BaseRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JtechnApi.Users.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AuthController : Controller
    {
        private readonly DBJtecContext _employeeContext;

        public AuthController(DBJtecContext employeeContext)
        {
            _employeeContext = employeeContext;
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Đăng nhập";
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromForm] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["LoginError"] = "Vui lòng nhập tên đăng nhập.";
                return Redirect("/login");
            }

            var normalizedUsername = username.Trim();
            var employee = await _employeeContext.Employee
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.Username == normalizedUsername && item.IsActive);

            if (employee == null)
            {
                TempData["LoginError"] = "Tên đăng nhập không hợp lệ hoặc tài khoản đã bị vô hiệu hóa.";
                return Redirect("/login");
            }

            var roles = await (
                from assignment in _employeeContext.RoleEmployees.AsNoTracking()
                join role in _employeeContext.Roles.AsNoTracking()
                    on assignment.RoleId equals role.Id
                where assignment.EmployeeId == employee.Id && role.IsActive
                select role.Code)
                .Distinct()
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim("FullName", employee.FullName ?? employee.Username)
            };
            claims.AddRange(roles.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return Redirect("/admin");
        }

        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }

        [HttpGet("/access-denied")]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Không có quyền truy cập";
            return View("~/Views/Home/AccessDenied.cshtml");
        }

    }
}
