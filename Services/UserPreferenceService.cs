using CatalogStudio.Data;
using CatalogStudio.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Services;

public class UserPreferenceService(AppDbContext db, FileService files)
{
    public Task<UserPreference?> GetAsync(int id) => db.UserPreferences.SingleOrDefaultAsync(x => x.UserId == id);
    public async Task SaveAsync(int id, string brand, string logo) { var p = await GetAsync(id); string? old = p?.LogoPath; if (p == null) { p = new() { UserId = id }; db.Add(p); } p.BrandName = brand; p.LogoPath = logo; p.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); if (old != logo) files.Delete(old); }
}
