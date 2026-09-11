using CatalogStudio.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DashboardController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index() { ViewBag.Users = await db.Users.CountAsync(); ViewBag.Catalogs = await db.Catalogs.CountAsync(); ViewBag.Today = await db.Catalogs.CountAsync(x => x.CreatedAt >= DateTime.UtcNow.Date); ViewBag.Brands = await db.UserPreferences.CountAsync(); ViewBag.RecentUsers = await db.Users.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(); return View(await db.Catalogs.Include(x => x.User).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync()); }
}
