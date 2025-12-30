using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Validation_Demo
{
    public sealed record CreateProductReqValidateObject(
         string Name,
         string Email,
    string Description,
    decimal Price
        ) : IValidatableObject
    {
        private static readonly Regex EmailRegex = new(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex NameRegex = new(@"^[\p{L}\p{N}\s\-\._]+$", RegexOptions.Compiled);
        private const int MinNameLength = 2;
        private const int MaxNameLength = 100;
        private const int MinDescriptionLength = 10;
        private const int MaxDescriptionLength = 1000;
        private const int MaxPriceDecimalPlaces = 2;
        private const int MaxEmailLength = 254;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Email validasyonu
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("Email gereklidir.", new[] { nameof(Email) });
            }
            else
            {
                if (Email.Length > MaxEmailLength)
                {
                    yield return new ValidationResult($"Email en fazla {MaxEmailLength} karakter olabilir.", new[] { nameof(Email) });
                }

                if (!EmailRegex.IsMatch(Email))
                {
                    yield return new ValidationResult("Geçersiz email formatı.", new[] { nameof(Email) });
                }
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Ürün adı (Name) gereklidir.", new[] { nameof(Name) });
            }

            if (!string.IsNullOrWhiteSpace(Name) && (Name.Length < MinNameLength || Name.Length > MaxNameLength))
            {
                yield return new ValidationResult($"Name {MinNameLength} ile {MaxNameLength} karakter arasında olmalıdır.", new[] { nameof(Name) });
            }

            if (!string.IsNullOrWhiteSpace(Name) && !NameRegex.IsMatch(Name))
            {
                yield return new ValidationResult("Name geçersiz karakterler içeriyor.", new[] { nameof(Name) });
            }

            if (Price < 0.01m || Price > 10.0m)
            {
                yield return new ValidationResult("Price must be between 0.01 and 10.0.", new[] { nameof(Price) });
            }

            // Ondalık hassasiyet kontrolü (ör. en fazla 2 ondalık basamak)
            if (decimal.Round(Price, MaxPriceDecimalPlaces) != Price)
            {
                yield return new ValidationResult($"Price en fazla {MaxPriceDecimalPlaces} ondalık basamak içerebilir.", new[] { nameof(Price) });
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                yield return new ValidationResult("Description is required.", new[] { nameof(Description) });
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length < MinDescriptionLength)
            {
                yield return new ValidationResult($"Description en az {MinDescriptionLength} karakter olmalıdır.", new[] { nameof(Description) });
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > MaxDescriptionLength)
            {
                yield return new ValidationResult($"Description en fazla {MaxDescriptionLength} karakter olabilir.", new[] { nameof(Description) });
            }

            // Basit HTML tag kontrolü (isteğe bağlı)
            if (!string.IsNullOrWhiteSpace(Description) && Regex.IsMatch(Description, "<.*?>"))
            {
                yield return new ValidationResult("Description HTML tag'ları içeremez.", new[] { nameof(Description) });
            }

        }
    }
}
