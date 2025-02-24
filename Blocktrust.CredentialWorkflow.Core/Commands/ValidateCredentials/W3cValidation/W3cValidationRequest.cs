using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

public class W3cValidationRequest : IRequest<Result<W3cValidationResponse>>
{
    public string Credential { get; }
    public List<ValidationRule> Rules { get; }

    public W3cValidationRequest(string credential, List<ValidationRule> rules)
    {
        Credential = credential;
        Rules = rules;
    }
}