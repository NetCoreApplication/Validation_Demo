using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Validation_Demo
{
    [Serializable]
    public record CreateProductReqDemoForValidate:IValidatableObject
    {
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ürün adı 3 ile 100 karakter arasında olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Açıklama gereklidir.")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        // İsteğe bağlı alan örneği; null olmasına izin veriyoruz
        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir.")]
        public string? Sku { get; set; }


        //Özel iş kurallarını tanımlayalım...
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Örnek iş kuralı : Name içinde "Test" geçemez
            if (!string.IsNullOrWhiteSpace(Name) && Name.Contains("test", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Ürün adı 'test' kelimesini içeremez.", new[] { nameof(Name) });
            }

            //Örnek iş kuralı
            if (Price > 100_000m)
            {
                yield return new ValidationResult("Fiyat kabul edilemeyecek kadar yüksek ", new[] { nameof(Price) });
            }

            //Sku için Özel Regex kontrolü
            if (!string.IsNullOrWhiteSpace(Sku))
            {
                var skuRegex = new Regex(@"^[A-Z0-9\-]+$");
                if (!skuRegex.IsMatch(Sku))
                {
                    yield return new ValidationResult("SKU yalnızca büyük harfler, rakamlar ve tire içerebilir.", new[] { nameof(Sku) });
                }
            }

            if (Description.Length < 10)
            {
                yield return new ValidationResult("Açıklama en az 10 karakter olmalıdır.", new[] { nameof(Description) });
            }
        }
    }
}
