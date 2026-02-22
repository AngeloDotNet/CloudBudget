namespace CloudBudget.API.DTOs;

public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = null!;

    // ClientId obbligatorio per la policy sliding-window / binding al device
    public string? ClientId { get; set; }
}