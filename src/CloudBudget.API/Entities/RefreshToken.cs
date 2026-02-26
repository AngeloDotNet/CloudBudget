namespace CloudBudget.API.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public Guid UserId { get; set; }
    public string ClientId { get; set; } = null!;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Nuova proprietà: country code (es. "IT", "US")
    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}