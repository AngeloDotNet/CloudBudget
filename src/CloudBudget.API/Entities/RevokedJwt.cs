namespace CloudBudget.API.Entities;

// Persistiamo qui i JWT (jti) revocati in modo che il middleware possa negarli
public class RevokedJwt : BaseEntity<Guid>
{
    public string Jti { get; set; } = null!;
    public DateTime RevokedAt { get; set; }
    public string? Reason { get; set; }
}