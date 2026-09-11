using CatalogStudio.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class BrandsController(AppDbContext db) : Controller { public async Task<IActionResult> Index() => View(await db.UserPreferences.Include(x => x.User).OrderByDescending(x => x.CreatedAt).ToListAsync()); }
