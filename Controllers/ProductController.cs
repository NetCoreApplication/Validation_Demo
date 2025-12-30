using Microsoft.AspNetCore.Mvc;

namespace Validation_Demo.Controllers
{


    [ApiController]
    [Route("api/[controller]")]

    public class ProductController:ControllerBase
    {
        public ProductController() { }

        [HttpPost("create")]
        public IActionResult CreateProduct([FromBody] CreateProductRequest request)
        {
            // If the model is valid, proceed with creating the product
            return Ok("Validation SuccessFul");
            //   return Ok(new { Message = "Product created successfully", Product = request });
        }

        [HttpPost("create_ivalidate")]
        public IActionResult CreateProductValidate([FromBody] CreateProductReqValidateObject request)
        {
            // If the model is valid, proceed with creating the product
            return Ok("Validation SuccessFul");
            //   return Ok(new { Message = "Product created successfully", Product = request });
        }

        [HttpPost("create_with_novalidation")]
        public IActionResult CreateProductWithoutValidation([FromBody] CreateProductRequest request)
        {
            ModelState.Clear();
            // Bypass validation and proceed with creating the product
            return Ok("No Validation Performed");
            // return Ok(new { Message = "Product created without validation", Product = request });
        }
    }
}
