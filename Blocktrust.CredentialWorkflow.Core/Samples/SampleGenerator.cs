using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;

namespace Blocktrust.CredentialWorkflow.Core.Samples
{
    public class SampleGenerator
    {
        public static ProcessFlow GenerateSampleProcessFlow()
        {
            var triggerId = Guid.NewGuid();
            var issuanceActionId = Guid.NewGuid();
            var outgoingRequestActionId = Guid.NewGuid();

            return new ProcessFlow
            {
                // Triggers = new Dictionary<Guid, Triggers>
                // {
                //     {
                //         triggerId,
                //         new Triggers
                //         {
                //             Type = ETriggerType.IncomingRequest,
                //             Input = new TriggerInputIncomingRequest
                //             {
                //                 Id = Guid.NewGuid(),
                //                 Method = "POST",
                //                 Uri = "https://api.blocktrust.dev/issue-credential",
                //                 Body = new
                //                 {
                //                     subjectId = "did:example:123456789abcdefghi",
                //                     credentialType = "VerifiableCredential",
                //                     claims = new
                //                     {
                //                         name = "Alice Smith",
                //                         dateOfBirth = "1990-05-15"
                //                     }
                //                 },
                //                 Headers = new Dictionary<string, string>
                //                 {
                //                     { "Content-Type", "application/json" },
                //                     { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
                //                 }
                //             }
                //         }
                //     }
                // },
                // Actions = new Dictionary<Guid, Actions>
                // {
                //     {
                //         issuanceActionId,
                //         new Actions
                //         {
                //             Type = EActionType.CredentialIssuance,
                //             Input = new IssueW3cCredentialInput
                //             {
                //                 Id = Guid.NewGuid(),
                //                 Subject = "did:example:123456789abcdefghi",
                //                 Issuer = "did:example:issuer987654321",
                //                 Claims = new Dictionary<string, string>
                //                 {
                //                     { "name", "Alice Smith" },
                //                     { "dateOfBirth", "1990-05-15" },
                //                     { "credentialType", "IdentityCredential" }
                //                 }
                //             },
                //             RunAfter = new Dictionary<Guid, List<EFlowStatus>>
                //             {
                //                 { triggerId, new List<EFlowStatus> { EFlowStatus.Succeeded } }
                //             }
                //         }
                //     },
                //     {
                //         outgoingRequestActionId,
                //         new Actions
                //         {
                //             Type = EActionType.OutgoingRequest,
                //             Input = new ActionInputOutgoingRequest
                //             {
                //                 Id = Guid.NewGuid(),
                //                 Method = "POST",
                //                 Uri = "https://example.com/receive-credential",
                //                 Body = "{\"credential\": \"eyJhbGciOiJFZERTQS...\"}",
                //                 Headers = new Dictionary<string, string>
                //                 {
                //                     { "Content-Type", "application/json" },
                //                     { "X-API-Key", "your-api-key-here" }
                //                 }
                //             },
                //             RunAfter = new Dictionary<Guid, List<EFlowStatus>>
                //             {
                //                 { issuanceActionId, new List<EFlowStatus> { EFlowStatus.Succeeded } }
                //             }
                //         }
                //     }
                // },
                // Outputs = new Dictionary<Guid, Output>
                // {
                //     {
                //         triggerId, 
                //         new Output
                //         {
                //             Type = OutputType.Object,
                //             Id = Guid.NewGuid(),
                //             Value = new
                //             {
                //                 status = "Succeeded",
                //                 message = "Incoming request processed successfully"
                //             }
                //         }
                //     },
                //     {
                //         issuanceActionId,
                //         new Output
                //         {
                //             Type = OutputType.Object,
                //             Id = Guid.NewGuid(),
                //             Value = new
                //             {
                //                 status = "Succeeded",
                //                 message = "Credential issued successfully",
                //                 credentialId = "vc:example:123456789"
                //             }
                //         }
                //     },
                //     {
                //         outgoingRequestActionId,
                //         new Output
                //         {
                //             Type = OutputType.Object,
                //             Id = Guid.NewGuid(),
                //             Value = new
                //             {
                //                 status = "Succeeded",
                //                 message = "Credential sent successfully",
                //                 httpStatus = 200,
                //                 responseBody = "{\"received\": true, \"message\": \"Credential stored successfully\"}"
                //             }
                //         }
                //     }
                // }
            };
        }
    }
}