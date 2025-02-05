using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;

public class VerifyW3CCredentialRequest : IRequest<Result<CredentialVerificationResult>>
{
    public VerifyW3CCredentialRequest(string credential, bool verifySignature, bool verifyExpiry, bool verifyRevocationStatus, bool verifySchema, bool verifyTrustRegistry)
    {
        Credential = credential;
        VerifySignature = verifySignature;
        VerifyExpiry = verifyExpiry;
        VerifyRevocationStatus = verifyRevocationStatus;
        VerifySchema = verifySchema;
        VerifyTrustRegistry = verifyTrustRegistry;
    }

    public string Credential { get; }
    public bool VerifySignature { get; set; }
    public bool VerifyExpiry { get; set; }
    public bool VerifyRevocationStatus { get; set; }
    public bool VerifySchema { get; set; }
    public bool VerifyTrustRegistry { get; set; }
}