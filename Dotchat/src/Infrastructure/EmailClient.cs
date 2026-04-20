using DotchatServer.src.Application.DTOs.EmailModels;
using DotchatServer.src.Application.DTOs.Emails;
using DotchatServer.src.Core.Interfaces;
using DotNetEnv;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Serilog;

namespace DotchatServer.src.Infrastructure;

public sealed class EmailClient : IEmailClient
{
    private readonly EmailOptions _options;
    private readonly string _appPassword;

    public EmailClient(EmailOptions options)
    {
        _options = options;
        _appPassword = Env.GetString("GMAIL_APP_PASSWORD");
    }

    public async Task<bool> TrySendEmailAsync(IEnumerable<MailboxAddress> recipients, Email email)
    {
        using SmtpClient client = new();
        try
        {
            MimeMessage mimeMessage = new()
            {
                From = { new MailboxAddress(name: _options.SenderName, address: _options.SenderEmail) },
                Subject = email.Subject,
                Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = email.HtmlBody
                },
            };
            mimeMessage.To.AddRange(recipients);

            client.Connect(host: _options.Host, port: _options.Port, SecureSocketOptions.StartTls);
            client.Authenticate(_options.SenderEmail, _appPassword);
            _ = client.Send(mimeMessage);

            return true;
        }
        catch (Exception ex)
        {
            // TODO: Handle specific exceptions (e.g., SmtpException). Implement a retry mechanism for transient failures.
            Log.Error(ex, "Failed to send email to {Recipients}", string.Join(", ", recipients.Select(x => x.Address)));
            return false;
        }
        finally
        {
            client.Disconnect(true);
        }
    }
}