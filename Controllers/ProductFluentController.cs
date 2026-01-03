using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Validation_Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductFluentController
        : ControllerBase
    {

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowDuplicateProperties = false,
            PropertyNameCaseInsensitive = true
        };

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
