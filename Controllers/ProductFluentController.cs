using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Validation_Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductFluentController
        : ControllerBase
    {
        private readonly IValidator<CreateProductRequest> _validator;

        public ProductFluentController(IValidator<CreateProductRequest> validator)
        {
            _validator = validator;
        }
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowDuplicateProperties = false,
            PropertyNameCaseInsensitive = true
        };

        [HttpPost("create_with_error_codes")]
        public IActionResult Create_With_Error_Codes([FromBody] CreateProductRequest request)
        {
            var validationResult = _validator.Validate(request);
            if (validationResult.IsValid)
            {
                return Ok(new { Message = "Product Created", Product = request });
            }

            //Hataları field bazlı topla
            var fieldErrors = validationResult.Errors.GroupBy(f =>

               string.IsNullOrEmpty(f.PropertyName) ? string.Empty : f.PropertyName
            ).ToDictionary(g => g.Key,
                           g => g.Select(f => f.ErrorMessage).ToArray()
                           );


            var problem = new ValidationProblemDetails(fieldErrors)
            {
                Title = "Kod içinde Validasyon failed oldu",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Fluent Validation kuralalarına uymuyor",
                Instance = HttpContext.Request.Path
            };

            //FluentValidation hata kodlarını extensions içine ekler

            var failureDetails = validationResult.Errors.Select(g => new
            {
                property = g.PropertyName,
                message = g.ErrorMessage,
                code = g.ErrorCode,
                customstate = g.CustomState
            }).ToArray();

            problem.Extensions.Add("validationFailures", failureDetails);
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            problem.Extensions["timestamp"] = DateTimeOffset.UtcNow;

            //return BadRequest(problem);
            return new BadRequestObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" }
            };

        }



        [HttpPost("create_manual_ruleset")]
        public IActionResult CreateManual([FromBody] CreateProductRequest request)
        {
            //Ruleset 'i manuel olarak belirtip doğruluyoruz
            var result = _validator.Validate(request, options =>
            {
                options.IncludeRuleSets("Create");
            });

            if (!result.IsValid)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            return Ok(new
            {
                Message = "Created using Create ruleset (manual)",
                Product = request
            });

        }
        

        [HttpPost("create_with_ruleset")]
        public IActionResult Create_With_RuleSet([CustomizeValidator(RuleSet = "Create")][FromBody] CreateProductRequest request)
        {
            // Eğer aksiyon çağrıldıysa validator "Create" ruleset'ini geçti
            return Ok(new { Message = "Created using Create ruleset", Product = request });
        }
        
        // FluentValidation otomatik olarak çalışır
        // (InvalidModelStateResponseFactory devredeyse 400 döner)
        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateProductRequest request)
        {
            //Eğer aksiyon çağrıldıysa model geçerli demektir (veya InvalidModelStateResponseFactory sizi yakaladı)
            return Ok(new { Message = "Product created (FluentValidation passed)", Product = request });

        }

        //Tek aksiyon için Fluent Validation ' ı atlamak CustomizeValidator attribute ile
        [HttpPost("create_skip_fluent")]
        public IActionResult CreateSkipFluent([CustomizeValidator(Skip = true)][FromBody] CreateProductRequest request)
        {
            //Bu aksiyon da fluent validation atlanır. 
            //Manuel validasyon yapılabilir.
            return Ok(new { Message = "Validation Skipped for this action", Product = request });
        }

        //Alternatif : ham json alıp manuel deserialize ederek validation atlanır
        [HttpPost("create_with_json")]
        public IActionResult CreateWithJson([FromBody] JsonElement body)
        {
            CreateProductRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<CreateProductRequest?>(body.GetRawText(), _jsonSerializerOptions);
            }
            catch (JsonException)
            {

                return BadRequest("Geçersiz JsON");

            }

            //Burada FluentValidation otomatik çalışmaz- isterseniz manuel olarak Validate isteyebiliriz.
            return Ok(new { Message = "Received JSON without automatic validation", Product = request });
        }
    }
}
