namespace CloudBudget.API.Services.EmailSender.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string[] recipients, string subject, string body, Stream? attachmentStream = null, string? attachmentName = null, CancellationToken cancellationToken = default);
}