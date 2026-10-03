using CatalogStudio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<int>, int>(options)
{
    public DbSet<TokenPricing> TokenPricing => Set<TokenPricing>();
    public DbSet<TokenWallet> TokenWallets => Set<TokenWallet>(); public DbSet<TokenOperation> TokenOperations => Set<TokenOperation>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>(); public DbSet<Catalog> Catalogs => Set<Catalog>();
    protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b); b.Entity<TokenWallet>().HasKey(x=>x.UserId); b.Entity<TokenWallet>().HasOne(x=>x.User).WithOne().HasForeignKey<TokenWallet>(x=>x.UserId); b.Entity<UserPreference>().HasIndex(x => x.UserId).IsUnique(); b.Entity<Catalog>().Ignore(x => x.Age); b.Entity<Catalog>().Property(x=>x.OutputWidth).HasDefaultValue(1400); b.Entity<Catalog>().Property(x=>x.OutputHeight).HasDefaultValue(1100); b.Entity<Catalog>().Property(x=>x.WhiteModelPathsJson).HasDefaultValue("[]"); b.Entity<Catalog>().Property(x=>x.WhiteOutputWidth).HasDefaultValue(1400); b.Entity<Catalog>().Property(x=>x.WhiteOutputHeight).HasDefaultValue(1100); b.Entity<Catalog>().Property(x=>x.ColorTokenPrice).HasDefaultValue(75); }
}
