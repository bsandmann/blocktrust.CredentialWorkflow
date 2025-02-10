using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.VerifiableCredential.Common;
using Blocktrust.VerifiableCredential.VC;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;

public class CreateW3cCredentialHandler : IRequestHandler<CreateW3cCredentialRequest, Result<Credential>>
{
    private static readonly CredentialOrPresentationContext DefaultContext =
        new()
        {
            Contexts = new List<object> { "https://www.w3.org/2018/credentials/v1" },
            SerializationOption = new SerializationOption()
            {
                UseArrayEvenForSingleElement = true
            }

        };

    private static readonly CredentialOrPresentationType DefaultType = new()
    {
        Type = new HashSet<string> { "VerifiableCredential" },
        SerializationOption = new SerializationOption()
        {
            UseArrayEvenForSingleElement = true
        }
    };

    public async Task<Result<Credential>> Handle(CreateW3cCredentialRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var credential = new Credential
            {
                CredentialContext = DefaultContext,
                Type = DefaultType,
                CredentialSubjects = new List<CredentialSubject>
                {
                    new()
                    {
                        Id = new Uri(request.SubjectDid),
                        AdditionalData = request.AdditionalSubjectData ?? new Dictionary<string, object>()
                    }
                },
                CredentialIssuer = new CredentialIssuer(
                    issuerId: new Uri(request.IssuerDid)),
                ValidFrom = request.ValidFrom.DateTime,
                ExpirationDate = request.ExpirationDate?.DateTime
            };

            return Result.Ok(credential);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create credential: {ex.Message}");
        }
    }
}