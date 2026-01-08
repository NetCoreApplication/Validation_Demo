
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Validation_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // Add services to the container.
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
             {
                 options.InvalidModelStateResponseFactory = ctx =>
                 {
                     var logger = ctx.HttpContext.RequestServices.GetService<ILogger<Program>>();

                     logger.LogWarning("Validasyon patladu {Path}", ctx.HttpContext.Request.Path);

                     var problem = new ValidationProblemDetails()
                     {
                         Title = "Global Error - Validation Patladu",
                         Type = "https://httpstatuses.com/400",
                         Instance = ctx.HttpContext.Request.Path,
                         Detail = "Gelen Ýstek Validasyon kurallarýna uymuyor",
                         Status = StatusCodes.Status400BadRequest
                     };

                     problem.Extensions.Add("TraceId", ctx.HttpContext.TraceIdentifier);
                     problem.Extensions["TimeStamp"] = DateTimeOffset.UtcNow;


                     foreach(var (key_code,errors) in ctx.ModelState)
                     {

                         logger.LogWarning("ModelState Key: {Key}, IsValid: {IsValid}, ErrorCount: {Count}",
       key_code,
       errors.ValidationState,
       errors.Errors.Count);

                         var errorMessage = errors.Errors.Select(d => d.ErrorMessage).ToArray();
                         problem.Errors.Add(key_code, errorMessage);
                     }



                     return new BadRequestObjectResult(problem)
                     {
                         ContentTypes = { "application/problem+json", "application/problem+xml" }
                     };
                 };

             });
            /*builder.Services.AddControllers().ConfigureApiBehaviorOptions(
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

                     //FluentValidation  hata kodlarýný topla (varsa)
                     var svc = context.HttpContext.RequestServices;
                     var fluentValidationDetails = new List<object>();
                     // Hatalý kod:
                     // foreach(var arg in context.ActionArguments)

                     // Düzeltme:
                     // ActionArguments özelliði, ActionContext'te deðil, ControllerContext veya ControllerBase'de bulunur.
                     // Burada, FluentValidation hatalarýný toplamak için doðrudan ModelState üzerinden iþlem yapabilirsiniz.
                     // Eðer ActionArguments'e eriþmek istiyorsanýz, context parametresinin tipini ActionExecutingContext olarak deðiþtirmeniz gerekir.
                     // Ancak, burada ModelState üzerinden devam etmek daha uygundur. Aþaðýdaki satýrý kaldýrabilirsiniz:

                     // foreach(var arg in context.ActionArguments)
                     // {
                     //     var value = arg.Value;
                     // }

                     // Bu satýrlarý tamamen kaldýrýn, çünkü context.ActionArguments mevcut deðil ve kullanýlmýyor.
                     


                     var result=new BadRequestObjectResult(problem)
                     {
                         ContentTypes = { "application/problem+json", "application/problem+xml" }
                     };
                     return result;

                 }
                );*/
           
            builder.Services.AddValidation();
            //Modelstate ' e  errors ekler.. Otomatikmen Validasyoný Modelstate ekler ,
            // AddFluentValidationAutoValidation remarklarsan InvalidModelStateResponseFactory geçip Action içine geçer.
           // builder.Services.AddFluentValidationAutoValidation();
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
