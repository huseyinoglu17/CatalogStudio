using CatalogStudio.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace CatalogStudio.Services;
public sealed class DatabaseHealthCheck(IServiceScopeFactory scopes):IHealthCheck {
 public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken ct=default) {
 try {using var scope=scopes.CreateScope();return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.CanConnectAsync(ct)?HealthCheckResult.Healthy():HealthCheckResult.Unhealthy();}
 catch {return HealthCheckResult.Unhealthy();}
 }
}