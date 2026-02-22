namespace CloudBudget.API.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; } = false;

    // ClientId: identificativo del device / client (es. client genera GUID persistente)
    public string? ClientId { get; set; }
}