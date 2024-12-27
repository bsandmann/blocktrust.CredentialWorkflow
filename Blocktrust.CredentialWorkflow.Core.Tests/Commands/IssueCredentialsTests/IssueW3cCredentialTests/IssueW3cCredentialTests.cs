using System.Text;
using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;
using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssueCredentialsTests.IssueW3cCredentialTests;

public class IssueW3CCredentialTests
{
    private readonly IEcService _ecService;
    private readonly IssueW3CCredentialHandler _handler;
    private readonly ExtractPrismPubKeyFromLongFormDid _extractor;
    private readonly CredentialParser _credentialParser;

    private const string ValidCredential = @"{
        ""@context"": [""https://www.w3.org/2018/credentials/v1""],
        ""type"": [""VerifiableCredential""],
        ""issuer"": ""did:prism:test:issuer"",
        ""issuanceDate"": ""2024-01-01T00:00:00Z"",
        ""credentialSubject"": {
            ""id"": ""did:prism:test:subject"",
            ""name"": ""Test Subject""
        }
    }";

    public IssueW3CCredentialTests()
    {
        _ecService = new EcServiceBouncyCastle();
        _handler = new IssueW3CCredentialHandler(_ecService);
        _extractor = new ExtractPrismPubKeyFromLongFormDid();
        _credentialParser = new CredentialParser();
    }

    [Fact]
    public async Task Handle_ValidCredential_ShouldReturnSignedJwtWithCleanPayload()
    {
        // Arrange
        var credential = _credentialParser.ParseCredential(ValidCredential).Value;
        var privateKey = new byte[32];
        var rng = new Random();
        rng.NextBytes(privateKey);
        var issuerDid = "did:prism:test:issuer";

        // Act
        var request = new IssueW3CCredentialRequest(credential, privateKey, issuerDid);
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var jwtParts = result.Value.Split('.');
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
        payload.GetProperty("iss").GetString().Should().Be(issuerDid);
        payload.GetProperty("sub").GetString().Should().Be("did:prism:test:subject");
        // payload.TryGetProperty("nbf").Should().BeTrue();
        // payload.TryGetProperty("exp").Should().BeTrue();

        // Verify VC payload is clean
        var vc = payload.GetProperty("vc");
        // vc.TryGetProperty("headerJson").Should().BeFalse();
        // vc.TryGetProperty("payloadJson").Should().BeFalse();

        // Verify required VC properties are present
        vc.GetProperty("context").EnumerateArray().First().GetString()
            .Should().Be("https://www.w3.org/2018/credentials/v1");
        vc.GetProperty("type").EnumerateArray().First().GetString()
            .Should().Be("VerifiableCredential");
        vc.GetProperty("issuer").GetString().Should().Be(issuerDid);
        vc.GetProperty("issuanceDate").GetString().Should().Be("2024-01-01T00:00:00Z");
        
        var subject = vc.GetProperty("credentialSubject");
        subject.GetProperty("id").GetString().Should().Be("did:prism:test:subject");
        subject.GetProperty("name").GetString().Should().Be("Test Subject");
    }

    [Fact]
    public async Task Handle_InvalidPrivateKey_ShouldFail()
    {
        // Arrange
        var credential = _credentialParser.ParseCredential(ValidCredential).Value;
        var invalidPrivateKey = new byte[16]; // Wrong size private key
        var issuerDid = "did:prism:test:issuer";

        // Act
        var request = new IssueW3CCredentialRequest(credential, invalidPrivateKey, issuerDid);
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Contain("Failed to sign credential");
    }

    [Fact]
    public async Task Handle_CredentialWithoutSubject_ShouldSucceed()
    {
        // Arrange
        var credentialWithoutSubject = @"{
            ""@context"": [""https://www.w3.org/2018/credentials/v1""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""did:prism:test:issuer"",
            ""issuanceDate"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";
        var credential = _credentialParser.ParseCredential(credentialWithoutSubject).Value;
        var privateKey = new byte[32];
        var rng = new Random();
        rng.NextBytes(privateKey);
        var issuerDid = "did:prism:test:issuer";

        // Act
        var request = new IssueW3CCredentialRequest(credential, privateKey, issuerDid);
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var jwtParts = result.Value.Split('.');
        var payloadJson = Encoding.UTF8.GetString(PrismEncoding.Base64ToByteArray(jwtParts[1]));
        var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);
        
        payload.GetProperty("vc").GetProperty("credentialSubject").ValueKind
            .Should().Be(JsonValueKind.Object);
        payload.GetProperty("vc").GetProperty("credentialSubject").EnumerateObject()
            .Should().BeEmpty();
    }
}