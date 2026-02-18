namespace CloudBudget.API.Settings;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpireMinutes { get; set; } = 60;
}