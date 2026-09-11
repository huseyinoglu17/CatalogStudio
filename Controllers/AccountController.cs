using CatalogStudio.Models;
using CatalogStudio.Services;
using CatalogStudio.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace CatalogStudio.Controllers;

public class AccountController(UserManager<AppUser> users, SignInManager<AppUser> signIn, UserPreferenceService preferences, FileService files) : Controller
{
    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost] public async Task<IActionResult> Register(RegisterViewModel m) { if (ModelState.IsValid) { var u = new AppUser { FullName = m.FullName, UserName = m.Email, Email = m.Email }; var result = await users.CreateAsync(u, m.Password); if (result.Succeeded) { DbInitializer.Check(await users.AddToRoleAsync(u, "User")); return RedirectToAction("Login"); } foreach (var e in result.Errors) ModelState.AddModelError("", Translate(e)); } return View(m); }
    [HttpGet] public IActionResult Login() => View(new LoginViewModel());
    [HttpPost] public async Task<IActionResult> Login(LoginViewModel m, string? returnUrl) { if (ModelState.IsValid) { var u = await users.FindByEmailAsync(m.Email); if (u != null && u.IsActive && (await signIn.PasswordSignInAsync(u, m.Password, m.RememberMe, true)).Succeeded) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/"); ModelState.AddModelError("", "E-posta veya şifre hatalı; hesap kilitli veya pasif olabilir."); } return View(m); }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Login"); }
    [Authorize, HttpGet] public async Task<IActionResult> Profile() { var u = (await users.GetUserAsync(User))!; var p = await preferences.GetAsync(u.Id); ViewBag.HasLogo = p != null; return View(new ProfileViewModel { FullName = u.FullName ?? "", BrandName = p?.BrandName ?? "" }); }
    [Authorize, HttpPost] public async Task<IActionResult> Profile(ProfileViewModel m, CancellationToken ct) { var u = (await users.GetUserAsync(User))!; var p = await preferences.GetAsync(u.Id); ViewBag.HasLogo = p != null; if (ModelState.IsValid) { string? newLogo = null; try { if (m.Logo != null) newLogo = await files.SaveAsync(m.Logo, ct); if (p == null && newLogo == null) throw new InvalidOperationException("İlk kullanımda logo yükleyin."); u.FullName = m.FullName; DbInitializer.Check(await users.UpdateAsync(u)); await preferences.SaveAsync(u.Id, m.BrandName, newLogo ?? p!.LogoPath); TempData["Message"] = "Profil kaydedildi."; return RedirectToAction("Profile"); } catch (InvalidOperationException e) { if (newLogo != null) files.Delete(newLogo); ModelState.AddModelError("", e.Message); } } return View(m); }
    [HttpGet] public IActionResult ForgotPassword() => View();
    [HttpPost] public IActionResult ForgotPassword(string email) { ViewBag.Message = "Şifre sıfırlama için marka yöneticinizle iletişime geçin. E-posta gönderimi bu MVP'de henüz yapılandırılmamıştır."; return View(); }
    public IActionResult AccessDenied() { Response.StatusCode = 403; return View(); }
    static string Translate(IdentityError e) => e.Code.StartsWith("Password") ? "Şifre en az 8 karakter, büyük/küçük harf ve rakam içermelidir." : e.Code.StartsWith("Duplicate") ? "Bu e-posta zaten kayıtlı." : "Kayıt oluşturulamadı. Bilgilerinizi kontrol edin.";
}