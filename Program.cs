
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Validation_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(
                 options =>
                 options.InvalidModelStateResponseFactory = context =>
                 {
                     // ModelState geçersizse döndürülecek özel response

                     var problem = new ValidationProblemDetails
                     {
                         Title = "Global Cs den -- Validasyon Patladu",
                         Status = StatusCodes.Status400BadRequest,
                         Type = "https://httpstatuses.com/400",
                         Detail = "Gelen istek validasyon kurallarina uymuyor. Lutfen isteginizi kontrol ediniz.",
                         Instance = context.HttpContext.Request.Path
                     };

                     //Ek meta veri ekleme  (örneðin ,traceId veya custom error codes)
                     problem.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                     problem.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                     foreach (var (key, errors) in context.ModelState)
                     {
                         var errorMessages = errors.Errors.Select(e => e.ErrorMessage).ToArray();
                         problem.Errors.Add(key, errorMessages);
                     }
                     // Hata mesajlarýný daha okunur hale getirmek isterseniz burada dönüþtürebilirsiniz.
                     // Örnek: alan adlarýný camelCase'e çevirme veya custom errorCode ekleme.

                     var result=new BadRequestObjectResult(problem)
                     {
                         ContentTypes = { "application/problem+json", "application/problem+xml" }
                     };
                     return result;

                 }
                );
           
            builder.Services.AddValidation();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Validators.CreateProductRequestValidator>();
            // Add validation services

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            
            var app = builder.Build();

            app.UseMiddleware<MiddleWare.TimingMiddleWare>();
            app.UseMiddleware<MiddleWare.ExceptionHandleMiddleWare>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                
                app.MapOpenApi();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
