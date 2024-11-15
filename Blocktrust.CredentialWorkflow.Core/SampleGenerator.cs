// using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
// using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
// using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;
// using Action = System.Action;
//
// namespace Blocktrust.CredentialWorkflow.Core;
//
// public static class SampleGenerator
// {
//     public static ProcessFlow GenerateCredentialIssuanceFlow()
//     {
//         var flow = new ProcessFlow();
//
//         // Add HTTP trigger for credential issuance
//         var trigger = new Trigger
//         {
//             Type = ETriggerType.IncomingRequest,
//             Input = new TriggerInputCredentialIssuance
//             {
//                 Id = Guid.NewGuid(),
//                 SubjectDid = string.Empty,
//                 DeliveryType = EDeliveryType.Email,
//                 Destination = string.Empty
//             }
//         };
//         flow.AddTrigger(trigger);
//
//         // Add credential issuance action
//         var issuanceAction = new Action
//         {
//             Type = EActionType.CredentialIssuance,
//             Input = new ActionInputCredentialIssuance
//             {
//                 Id = Guid.NewGuid(),
//                 SubjectDid = "$.trigger.subjectDid",
//                 IssuerDid = "$.settings.defaultIssuerDid",
//                 ClaimsJson = "{}"
//             }
//         };
//         flow.AddAction(issuanceAction);
//
//         // Add delivery action
//         var deliveryAction = new Action
//         {
//             Type = EActionType.Delivery,
//             Input = new ActionInputDelivery
//             {
//                 Id = Guid.NewGuid(),
//                 DeliveryType = EDeliveryType.Email,
//                 Destination = "$.trigger.destination"
//             }
//         };
//         flow.AddAction(deliveryAction);
//
//         return flow;
//     }
// }