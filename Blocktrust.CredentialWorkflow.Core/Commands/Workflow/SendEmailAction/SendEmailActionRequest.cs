using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.SendEmailAction;

public class SendEmailActionRequest : IRequest<Result>
{
    public string ToEmail { get; }
    public string Subject { get; }
    public string Body { get; }

    public SendEmailActionRequest(string toEmail, string subject, string body)
    {
        ToEmail = toEmail;
        Subject = subject;
        Body = body;
    }
}