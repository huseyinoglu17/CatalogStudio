using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace CatalogStudio.Models;

public enum ModelGender { Girl, Boy }
public enum AgeUnit { Months, Years }
public record AgeRange(int MinimumAge, int MaximumAge, AgeUnit AgeUnit) { public override string ToString() => $"{MinimumAge}-{MaximumAge} {(AgeUnit == AgeUnit.Months ? "months" : "years")}"; }
public class AppUser : IdentityUser<int> { public string? FullName { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public bool IsActive { get; set; } = true; }
public class UserPreference { public int Id { get; set; } public int UserId { get; set; } public AppUser User { get; set; } = null!; [MaxLength(100)] public string BrandName { get; set; } = ""; public string LogoPath { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; }
public class Catalog { public bool? HasPockets {get;set;} public int? ModelAge {get;set;} public AgeUnit? ModelAgeUnit {get;set;} public string BrandName {get;set;}=""; public string LogoSnapshotPath {get;set;}=""; public string VariantPathsJson {get;set;}="[]"; public ModelGender? ModelGender { get; set; } public int? SeriesCount { get; set; } public int Id { get; set; } public int UserId { get; set; } public AppUser User { get; set; } = null!; public int ProductCode { get; set; } public int MinimumAge { get; set; } public int MaximumAge { get; set; } public AgeUnit AgeUnit { get; set; } public string MainProductImagePath { get; set; } = ""; public string GeneratedCatalogPath { get; set; } = ""; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public AgeRange Age => new(MinimumAge, MaximumAge, AgeUnit); }
public class CatalogRequest : IValidatableObject
{
    [Required(ErrorMessage = "Marka adı zorunludur."), StringLength(100)] public string BrandName { get; set; } = "";
    [Required, EnumDataType(typeof(ModelGender))] public ModelGender? ModelGender { get; set; }
    [Required, Range(1,1000)] public int? SeriesCount { get; set; }
    [Required(ErrorMessage="Cep durumunu seçin.")] public bool? HasPockets {get;set;}
    [Required(ErrorMessage="Manken yaşını girin."), Range(1,216)] public int? ModelAge {get;set;}
    [Required, EnumDataType(typeof(AgeUnit))] public AgeUnit? ModelAgeUnit {get;set;}
    public IFormFile? Logo { get; set; }
    public bool UseSavedLogo { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Pozitif bir ürün kodu girin.")] public int ProductCode { get; set; }
    [Range(0, 216)] public int MinimumAge { get; set; }
    [Range(1, 216)] public int MaximumAge { get; set; }
    [EnumDataType(typeof(AgeUnit))] public AgeUnit AgeUnit { get; set; }
    [Required(ErrorMessage = "Mankene giydirilecek ürün fotoğrafı zorunludur.")] public IFormFile MainModelProductImage { get; set; } = null!;
    public List<IFormFile> VariantImages { get; set; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ModelAgeUnit == CatalogStudio.Models.AgeUnit.Years && ModelAge > 18) yield return new("Manken yaşı en fazla 18 olabilir.", [nameof(ModelAge)]);
        if (MinimumAge > MaximumAge) yield return new("Minimum yaş maksimum yaştan büyük olamaz.", [nameof(MinimumAge)]);
        if (AgeUnit == AgeUnit.Years && MaximumAge > 18) yield return new("Çocuk yaşı en fazla 18 olabilir.", [nameof(MaximumAge)]);
        if (VariantImages.Count is < 1 or > 10) yield return new("1-10 renk fotoğrafı yükleyin.", [nameof(VariantImages)]);
    }
}
