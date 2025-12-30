using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Validation_Demo.MiddleWare
{
    public class ExceptionHandleMiddleWare
    {
        public readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandleMiddleWare> _logger;
        private readonly IWebHostEnvironment _env;
        public ExceptionHandleMiddleWare(RequestDelegate next, ILogger<ExceptionHandleMiddleWare> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);

            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = (int)StatusCodes.Status500InternalServerError;
            object payload;

            if(_env.IsDevelopment())
            {
                //Development ortamında detaylı hata bilgisi döner
                payload = new
                {
                    title = "Internal Server Error",
                    StatusCode = statusCode,
                    detail = exception.Message,
                    instance = context.Request.Path,
                    ActivityTraceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                    errortype = exception.GetType().Name,
                    stacktrace = exception.StackTrace,
                    timestamp = DateTimeOffset.UtcNow,

                    /*Message = exception.Message,
                    StackTrace = exception.StackTrace*/
                };
            }
            else
            {
                // Production: genel/güvenli mesaj
                payload = new
                {
                    title = "Internal Server Error",
                    statusCode,
                    detail = "Beklenmeyen bir hata oluştu.",
                    instance = context.Request.Path,
                    traceId = context.TraceIdentifier,
                    timestamp = DateTimeOffset.UtcNow
                };
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;

            var json=JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
             /*context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error",
                Detailed = _env.IsDevelopment() ? exception.StackTrace : null
            };
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, options));*/


        }
    }



    
}
