using System.ComponentModel.DataAnnotations;

namespace Validation_Demo
{
    public sealed record CreateProductReqValidateObject(
         string Name,
    string Description,
    decimal Price
        ) : IValidatableObject
    {

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Ürün adı name is required.", new[] { nameof(Name) });
            }
            ;

            if (Price < 0.01m || Price > 10.0m)
            {
                yield return new ValidationResult("Price must be greater than zero.", [nameof(Price)]);
            }
            ;

            if (string.IsNullOrWhiteSpace(Description))
            {
                yield return new ValidationResult("Description is required.", new[] { nameof(Description) });
            }
            ;

        }
    }
}
