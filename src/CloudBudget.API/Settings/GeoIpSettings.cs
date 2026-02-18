namespace CloudBudget.API.Settings;

public class GeoIpSettings
{
    // provider: "ipapi" | "ipinfo" | "none"
    public string Provider { get; set; } = "none";
    public string? ApiKey { get; set; }
}