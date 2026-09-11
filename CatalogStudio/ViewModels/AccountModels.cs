using System.ComponentModel.DataAnnotations;
namespace CatalogStudio.ViewModels;

public class LoginViewModel { [Required, EmailAddress, Display(Name = "E-posta")] public string Email { get; set; } = ""; [Required, DataType(DataType.Password), Display(Name = "Şifre")] public string Password { get; set; } = ""; [Display(Name = "Beni hatırla")] public bool RememberMe { get; set; } }
public class RegisterViewModel : LoginViewModel { [Required, StringLength(100), Display(Name = "Ad Soyad")] public string FullName { get; set; } = ""; [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor."), DataType(DataType.Password), Display(Name = "Şifre tekrar")] public string ConfirmPassword { get; set; } = ""; }
public class ProfileViewModel { [Required, StringLength(100), Display(Name = "Ad Soyad")] public string FullName { get; set; } = ""; [Required, StringLength(100), Display(Name = "Marka adı")] public string BrandName { get; set; } = ""; public IFormFile? Logo { get; set; } }
