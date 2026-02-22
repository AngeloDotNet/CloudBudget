namespace CloudBudget.API.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Token stringa (secure random)
    public string Token { get; set; } = null!;

    // L'id del JWT associato (jti claim)
    public string JwtId { get; set; } = null!;

    // Proprietario del refresh token
    public Guid UserId { get; set; }

    // Identificativo del client/device (es. client-generated GUID o device id)
    public string ClientId { get; set; } = null!;

    // Info di contesto
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Se revocato, l'istante di revoca
    public DateTime? RevokedAt { get; set; }

    // Se il refresh token è stato rotato/replaced conserva il token sostitutivo
    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}