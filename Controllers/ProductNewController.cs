using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Validation_Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductNewController
        : ControllerBase
    {
        // CA1869: JsonSerializerOptions örneğini önbelleğe al
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };


        [HttpPost("create")]
        public IActionResult CreateProductValidate([FromBody] CreateProductReqDemoForValidate request)
        {
            // Bu aksiyon için benzer şekilde iş kurallarını uygulayabilirsiniz.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            return Ok("Validation SuccessFul");
        }


        [HttpPost("create_with_bs")]
        public IActionResult CreateProduct([FromBody] CreateProductReqDemoForValidate request)
        {
            //Model binding ve model içi (IValidatableObject / DataAnnotations) doğrulamaları 
            //Program.cs içinde InvalidModelStateResponseFactory configure edildiği için
            //model geçersizse aksiyon çağrılmayacaktır ve framework otomatik olarak 400 dönecektir.
            //Bu aksiyon çağrıldıysa model anotasyon doğrulamaları geçmiş demektir
            //Yinede iş kurallarını yazalım

            //Örnek iş kuralları 
            if (request.Price > 100_000m)
            {
                ModelState.AddModelError(nameof(request.Price), "Fiyat kabul edilemeyecek kadar yüksek.");
            }
            if (request.Name.Contains("prohibited", System.StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(request.Name), "Ürün adı izin verilmeyen bir kelime içeriyor");
            }

            //Eğer ek iş kurallarından dolayı hatalar varsa ,standart ValidationProblem dön
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            //Doğrulama başarılıysa iş mantığını çalıştırın
            return Ok(new { Message = "Product Created successfully", Product = request });

        }
       

        [HttpPost("create_with_json")]
        public IActionResult CreateProductWithJson([FromBody] JsonElement request)
        {
            //Ham Json aldığımız için Api Controller'ın otomatik model-state kısa devresi tetklenmez.
            //Burada manuel olarak validasyon yapabiliriz.

            CreateProductReqDemoForValidate? product = null;
            try
            {
                // CS0029: Doğru değişken ataması
                product = JsonSerializer.Deserialize<CreateProductReqDemoForValidate>(request.GetRawText(), _jsonOptions);
            }
            catch (JsonException)
            {
                return BadRequest("Geçersiz JSON.");
            }

            if (product is null)
                return BadRequest("Gönderilen JSON, beklenen modele dönüştürülemedi.");

            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(product!);

            bool isValid = Validator.TryValidateObject(
                product!,
                ctx,
                validationResults,
                validateAllProperties: true
                );

            //IValidatableObject.Validate 'i elle çağırarak kesinlikle tetiklenmesini sağlıyoruz.
            /*if (product is IValidatableObject validatable)
            {
                foreach (var vr in validatable.Validate(ctx))
                {
                    validationResults.Add(vr);
                }

                /* var customResults = validatable.Validate(ctx);
                 validationResults.AddRange(customResults);
            }*/


            if (validationResults.Any())
            {
                //ValidationResults içeriğini ModelState'e ekleyip Validation Problem dönebiliriz.
                foreach (var vr in validationResults) {
                    var memberName = vr.MemberNames.FirstOrDefault() ?? string.Empty;
                    ModelState.AddModelError(memberName, vr.ErrorMessage ?? string.Empty);
                }

                return ValidationProblem(ModelState);

            }

            // Doğrulamayı tamamen atla, işlem devam etsin
            return Ok(new { Message = "No Validation Performed", Product = request });

        }
    }
}
