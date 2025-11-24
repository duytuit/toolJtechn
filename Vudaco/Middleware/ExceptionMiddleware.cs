using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Vudaco.Controllers;
using System.Linq;
using Vudaco.Shares;

namespace Vudaco
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "Internal Server Error";
            string lineInfo = "";
            string fullDetail = "";

            try
            {
                Exception currentEx = exception;
                int depth = 0;

                // Lấy thông tin lỗi lồng nhau (InnerException)
                while (currentEx != null)
                {
                    var trace = currentEx.StackTrace;
                    var line = trace?
                        .Split('\n')
                        .LastOrDefault(l => l.Contains(":line"))?
                        .Trim() ?? "No line info";

                    fullDetail += $"\n[{depth}] {currentEx.GetType().Name}: {currentEx.Message} | {line}";
                    currentEx = currentEx.InnerException;
                    depth++;
                }

                message = exception.Message;
                lineInfo = fullDetail;
                await Helper.SendTelegramMessageAsync($"❌ Exception: {message}");
                _logger.LogError($"❌ Exception: {message}\nDetails:{fullDetail}");

                var response = new ApiResponse<object>(false, $"{message} | {fullDetail}");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                // fallback tránh crash middleware
                _logger.LogError($"[ExceptionMiddlewareError] {ex.Message}");
                await Helper.SendTelegramMessageAsync($"[ExceptionMiddlewareError] {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("{\"success\":false,\"message\":\"Middleware error\"}");
            }
        }
    }
}