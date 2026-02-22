namespace CloudBudget.API.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }

    // Refresh token e scadenza
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
