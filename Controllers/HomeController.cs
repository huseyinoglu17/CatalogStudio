using CatalogStudio.Models;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CatalogStudio.Controllers;

public class HomeController(UserPreferenceService preferences) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index(string? mode = null, int? count = null)
    {
        if (mode != "front" && mode != "back") return View("Choose");
        if (count is null or < 1 or > 10)
        {
            ViewBag.CatalogMode = mode;
            return View("ColorCount");
        }
        var preference = await preferences.GetAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        ViewBag.Preference = preference;
        return View(new CatalogRequest
        {
            BackPrint = mode == "back", ColorCount = count,
            BrandName = preference?.BrandName ?? "", MinimumAge = 9, MaximumAge = 24
        });
    }
    [IgnoreAntiforgeryToken] public IActionResult Error() { Response.StatusCode = 500; return View(); }
}
