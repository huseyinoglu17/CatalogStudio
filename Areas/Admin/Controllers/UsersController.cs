using CatalogStudio.Data;
using CatalogStudio.Models;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CatalogStudio.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class UsersController(AppDbContext db, UserManager<AppUser> users, FileService files) : Controller
{
    public async Task<IActionResult> Index() { var list = await db.Users.OrderByDescending(x => x.CreatedAt).ToListAsync(); var roles = new Dictionary<int, string>(); foreach (var u in list) roles[u.Id] = string.Join(", ", await users.GetRolesAsync(u)); ViewBag.Roles = roles; return View(list); }
    public async Task<IActionResult> Details(int id) { var u = await users.FindByIdAsync(id.ToString()); if (u == null) return NotFound(); ViewBag.Roles = string.Join(", ", await users.GetRolesAsync(u)); ViewBag.CatalogCount = await db.Catalogs.CountAsync(x => x.UserId == id); return View(u); }
    [HttpPost]
    public async Task<IActionResult> Change(int id, string operation)
    {
        if (id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier)) { TempData["Message"] = "Kendi hesabınız üzerinde bu işlem yapılamaz."; return RedirectToAction("Index"); }
        var u = await users.FindByIdAsync(id.ToString()); if (u == null) return NotFound();
        switch (operation)
        {
            case "deactivate": u.IsActive = false; DbInitializer.Check(await users.UpdateAsync(u)); break;
            case "activate": u.IsActive = true; DbInitializer.Check(await users.UpdateAsync(u)); break;
            case "grant": DbInitializer.Check(await users.AddToRoleAsync(u, "Admin")); break;
            case "revoke": DbInitializer.Check(await users.RemoveFromRoleAsync(u, "Admin")); break;
            case "delete":
                var catalogs = await db.Catalogs.Where(x => x.UserId == id).ToListAsync(); var p = await db.UserPreferences.SingleOrDefaultAsync(x => x.UserId == id);
                DbInitializer.Check(await users.DeleteAsync(u)); foreach (var c in catalogs) { files.DeleteCatalog(c); }
                files.Delete(p?.LogoPath); return RedirectToAction("Index");
            default: return BadRequest();
        }
        DbInitializer.Check(await users.UpdateSecurityStampAsync(u)); TempData["Message"] = "Kullanıcı güncellendi."; return RedirectToAction("Index");
    }
}
