namespace Blocktrust.CredentialWorkflow.Web.Services;

using Core.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly EmailSettings _emailSettings;

    public SendGridEmailSender(IOptions<EmailSettings> emailSettingsOptions, ILogger<SendGridEmailSender> logger)
    {
        _emailSettings = emailSettingsOptions.Value;
        _logger = logger;
    }


    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(_emailSettings.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }

        await Execute(_emailSettings.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_emailSettings.SendGridFromEmail, _emailSettings.DefaultFromName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure Email to {toEmail}");
    }
}