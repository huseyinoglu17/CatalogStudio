using CatalogStudio.Data;
using CatalogStudio.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace CatalogStudio.Services;

// One completed catalog per user; failed generations never replace the current result.
public class CatalogRetentionService(AppDbContext db, FileService files, ILogger<CatalogRetentionService> logger)
{
    private static readonly SemaphoreSlim Gate = new(1);
    static IEnumerable<string> Names(Catalog c) =>
        new[] { c.GeneratedCatalogPath, c.MainProductImagePath, c.LogoSnapshotPath }
        .Concat(JsonSerializer.Deserialize<List<string>>(c.VariantPathsJson) ?? [])
        .Where(x => !string.IsNullOrWhiteSpace(x));

    public async Task Cleanup()
    {
        await Gate.WaitAsync();
        try
        {
            var catalogs = await db.Catalogs.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync();
            var obsolete = catalogs.GroupBy(x => x.UserId).SelectMany(x => x.Skip(1)).ToList();
            var oldIds = obsolete.Select(x => x.Id).ToHashSet();
            var protectedFiles = catalogs.Where(x => !oldIds.Contains(x.Id)).SelectMany(Names).ToHashSet(StringComparer.Ordinal);
            protectedFiles.UnionWith(await db.UserPreferences.Select(x => x.LogoPath).ToListAsync());
            foreach (var old in obsolete)
            {
                // Delete files first: if storage is temporarily unavailable the row remains for retry.
                foreach (var name in Names(old).Distinct())
                    if (!protectedFiles.Contains(name)) files.Delete(name);
                await db.Catalogs.Where(x => x.Id == old.Id).ExecuteDeleteAsync();
            }
            if (obsolete.Count > 0) logger.LogInformation("Removed {Count} previous catalogs and their files.", obsolete.Count);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            logger.LogWarning("Catalog cleanup failed ({Type}); will retry on startup or next successful generation.", ex.GetType().Name);
        }
        finally { Gate.Release(); }
    }
}
