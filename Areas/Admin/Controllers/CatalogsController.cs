using CatalogStudio.Data;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CatalogsController(AppDbContext db, FileService files) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Catalogs.Include(x => x.User).OrderByDescending(x => x.CreatedAt).ToListAsync());
    [HttpPost] public async Task<IActionResult> Delete(int id) { var c = await db.Catalogs.FindAsync(id); if (c == null) return NotFound(); db.Remove(c); await db.SaveChangesAsync(); files.DeleteCatalog(c); return RedirectToAction("Index"); }
}
