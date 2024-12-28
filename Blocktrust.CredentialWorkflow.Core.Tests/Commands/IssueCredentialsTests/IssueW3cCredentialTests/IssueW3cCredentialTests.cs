// using System.Text;
// using System.Text.Json;
// using Blocktrust.CredentialBadges.OpenBadges;
// using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential;
// using Blocktrust.CredentialWorkflow.Core.Crypto;
// using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
// using Blocktrust.CredentialWorkflow.Core.Prism;
// using Blocktrust.VerifiableCredential.VC;
// using FluentAssertions;
//
// namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssueCredentialsTests.IssueW3cCredentialTests;
//
// public class IssueW3CCredentialTests
// {
//     private readonly IEcService _ecService;
//     private readonly SignW3cCredentialHandler _handler;
//
//     private const string IssuerDid = "did:prism:53900b7c1f1c7044ed4989ab46570d2927236e9414febe82ea36aaa917a642dd:CoQBCoEBEkIKDm15LWlzc3Vpbmcta2V5EAJKLgoJc2VjcDI1NmsxEiECfd6iCbzvLCSONelmvs3oS2IYyug8Z3hp9MZeS2W2BrkSOwoHbWFzdGVyMBABSi4KCXNlY3AyNTZrMRIhAkyZEcCAaL-VdPnQOOtulV6DSI6xb1USWExoQlInl2ma";
//     private const string SubjectDid = "did:prism:d2250d9ee063c3f5baed212ed45c9f53c08bc189c047a95d86da50ffdef43cee:CnsKeRI6CgZhdXRoLTEQBEouCglzZWNwMjU2azESIQOfNAetvAuZBFzW4VtjBs-L2XCbyvHaye2VkSVp9F5oQBI7CgdtYXN0ZXIwEAFKLgoJc2VjcDI1NmsxEiECvaWNqvszvwnURuWyxOW3Go2LqP5z7cKNVjWM5LY5n38";
//
//     public IssueW3CCredentialTests()
//     {
//         _ecService = new EcServiceBouncyCastle();
//         _handler = new SignW3cCredentialHandler(_ecService);
//     }
//
//     [Fact]
//     public async Task Handle_ValidCredential_ShouldMatchExpectedStructure()
//     {
//         // Arrange
//         var privateKey = new byte[32];
//         var rng = new Random();
//         rng.NextBytes(privateKey);
//
//         var credential = new Credential
//         {
//             CredentialContext = new List<CredentialOrPresentationContext> 
//             { 
//                 new CredentialOrPresentationContext("https://www.w3.org/2018/credentials/v1") 
//             },
//             Type = new[] { "VerifiableCredential" },
//             CredentialSubjects = new List<CredentialSubject>
//             {
//                 new CredentialSubject
//                 {
//                     Id = new Uri(SubjectDid),
//                     AdditionalData = new Dictionary<string, object>
//                     {
//                         {
//                             "achievement", new Dictionary<string, object>
//                             {
//                                 { "achievementType", "Diploma" },
//                                 { "name", "Digital Identity Course" },
//                                 { "description", "A course on Digital identity" },
//                                 {
//                                     "criteria", new Dictionary<string, object>
//                                     {
//                                         {
//                                             "narrative",
//                                             "Through series of lessons.Aliquam erat volutpat. Donec imperdiet eros sapien, eget pharetra sapien vulputate ut. Duis id volutpat dolor. Proin tincidunt maximus blandit. Nam id malesuada erat, sit amet sodales lectus. Fusce ut consequat purus. Ut interdum sapien et tortor laoreet pulvinar."
//                                         },
//                                         { "type", "Criteria" }
//                                     }
//                                 },
//                                 { "id", "urn:uuid:a35a8e4f-7cd4-40da-af32-40289dc16d91" },
//                                 { "type", new[] { "Achievement" } }
//                             }
//                         },
//                         { "type", new[] { "AchievementSubject" } }
//                     }
//                 }
//             },
//             CredentialStatus = new CredentialStatus
//             {
//                 Id = new Uri("http://10.10.50.105:8000/cloud-agent/credential-status/b9b6bb1e-6864-4074-b8ac-12b3a0b30f0c#5"),
//                 Type = "StatusList2021Entry",
//                 StatusPurpose = "Revocation",
//                 StatusListIndex = 5,
//                 StatusListCredential = new string("http://10.10.50.105:8000/cloud-agent/credential-status/b9b6bb1e-6864-4074-b8ac-12b3a0b30f0c")
//             },
//             ValidFrom = DateTimeOffset.FromUnixTimeSeconds(1726843196),
//             ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(2026843196)
//         };
//
//         // Act
//         var request = new SignW3cCredentialRequest(credential, privateKey, IssuerDid);
//         var result = await _handler.Handle(request, CancellationToken.None);
//
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().NotBeNull();
//
//         var jwtParts = result.Value.Split('.');
//         jwtParts.Should().HaveCount(3);
//
//         // Verify Header
//         var headerJson = Encoding.UTF8.GetString(PrismEncoding.Base64ToByteArray(jwtParts[0]));
//         var header = JsonSerializer.Deserialize<JsonElement>(headerJson);
//         header.GetProperty("alg").GetString().Should().Be("ES256K");
//         header.GetProperty("typ").GetString().Should().Be("JWT");
//
//         // Verify Payload
//         var payloadJson = Encoding.UTF8.GetString(PrismEncoding.Base64ToByteArray(jwtParts[1]));
//         var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);
//
//         // Core JWT claims
//         payload.GetProperty("iss").GetString().Should().Be(IssuerDid);
//         payload.GetProperty("sub").GetString().Should().Be(SubjectDid);
//         payload.GetProperty("nbf").GetInt64().Should().Be(1726843196);
//         payload.GetProperty("exp").GetInt64().Should().Be(2026843196);
//
//         // Verify VC payload structure
//         var vc = payload.GetProperty("vc");
//         vc.GetProperty("type").EnumerateArray().First().GetString()
//             .Should().Be("VerifiableCredential");
//         vc.GetProperty("@context").EnumerateArray().First().GetString()
//             .Should().Be("https://www.w3.org/2018/credentials/v1");
//         
//         // Verify achievement subject
//         var subject = vc.GetProperty("credentialSubject");
//         subject.GetProperty("id").GetString().Should().Be(SubjectDid);
//         var achievement = subject.GetProperty("achievement");
//         achievement.GetProperty("achievementType").GetString().Should().Be("Diploma");
//         achievement.GetProperty("name").GetString().Should().Be("Digital Identity Course");
//         
//         // Verify credential status
//         var status = vc.GetProperty("credentialStatus");
//         status.GetProperty("statusPurpose").GetString().Should().Be("Revocation");
//         status.GetProperty("statusListIndex").GetInt32().Should().Be(5);
//     }
// }