using MailKit.Security;

namespace CloudBudget.API.Settings;

public class SmtpSettings
{
    public string Server { get; set; } = null!;
    public int Port { get; set; } = 587;
    //public bool UseSsl { get; set; } = true;
    public SecureSocketOptions Security { get; init; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = "Cloud Budget";
}