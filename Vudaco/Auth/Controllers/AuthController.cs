using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Repositories;
using Vudaco.Controllers;

namespace Vudaco.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IAuthRepository _authRepository;
      
        //int userId = (int)HttpContext.Items["UserId"];
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
          
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            try
            {
                var user = await _authRepository.RegisterAsync(request);
                return ApiResponseResult(true, "Đăng ký thành công", new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName
                });
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (accessToken, refreshToken) = await _authRepository.LoginAsync(request);
                return ApiResponseResult(true, "Đăng nhập thành công", new
                {
                    accessToken,
                    refreshToken
                });
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshTokenRequest request)
        {
            try
            {
                var (accessToken, refreshToken) = await _authRepository.RefreshTokenAsync(request);
                return ApiResponseResult(true, "Làm mới token thành công", new
                {
                    accessToken,
                    refreshToken
                });
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromForm] RefreshTokenRequest request)
        {
            try
            {
                await _authRepository.LogoutAsync(request.UserId, request.DeviceId);
                return ApiResponseResult<object>(true, "Đăng xuất thành công");
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message);
            }
        }
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            try
            {
                int userId = (int)HttpContext.Items["UserId"];
                await _authRepository.LogoutAllAsync(userId);
                return ApiResponseResult<object>(true, "Đã đăng xuất khỏi tất cả thiết bị");
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, ex.Message);
            }
        }
    }
}
