using CloudBudget.API.Services.EmailSender.Interfaces;
using CloudBudget.API.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CloudBudget.API.Services.EmailSender;

public class MailKitEmailSender(IOptions<SmtpSettings> settings) : IEmailSender
{
    private readonly SmtpSettings settings = settings.Value;

    public async Task SendAsync(string[] recipients, string subject, string body, Stream? attachmentStream = null, string? attachmentName = null, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));

        foreach (var recipient in recipients)
        {
            message.To.Add(MailboxAddress.Parse(recipient));
        }

        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };

        if (attachmentStream != null && attachmentName != null)
        {
            attachmentStream.Position = 0;
            builder.Attachments.Add(attachmentName, attachmentStream, cancellationToken);
        }

        builder.HtmlBody = body;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        //await client.ConnectAsync(settings.Server, settings.Port, settings.UseSsl, cancellationToken);
        await client.ConnectAsync(settings.Server, settings.Port, settings.Security, cancellationToken);

        if (!string.IsNullOrEmpty(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}