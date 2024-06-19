namespace Blocktrust.CredentialWorkflow.Web.Services;

using Common;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly AppSettings _appSettings;

    public SendGridEmailSender(IOptions<AppSettings> appSettingOptions, ILogger<SendGridEmailSender> logger)
    {
        _appSettings = appSettingOptions.Value;
        _logger = logger;
    }


    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(_appSettings.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }

        await Execute(_appSettings.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_appSettings.SendGridFromEmail, "Blocktrust Credential Workflow Platform"),
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