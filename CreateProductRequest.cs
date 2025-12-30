using System.ComponentModel.DataAnnotations;
namespace Validation_Demo
{
    public record CreateProductRequest
    {
        [Required] public string Name { get; set; }
        [Range(0.01, double.MaxValue)] public decimal Price { get; set; }

        [Required] public string Description { get; set; }

    }
}
