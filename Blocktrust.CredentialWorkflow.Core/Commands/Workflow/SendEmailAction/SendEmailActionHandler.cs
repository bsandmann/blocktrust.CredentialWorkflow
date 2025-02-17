using Blocktrust.CredentialWorkflow.Core.Services;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.SendEmailAction;

public class SendEmailActionHandler : IRequestHandler<SendEmailActionRequest, Result>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailActionHandler> _logger;

    public SendEmailActionHandler(IEmailService emailService, ILogger<SendEmailActionHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmailActionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Body);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return Result.Fail(ex.Message);
        }
    }
}