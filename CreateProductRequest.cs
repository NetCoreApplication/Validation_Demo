using System.ComponentModel.DataAnnotations;
namespace Validation_Demo
{
    public record CreateProductRequest
    {
         public string Name { get; set; }
         public decimal Price { get; set; }

         public string Description { get; set; }

          public string Sku { get; set; }

    }
}
