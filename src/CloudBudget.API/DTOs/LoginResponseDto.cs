namespace CloudBudget.API.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
}