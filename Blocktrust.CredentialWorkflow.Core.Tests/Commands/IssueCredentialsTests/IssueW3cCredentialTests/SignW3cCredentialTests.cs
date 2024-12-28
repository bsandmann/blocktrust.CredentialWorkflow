using System.Text;
using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssueCredentialsTests.IssueW3cCredentialTests;

public class SignW3CCredentialTests
{
    private readonly IEcService _ecService;
    private readonly SignW3cCredentialHandler _signHandler;
    private readonly CreateW3cCredentialHandler _createHandler;

    private const string IssuerDid = "did:prism:53900b7c1f1c7044ed4989ab46570d2927236e9414febe82ea36aaa917a642dd:CoQBCoEBEkIKDm15LWlzc3Vpbmcta2V5EAJKLgoJc2VjcDI1NmsxEiECfd6iCbzvLCSONelmvs3oS2IYyug8Z3hp9MZeS2W2BrkSOwoHbWFzdGVyMBABSi4KCXNlY3AyNTZrMRIhAkyZEcCAaL-VdPnQOOtulV6DSI6xb1USWExoQlInl2ma";
    private const string SubjectDid = "did:prism:d2250d9ee063c3f5baed212ed45c9f53c08bc189c047a95d86da50ffdef43cee:CnsKeRI6CgZhdXRoLTEQBEouCglzZWNwMjU2azESIQOfNAetvAuZBFzW4VtjBs-L2XCbyvHaye2VkSVp9F5oQBI7CgdtYXN0ZXIwEAFKLgoJc2VjcDI1NmsxEiECvaWNqvszvwnURuWyxOW3Go2LqP5z7cKNVjWM5LY5n38";

    public SignW3CCredentialTests()
    {
        _ecService = new EcServiceBouncyCastle();
        _signHandler = new SignW3cCredentialHandler(_ecService);
        _createHandler = new CreateW3cCredentialHandler();
    }

    [Fact]
    public async Task Handle_ValidCredential_ShouldMatchExpectedStructure()
    {
        // Arrange
        var privateKey = new byte[32];
        var rng = new Random();
        rng.NextBytes(privateKey);

        var validFrom = DateTimeOffset.FromUnixTimeSeconds(1726843196);
        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(2026843196);

        // Create a credential using CreateW3cCredentialHandler
        var additionalSubjectData = new Dictionary<string, object>
        {
            {
                "achievement", new Dictionary<string, object>
                {
                    { "achievementType", "Diploma" },
                    { "name", "Digital Identity Course" },
                    { "description", "A course on Digital identity" }
                }
            }
        };

        var createRequest = new CreateW3cCredentialRequest(
            IssuerDid,
            SubjectDid,
            additionalSubjectData,
            validFrom,
            expirationDate);

        var createResult = await _createHandler.Handle(createRequest, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        // Act - Sign the created credential
        var signRequest = new SignW3cCredentialRequest(createResult.Value, privateKey, IssuerDid);
        var signResult = await _signHandler.Handle(signRequest, CancellationToken.None);

        // Assert
        signResult.IsSuccess.Should().BeTrue();
        signResult.Value.Should().NotBeNull();

        var jwtParts = signResult.Value.Split('.');
        jwtParts.Should().HaveCount(3);

        // Verify Header
        var headerJson = Encoding.UTF8.GetString(PrismEncoding.Base64ToByteArray(jwtParts[0]));
        var header = JsonSerializer.Deserialize<JsonElement>(headerJson);
        header.GetProperty("alg").GetString().Should().Be("ES256K");
        header.GetProperty("typ").GetString().Should().Be("JWT");

        // Verify Payload
        var payloadJson = Encoding.UTF8.GetString(PrismEncoding.Base64ToByteArray(jwtParts[1]));
        var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);

        // Core JWT claims
        payload.GetProperty("iss").GetString().Should().Be(IssuerDid);
        payload.GetProperty("sub").GetString().Should().Be(SubjectDid);
        // payload.GetProperty("nbf").GetInt64().Should().Be(1726843196);
        // payload.GetProperty("exp").GetInt64().Should().Be(2026843196);

        // Verify VC payload structure
        var vc = payload.GetProperty("vc");
        vc.GetProperty("type").EnumerateArray().First().GetString()
            .Should().Be("VerifiableCredential");
        vc.GetProperty("@context").EnumerateArray().First().GetString()
            .Should().Be("https://www.w3.org/2018/credentials/v1");
        
        // Verify achievement subject
        var subject = vc.GetProperty("credentialSubject");
        subject.GetProperty("id").GetString().Should().Be(SubjectDid);
        var achievement = subject.GetProperty("achievement");
        achievement.GetProperty("achievementType").GetString().Should().Be("Diploma");
        achievement.GetProperty("name").GetString().Should().Be("Digital Identity Course");
    }

    [Fact]
    public async Task Handle_InvalidPrivateKey_ShouldFail()
    {
        // Arrange
        var invalidPrivateKey = new byte[16]; // Wrong size private key
        
        // Create a credential first
        var createRequest = new CreateW3cCredentialRequest(IssuerDid, SubjectDid);
        var createResult = await _createHandler.Handle(createRequest, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        // Act
        var signRequest = new SignW3cCredentialRequest(createResult.Value, invalidPrivateKey, IssuerDid);
        var result = await _signHandler.Handle(signRequest, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Contain("Failed to sign credential");
    }
}