using CatalogStudio.Data;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Areas.Admin.Controllers;
[Area("Admin"),Authorize(Roles="Admin")]
public class TokensController(AppDbContext db,TokenService tokens):Controller {
 public async Task<IActionResult> Index(string? search) {
 ViewBag.Search=search;
 var q=db.Users.AsNoTracking();
 if(!string.IsNullOrWhiteSpace(search)){var key=search.Trim().ToUpperInvariant();q=q.Where(x=>x.NormalizedEmail==key||x.NormalizedUserName==key);}
 var users=await q.OrderBy(x=>x.Email).Take(100).ToListAsync();
 var balances=new Dictionary<int,long>();foreach(var user in users)balances[user.Id]=await tokens.Balance(user.Id);
 ViewBag.Balances=balances;return View(users);
 }
 [HttpPost] public async Task<IActionResult> Change(string search,string operation,long amount) {
 if(!ModelState.IsValid)return BadRequest();
 var key=(search??"").Trim().ToUpperInvariant();
 var u=await db.Users.SingleOrDefaultAsync(x=>x.NormalizedEmail==key||x.NormalizedUserName==key);
 TempData["Message"]=u==null?"Kullanıcı bulunamadı.":await tokens.Change(u.Id,operation,amount)?"Token bakiyesi güncellendi.":"İşlem yapılamadı. Tutarı ve mevcut bakiyeyi kontrol edin.";
 return RedirectToAction("Index",new{search});
 }
}