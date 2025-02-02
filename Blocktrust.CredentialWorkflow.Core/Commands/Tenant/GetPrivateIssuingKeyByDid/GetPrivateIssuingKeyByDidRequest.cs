using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid
{
    public class GetPrivateIssuingKeyByDidRequest : IRequest<Result<string>>
    {
        public string Did { get; }

        public GetPrivateIssuingKeyByDidRequest(string did)
        {
            Did = did;
        }
    }
}