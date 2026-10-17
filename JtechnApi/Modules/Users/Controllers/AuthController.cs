
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
        private readonly JtechnApi.Users.Repositories.IAuthenticationRepository repository;

        public AuthController(JtechnApi.Users.Repositories.IAuthenticationRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("/admin/login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Đăng nhập";
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost("/admin/login")]
        public async Task<IActionResult> Login([FromForm] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["LoginError"] = "Vui lòng nhập tên đăng nhập.";
                return Redirect("/admin/login");
            }

            var claims = await repository.GetLoginClaims(username);
            if (claims == null)
            {
                TempData["LoginError"] = "Tên đăng nhập không hợp lệ hoặc tài khoản đã bị vô hiệu hóa.";
                return Redirect("/admin/login");
            }

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

        [HttpGet("/admin/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/admin/login");
        }

        [HttpGet("/admin/access-denied")]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Không có quyền truy cập";
            return View("~/Views/Home/AccessDenied.cshtml");
        }

    }
}
