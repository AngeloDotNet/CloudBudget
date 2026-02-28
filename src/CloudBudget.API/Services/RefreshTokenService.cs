using System.Security.Cryptography;
using CloudBudget.API.Data;
using CloudBudget.API.Entities;
using CloudBudget.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Services;

public class RefreshTokenService(CloudBudgetDbContext db) : IRefreshTokenService
{
    private readonly TimeSpan refreshTtl = TimeSpan.FromDays(30);

    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string jwtId, string clientId, string? ipAddress, string? userAgent,
        string? country, CancellationToken ct = default)
    {
        // Sliding window: revoke previous active tokens for same user+clientId
        var previousActive = await db.RefreshTokens
            .Where(r => r.UserId == userId && r.ClientId == clientId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var p in previousActive)
        {
            p.RevokedAt = DateTime.UtcNow;
            p.ReplacedByToken = null; // will be set below if needed
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);

        var now = DateTime.UtcNow;
        var rt = new RefreshToken
        {
            Token = token,
            JwtId = jwtId,
            UserId = userId,
            ClientId = clientId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Country = country,
            CreatedAt = now,
            ExpiresAt = now.Add(refreshTtl),
            RevokedAt = null,
        };

        await db.RefreshTokens.AddAsync(rt, ct);

        // mark replacedBy for previous tokens to this new token
        foreach (var p in previousActive)
        {
            p.ReplacedByToken = rt.Token;
            db.RefreshTokens.Update(p);
        }

        await db.SaveChangesAsync(ct);
        return rt;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        return await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? replacedBy = null, CancellationToken ct = default)
    {
        var rt = await GetByTokenAsync(token, ct);
        if (rt == null)
        {
            return;
        }

        if (rt.RevokedAt != null)
        {
            return;
        }

        rt.RevokedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(replacedBy))
        {
            rt.ReplacedByToken = replacedBy;
        }

        db.RefreshTokens.Update(rt);
        await db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var t in tokens)
        {
            t.RevokedAt = now;
        }

        db.RefreshTokens.UpdateRange(tokens);
        await db.SaveChangesAsync(ct);
    }

    public async Task RevokeJwtAsync(string jti, string? reason = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(jti))
        {
            return;
        }

        var exists = await db.RevokedJwts.FindAsync(new object[] { jti }, ct);
        if (exists != null)
        {
            return;
        }

        var r = new RevokedJwt
        {
            Jti = jti,
            RevokedAt = DateTime.UtcNow,
            Reason = reason
        };

        await db.RevokedJwts.AddAsync(r, ct);
        await db.SaveChangesAsync(ct);
    }
}
