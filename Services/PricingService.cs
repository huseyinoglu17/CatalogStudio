using CatalogStudio.Data;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Services;
public class PricingService(AppDbContext db)
{
    public async Task<int> GetColorPrice()
    {
        await db.Database.ExecuteSqlRawAsync("INSERT OR IGNORE INTO TokenPricing (Id, ColorTokenPrice) VALUES (1, 75)");
        return await db.TokenPricing.AsNoTracking().Where(x=>x.Id==1).Select(x=>x.ColorTokenPrice).SingleAsync();
    }
    public async Task<bool> SetColorPrice(int amount)
    {
        if(amount is <1 or >100000)return false;
        await GetColorPrice();
        return await db.TokenPricing.Where(x=>x.Id==1).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.ColorTokenPrice,amount))==1;
    }
}
