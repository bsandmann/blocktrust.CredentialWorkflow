// using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
// using FluentResults;
// using Microsoft.Extensions.Logging;
//
// namespace Blocktrust.CredentialWorkflow.Core.Services;
//
// public class DeliveryService : IDeliveryService
// {
//     private readonly ILogger<DeliveryService> _logger;
//     private readonly IDidResolutionService _didResolutionService;
//
//     public DeliveryService(
//         ILogger<DeliveryService> logger,
//         IDidResolutionService didResolutionService)
//     {
//         _logger = logger;
//         _didResolutionService = didResolutionService;
//     }
//
//     public async Task<Result> DeliverViaEmail(string email, string credential)
//     {
//         try
//         {
//             // TODO: Implement email delivery
//             return Result.Ok();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to deliver credential via email");
//             return Result.Fail("Failed to deliver via email: " + ex.Message);
//         }
//     }
//
//     public async Task<Result> DeliverViaDIDComm(string peerDid, string credential)
//     {
//         try
//         {
//             // TODO: Implement DIDComm delivery
//             return Result.Ok();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to deliver credential via DIDComm");
//             return Result.Fail("Failed to deliver via DIDComm: " + ex.Message);
//         }
//     }
// }