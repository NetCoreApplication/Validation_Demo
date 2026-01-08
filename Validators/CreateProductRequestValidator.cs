using FluentValidation;

namespace Validation_Demo.Validators
{
    public class CreateProductRequestValidator:AbstractValidator<CreateProductRequest>
    {

        public CreateProductRequestValidator()
        {

            //Name kuralları
            RuleFor(x => x.Name)
                  .NotEmpty().WithMessage("Ürün adı gereklidir ").WithErrorCode("NAME_REQUIRED")
                  .WithState(_ => new { Severity = Severity.Error, ClientCode = "P1001" })
                  .Length(3, 100).WithMessage("Ürün adı 3 ile 100 karakter arasında olmalıdır").WithErrorCode("NAME_LENGTH")
                  .Must(name => !name.Contains("test", System.StringComparison.OrdinalIgnoreCase)).WithErrorCode("NAME_PROHIBITED_WORD")
                  .WithMessage("Ürün  adı 'test' kelimesini içeremez");

            //Price kuralları
            RuleFor(x => x.Price)
             .NotEmpty().WithMessage("Fiyat boş olamaz").WithErrorCode("PRICE_REQUIRED")
             .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.").WithErrorCode("PRICE_MIN")
             .LessThanOrEqualTo(100_000m).WithMessage("Fiyat kabul edilemeyecek kadar yüksek olamaz.").WithErrorCode("PRICE_MAX")
            // .PrecisionScale(2, 10, true).WithMessage("Fiyat en fazla 2 ondalık basamak içerebilir.").WithErrorCode("PROCE_SCALE");
            //Ondalık hassasiyet kontrölü (en fazla 2 ondalık basamak)
            .Must(price => DecimalScaleIsAtMost(price, 2)).WithMessage("Fiyat en fazla 2 ondalık basamak içerebilir").
            WithErrorCode("PRICE_SCALE")
            .WithState(_ => new { Severity = Severity.Warning, ClientCode = "P2211" });

         

            //Description kuralları 
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama gereklidir.").WithErrorCode("DESCRIPTION_REQUIRED")
                .MinimumLength(10).WithMessage("Açıklama en az 10 karakter olmalıdır.").WithErrorCode("DESCRIPTION_MIN_LENGTH")
                .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.").WithErrorCode("DESCRIPTION_MAX_LENGTH");

            //Koşullu kural : özel isimler için daha uzun açıklama isteği
            When(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("special", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.Description)
                .MinimumLength(20)
                .WithMessage("Özel ürünler için açıklama en az 20 karakter olmalıdır")
                .WithErrorCode("DESCRIPTON_MIN_FOR_SPECIAL");
            });

            //
            RuleFor(x => x.Sku)
                .Cascade(CascadeMode.Continue)
                .Matches(@"^[A-Z0-9\-]+$").When(x => !string.IsNullOrWhiteSpace(x.Sku))
                .WithMessage("SKU yalnızca büyük harfler, rakamlar ve tire içerebilir.").WithErrorCode("SKU_FORMAT")
                .MaximumLength(50).WithMessage("Sku en fazla 50 karakterli olabilir").WithErrorCode("SKU_MAX_LENGTH");

            //örnek farklı senaryolar için named ruleset (Create/Update)
            RuleSet("Create", () =>
            {
                // Create için ek kurallar eklenebilir (ör: zorunlu SKU)
                //RuleFor(x => x.Sku).NotEmpty().WithMessage("Create işlemi için SKU gereklidir.").WithErrorCode("SKU_REQUIRED_CREATE");

            });

            RuleSet("Update", () =>
            {
                // Update için farklı kurallar eklenebilir
            });
        }

        private static bool DecimalScaleIsAtMost(decimal value, int maxScale)
        {
            if (maxScale < 0) return false;
            value = Math.Abs(value);

            decimal scaled = value * (decimal)Math.Pow(10, maxScale);
            return decimal.Truncate(scaled) == scaled;
        }


        
    }
    
    
}
