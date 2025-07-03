using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using JtechnApi.Controllers;
using Microsoft.Extensions.Logging;

namespace JtechnApi.Infra
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

            if (exception is Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx)
            {
                statusCode = 500;
                message = dbUpdateEx.InnerException?.Message ?? dbUpdateEx.Message;
            }
            else
            {
                message = exception.Message;
            }
            // Ghi log lỗi
            _logger.LogError(message);
            var response = new ApiResponse<object>(false,message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}