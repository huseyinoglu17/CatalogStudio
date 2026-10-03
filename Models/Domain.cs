using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace CatalogStudio.Models;

public enum ModelGender { Girl, Boy }
public enum DecorationType { Auto, Embroidery, Print, Mixed }
public enum AgeUnit { Months, Years }
public record AgeRange(int MinimumAge, int MaximumAge, AgeUnit AgeUnit) { public override string ToString() => $"{MinimumAge}-{MaximumAge} {(AgeUnit == AgeUnit.Months ? "months" : "years")}"; }
public class AppUser : IdentityUser<int> { public string? FullName { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public bool IsActive { get; set; } = true; }
public class UserPreference { public int Id { get; set; } public int UserId { get; set; } public AppUser User { get; set; } = null!; [MaxLength(100)] public string BrandName { get; set; } = ""; public string LogoPath { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; }
public class Catalog { public int WhiteOutputWidth {get;set;}=1400; public int WhiteOutputHeight {get;set;}=1100; public DecorationType DecorationType {get;set;} public string DetailImagePath {get;set;}=""; public string RegenerationReason {get;set;}=""; public string GenerationInstructions {get;set;}=""; public string RegenerationChanges {get;set;}=""; public int ColorTokenPrice {get;set;}=75; public int OutputWidth {get;set;}=1400; public int OutputHeight {get;set;}=1100; public string WhiteModelPathsJson {get;set;}="[]"; public bool BackPrint {get;set;} public bool? HasPockets {get;set;} public int? ModelAge {get;set;} public AgeUnit? ModelAgeUnit {get;set;} public string BrandName {get;set;}=""; public string LogoSnapshotPath {get;set;}=""; public string VariantPathsJson {get;set;}="[]"; public ModelGender? ModelGender { get; set; } public int? SeriesCount { get; set; } public int Id { get; set; } public int UserId { get; set; } public AppUser User { get; set; } = null!; public int ProductCode { get; set; } public int MinimumAge { get; set; } public int MaximumAge { get; set; } public AgeUnit AgeUnit { get; set; } public string MainProductImagePath { get; set; } = ""; public string GeneratedCatalogPath { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public AgeRange Age => new(MinimumAge, MaximumAge, AgeUnit); }
public class CatalogRequest : IValidatableObject
{
    [Required(ErrorMessage = "Marka adı zorunludur."), StringLength(100)] public string BrandName { get; set; } = "";
    [Required, EnumDataType(typeof(ModelGender))] public ModelGender? ModelGender { get; set; }
    [Required, Range(1,1000)] public int? SeriesCount { get; set; }
    [Required(ErrorMessage="Cep durumunu seçin.")] public bool? HasPockets {get;set;}
    [Required(ErrorMessage="Manken yaşını girin."), Range(1,216)] public int? ModelAge {get;set;}
    [Required, EnumDataType(typeof(AgeUnit))] public AgeUnit? ModelAgeUnit {get;set;}
    public bool BackPrint {get;set;}
    public int? ColorCount {get;set;}
    [EnumDataType(typeof(DecorationType))] public DecorationType DecorationType {get;set;}
    public IFormFile? EmbroideryDetailImage {get;set;}
    public int? QuotedColorTokenPrice {get;set;}
    public bool WhiteCustomSize {get;set;}
    public int? WhiteOutputWidth {get;set;}
    public int? WhiteOutputHeight {get;set;}
    public bool CustomSize {get;set;}
    public int? OutputWidth {get;set;}
    public int? OutputHeight {get;set;}
    public List<IFormFile> FrontImages {get;set;} = [];
    public List<IFormFile> BackImages {get;set;} = [];
    public IFormFile? Logo { get; set; }
    public bool UseSavedLogo { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Pozitif bir ürün kodu girin.")] public int ProductCode { get; set; }
    [Range(0, 216)] public int MinimumAge { get; set; }
    [Range(1, 216)] public int MaximumAge { get; set; }
    [EnumDataType(typeof(AgeUnit))] public AgeUnit AgeUnit { get; set; }
    public IFormFile? MainModelProductImage { get; set; } = null!;
    public List<IFormFile> VariantImages { get; set; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ModelAgeUnit == CatalogStudio.Models.AgeUnit.Years && ModelAge > 18) yield return new("Manken yaşı en fazla 18 olabilir.", [nameof(ModelAge)]);
        if (MinimumAge > MaximumAge) yield return new("Minimum yaş maksimum yaştan büyük olamaz.", [nameof(MinimumAge)]);
        if (AgeUnit == AgeUnit.Years && MaximumAge > 18) yield return new("Çocuk yaşı en fazla 18 olabilir.", [nameof(MaximumAge)]);
        if (ColorCount is null or < 1 or > 10) yield return new("1-10 arasında toplam renk sayısı girin.", [nameof(ColorCount)]);
        if (CustomSize && (OutputWidth is null or < 256 or > 4096 || OutputHeight is null or < 256 or > 4096)) yield return new("Özel boyutta genişlik ve yüksekliği 256-4096 piksel arasında girin.", [nameof(OutputWidth),nameof(OutputHeight)]);
        if (WhiteCustomSize && (WhiteOutputWidth is null or < 256 or > 4096 || WhiteOutputHeight is null or < 256 or > 4096)) yield return new("Beyaz fonlu fotoğrafın genişlik ve yüksekliğini 256-4096 piksel arasında girin.", [nameof(WhiteOutputWidth),nameof(WhiteOutputHeight)]);
        if (BackPrint) {

            if (FrontImages.Count != ColorCount || BackImages.Count != 1) yield return new("Her renk için ön fotoğraf ve yalnızca ilk renk için bir arka fotoğraf yükleyin.", [nameof(FrontImages)]);
        } else if (MainModelProductImage == null) yield return new("Mankene giydirilecek ürün fotoğrafı zorunludur.", [nameof(MainModelProductImage)]);
        if (!BackPrint && (VariantImages.Count > 9 || VariantImages.Count != ColorCount - 1)) yield return new("İlk renk dışında kalan her renk için bir fotoğraf yükleyin; tek renk için başka fotoğraf gerekmez.", [nameof(VariantImages)]);
    }
}

public class RegenerationRequest {
 [Required(ErrorMessage="Yeniden üretme nedenini yazın."),StringLength(500)] public string Reason {get;set;}="";
 [Required(ErrorMessage="Yapılmasını istediğiniz değişiklikleri yazın."),StringLength(2000)] public string Changes {get;set;}="";
}
public class TokenPricing {public int Id {get;set;}=1; public int ColorTokenPrice {get;set;}=75;}
