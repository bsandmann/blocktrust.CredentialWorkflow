using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services.Interfaces;

public interface IDeliveryService
{
    Task<Result> DeliverViaEmail(string email, string credential);
    Task<Result> DeliverViaDIDComm(string peerDid, string credential);
}