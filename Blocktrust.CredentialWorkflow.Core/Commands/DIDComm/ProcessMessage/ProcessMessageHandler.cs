﻿// namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;
//
// using Blocktrust.DIDComm.Message.FromPriors;
// using Blocktrust.DIDComm.Message.Messages;
// using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
// using Blocktrust.Mediator.Common.Models.ProblemReport;
// using Blocktrust.Mediator.Common.Protocols;
// using MediatR;
//
// public class ProcessMessageHandler : IRequestHandler<ProcessMessageRequest, ProcessMessageResponse>
// {
//     private readonly IMediator _mediator;
//
//     /// <summary>
//     /// Constructor
//     /// </summary>
//     public ProcessMessageHandler(IMediator mediator)
//     {
//         this._mediator = mediator;
//     }
//
//     /// <inheritdoc />
//     public async Task<ProcessMessageResponse> Handle(ProcessMessageRequest request, CancellationToken cancellationToken)
//     {
//         FromPrior? fromPrior = null;
//         string? mediatorDid;
//         var fallBackMediatorDidForErrorMessages = request.UnpackResult.Metadata?.EncryptedTo?.FirstOrDefault();
//         if (fallBackMediatorDidForErrorMessages is null)
//         {
//             //TODO no clue
//         }
//
//         // var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderOldDid, null), cancellationToken);
//         // if (existingConnection.IsFailed)
//         // {
//         //     return new ProcessMessageResponse(
//         //         ProblemReportMessage.BuildDefaultInternalError(
//         //             errorMessage: "Unable to connect to database",
//         //             threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
//         //             fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
//         // }
//         //
//         // if (existingConnection.Value is null)
//         // {
//         //     // Create new connection
//         //     var mediatorDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: new Uri(request.HostUrl)), cancellationToken);
//         //     if (mediatorDidResult.IsFailed)
//         //     {
//         //         return new ProcessMessageResponse(
//         //             ProblemReportMessage.BuildDefaultInternalError(
//         //                 errorMessage: "Unable to create PeerDID",
//         //                 threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
//         //                 fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
//         //     }
//         //
//         //     var iss = request.UnpackResult.Metadata?.EncryptedTo?.First().Split('#')[0]; // The current Did of the mediator the msg was send to
//         //     var sub = mediatorDidResult.Value.PeerDid.Value; // The new Did of the mediator that will be used for future communication
//         //     fromPrior = FromPrior.Builder(iss, sub).Build();
//         //
//         //     if (request.SenderDid is not null)
//         //     {
//         //         var createConnectionResult = await _mediator.Send(new CreateConnectionRequest(mediatorDidResult.Value.PeerDid.Value, request.SenderDid), cancellationToken);
//         //         if (createConnectionResult.IsFailed)
//         //         {
//         //             return new ProcessMessageResponse(
//         //                 ProblemReportMessage.BuildDefaultInternalError(
//         //                     errorMessage: "Unable grant mediation",
//         //                     threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
//         //                     fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
//         //         }
//         //     }
//         //
//         //     mediatorDid = mediatorDidResult.Value.PeerDid.Value;
//         // }
//         // else
//         // {
//         //     mediatorDid = existingConnection?.Value?.MediatorDid;
//         // }
//
//         Message? result;
//         switch (request.UnpackResult.Message.Type)
//         {
//             case ProtocolConstants.TrustPingRequest:
//                 result = await _mediator.Send(new ProcessTrustPingRequest(request.UnpackResult.Message), cancellationToken);
//                 break;
//             case ProtocolConstants.BasicMessage:
//                 result = await _mediator.Send(new BasicMessageAbRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior), cancellationToken);
//                 break;
//             default:
//                 return new ProcessMessageResponse(ProblemReportMessage.Build(
//                     fromPrior: fromPrior,
//                     threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
//                     problemCode: new ProblemCode(
//                         sorter: EnumProblemCodeSorter.Error,
//                         scope: EnumProblemCodeScope.Message,
//                         stateNameForScope: null,
//                         descriptor: EnumProblemCodeDescriptor.Message,
//                         otherDescriptor: null
//                     ),
//                     comment: $"Not supported message type: '{request.UnpackResult.Message.Type}'",
//                     commentArguments: null,
//                     escalateTo: new Uri("mailto:info@blocktrust.dev")), mediatorDid);
//         }
//
//         if (result is null)
//         {
//             return new ProcessMessageResponse(); // 202 Accecpted
//         }
//
//         return new ProcessMessageResponse(result, mediatorDid);
//     }
// }