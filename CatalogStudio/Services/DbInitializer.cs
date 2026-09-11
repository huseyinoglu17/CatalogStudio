using CatalogStudio.Models;
using Microsoft.AspNetCore.Identity;
namespace CatalogStudio.Services;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services, IHostEnvironment env, IConfiguration config)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole<int>>>(); var users = services.GetRequiredService<UserManager<AppUser>>();
        foreach (var role in new[] { "Admin", "User" }) if (!await roles.RoleExistsAsync(role)) Check(await roles.CreateAsync(new(role)));
        var email = env.IsDevelopment() ? "admin@catalog.local" : config["ADMIN_EMAIL"]; var password = env.IsDevelopment() ? "Admin1234!" : config["ADMIN_PASSWORD"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) { services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed").LogWarning("ADMIN_EMAIL / ADMIN_PASSWORD eksik; varsayılan admin oluşturulmadı."); return; }
        if (await users.FindByEmailAsync(email) != null) {services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed").LogInformation("Admin seed skipped: configured account already exists.");return;}
        var user = new AppUser { UserName = email, Email = email, FullName = "Katalog Yöneticisi", EmailConfirmed = true }; Check(await users.CreateAsync(user, password)); Check(await users.AddToRolesAsync(user, ["Admin", "User"])); services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed").LogInformation("Admin seed completed.");
    }
    public static void Check(IdentityResult result) { if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description))); }
}
