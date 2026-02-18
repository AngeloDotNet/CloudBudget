using BudgetApp.Models;

namespace CloudBudget.API.Entities;

public class RefreshToken : BaseEntity<Guid>
{
    //public Guid Id { get; set; } = Guid.NewGuid();

    // Token stringa (secure random)
    public string Token { get; set; } = null!;

    // L'id del JWT associato (jti claim)
    public string JwtId { get; set; } = null!;

    // Proprietario del refresh token
    public Guid UserId { get; set; }

    //public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Se revocato, l'istante di revoca
    public DateTime? RevokedAt { get; set; }

    // Se il refresh token è stato rotato/replaced conserva il token sostitutivo
    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}