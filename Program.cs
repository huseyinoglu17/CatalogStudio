using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using CatalogStudio.Data;
using CatalogStudio.Models;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o=>{o.SingleLine=true;o.TimestampFormat="yyyy-MM-dd HH:mm:ss ";});
var port=builder.Configuration["PORT"];
if(!string.IsNullOrWhiteSpace(port)){
 if(!int.TryParse(port,out var parsedPort)||parsedPort<1||parsedPort>65535)throw new InvalidOperationException("PORT must be between 1 and 65535.");
 builder.WebHost.UseUrls($"http://0.0.0.0:{parsedPort}");
}
builder.Services.AddSingleton<IStoragePathService,StoragePathService>();
builder.Services.AddDataProtection().SetApplicationName("CatalogGenerator")
 .AddKeyManagementOptions(o=>{});
builder.Services.AddOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>()
 .Configure<IStoragePathService>((o,paths)=>o.XmlRepository=new Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository(new DirectoryInfo(paths.KeysPath),Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance));
builder.Services.Configure<ForwardedHeadersOptions>(o=>{
 o.ForwardedHeaders=ForwardedHeaders.XForwardedFor|ForwardedHeaders.XForwardedProto;
 o.ForwardLimit=1;
 if(builder.Configuration.GetValue<bool>("ReverseProxy:TrustForwardedHeaders")){
  o.KnownIPNetworks.Clear();o.KnownProxies.Clear();
 }
});
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("sqlite");
builder.Services.AddControllersWithViews(o => o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<AppDbContext>((services,o) => o.UseSqlite(services.GetRequiredService<IStoragePathService>().ConnectionString));
builder.Services.AddIdentity<AppUser, IdentityRole<int>>(o => { o.User.RequireUniqueEmail = true; o.Password.RequiredLength = 8; o.Password.RequireNonAlphanumeric = false; o.Lockout.MaxFailedAccessAttempts = 5; }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(o => { o.Cookie.HttpOnly=true; o.Cookie.SameSite=SameSiteMode.Lax; o.Cookie.SecurePolicy=builder.Environment.IsDevelopment()?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always; o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied"; o.ExpireTimeSpan = TimeSpan.FromDays(30); o.Events.OnValidatePrincipal = async c => { await SecurityStampValidator.ValidatePrincipalAsync(c); if (c.Principal?.Identity?.IsAuthenticated == true) { var users = c.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>(); var user = await users.GetUserAsync(c.Principal); if (user == null || !user.IsActive) { c.RejectPrincipal(); await c.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); } } }; });
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 125 * 1024 * 1024);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 125 * 1024 * 1024);
builder.Services.AddScoped<CatalogRetentionService>(); builder.Services.AddScoped<FileService>(); builder.Services.AddScoped<TokenService>(); builder.Services.AddScoped<UserPreferenceService>(); builder.Services.AddSingleton<AgeCalculatorService>(); builder.Services.AddSingleton<PromptBuilderService>(); builder.Services.AddSingleton<CatalogSceneService>(); builder.Services.AddScoped<CatalogComposerService>();
builder.Services.AddHttpClient<OpenAiImageService>(c => { c.BaseAddress = new Uri("https://api.openai.com/v1/"); c.Timeout = TimeSpan.FromMinutes(5); });
var app = builder.Build();
app.UseForwardedHeaders();
app.UseExceptionHandler("/Home/Error"); if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseWhen(ctx=>!ctx.Request.Path.Equals("/health"),branch=>branch.UseHttpsRedirection()); }
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"); app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
using (var scope=app.Services.CreateScope()) {
 try {
  var paths=scope.ServiceProvider.GetRequiredService<IStoragePathService>();
  app.Logger.LogInformation("Starting CatalogStudio. Storage: {StoragePath}",paths.BasePath);
  app.Logger.LogInformation("Applying database migrations.");
  await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
  app.Logger.LogInformation("Database migrations completed.");
  await DbInitializer.SeedRolesAndAdminAsync(scope.ServiceProvider,app.Environment,app.Configuration);
  await scope.ServiceProvider.GetRequiredService<TokenService>().RecoverPending();
  await scope.ServiceProvider.GetRequiredService<CatalogRetentionService>().Cleanup();
 } catch(Exception ex) {
  app.Logger.LogCritical("Startup storage or migration failed ({Type}). Verify volume permissions and database configuration.",ex.GetType().Name);
  throw;
 }
}
app.Run(); public partial class Program { }
