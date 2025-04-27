using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Settings;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string message, List<AttachmentInfo> attachments = null);
    string ProcessTemplate(string template, Dictionary<string, string> parameters);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly Regex _parameterRegex = new(@"{{(\w+)}}");

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message, List<AttachmentInfo> attachments = null)
    {
        if (string.IsNullOrEmpty(_emailSettings.SendGridKey))
        {
            throw new Exception("SendGrid API key is not configured");
        }

        var client = CreateSendGridClient(_emailSettings.SendGridKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_emailSettings.SendGridFromEmail, _emailSettings.DefaultFromName),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };

        msg.AddTo(new EmailAddress(toEmail));

        if (attachments != null && attachments.Any())
        {
            foreach (var attachment in attachments)
            {
                var bytes = Convert.FromBase64String(attachment.Content);
                msg.AddAttachment(attachment.FileName, Convert.ToBase64String(bytes));
            }
        }

        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode 
            ? $"Email to {toEmail} queued successfully!" 
            : $"Failed to send email to {toEmail}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
    }

    public string ProcessTemplate(string template, Dictionary<string, string> parameters)
    {
        return _parameterRegex.Replace(template, match =>
        {
            var paramName = match.Groups[1].Value;
            return parameters.TryGetValue(paramName, out var value) ? value : match.Value;
        });
    }
    
    /// <summary>
    /// Creates a SendGrid client with the given API key.
    /// This is protected and virtual to allow mocking in tests.
    /// </summary>
    /// <param name="apiKey">The SendGrid API key</param>
    /// <returns>A SendGrid client</returns>
    protected virtual ISendGridClient CreateSendGridClient(string apiKey)
    {
        return new SendGridClient(apiKey);
    }
}

public class AttachmentInfo
{
    public string FileName { get; set; }
    public string Content { get; set; }  // Base64 encoded content
}