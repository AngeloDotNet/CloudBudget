using CloudBudget.API.Entities;

namespace CloudBudget.API.Services.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Crea un nuovo refresh token persistente collegato allo userId e al jwtId (jti),
    /// con contestualizzazione clientId, ip, userAgent e country.
    /// Implementa sliding-window: disabilita i token precedenti per lo stesso user+client.
    /// </summary>
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string jwtId, string clientId, string? ipAddress, string? userAgent, string? country, CancellationToken ct = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    Task RevokeRefreshTokenAsync(string token, string? replacedBy = null, CancellationToken ct = default);

    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    Task RevokeJwtAsync(string jti, string? reason = null, CancellationToken ct = default);
}