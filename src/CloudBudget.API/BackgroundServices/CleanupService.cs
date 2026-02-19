using CloudBudget.API.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.BackgroundServices;

/// <summary>
/// Background job che pulisce DB:
/// - elimina refresh token scaduti da più di ExpiredRefreshRetentionDays
/// - elimina revoked-jwts piu' vecchi di RevokedJwtRetentionDays
/// </summary>
public class CleanupService(IServiceProvider provider, ILogger<CleanupService> logger) : BackgroundService
{

    // parametri retention (potresti spostarli in configurazione)
    private const int ExpiredRefreshRetentionDays = 30; // elimina refresh token scaduti da > 30 giorni
    private const int RevokedJwtRetentionDays = 90;    // elimina revoked-jwts più vecchi di 90 giorni
    private static readonly TimeSpan runInterval = TimeSpan.FromHours(24); // esegue una volta al giorno

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CleanupService avviato.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CloudBudgetDbContext>();

                var now = DateTime.UtcNow;
                var refreshThreshold = now.AddDays(-ExpiredRefreshRetentionDays);
                var revokedThreshold = now.AddDays(-RevokedJwtRetentionDays);

                var expiredRefresh = await db.RefreshTokens
                    .Where(r => r.ExpiresAt < now && (r.RevokedAt ?? DateTime.MinValue) < refreshThreshold)
                    .ToListAsync(stoppingToken);

                if (expiredRefresh.Count > 0)
                {
                    db.RefreshTokens.RemoveRange(expiredRefresh);
                    logger.LogInformation("CleanupService: rimosse {Count} refresh token scaduti.", expiredRefresh.Count);
                }

                var oldRevoked = await db.RevokedJwts
                    .Where(r => r.RevokedAt < revokedThreshold)
                    .ToListAsync(stoppingToken);

                if (oldRevoked.Count > 0)
                {
                    db.RevokedJwts.RemoveRange(oldRevoked);
                    logger.LogInformation("CleanupService: rimosse {Count} revoked-jwts vecchi.", oldRevoked.Count);
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore in CleanupService");
            }

            await Task.Delay(runInterval, stoppingToken);
        }

        logger.LogInformation("CleanupService stopping.");
    }
}