using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Resolver;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Client.Commands.ForwardMessage;
using Blocktrust.Mediator.Client.Commands.TrustPing;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

public class DIDCommActionProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ISecretResolver> _secretResolverMock;
    private readonly Mock<IDidDocResolver> _didDocResolverMock;
    private readonly DIDCommActionProcessor _processor;
    private readonly ActionOutcome _actionOutcome;
    private readonly Guid _actionId;
    private readonly ActionProcessingContext _processingContext;
    
    // Static field to hold our mock response for DidDocPeerDid.FromJson
    public static Result<DidDocPeerDid>? DidDocPeerDid_FromJson_Result;
    
    // Sample values for testing
    private const string SenderPeerDid = "did:peer:sender123";
    private const string RecipientPeerDid = "did:peer:recipient456";
    private const string ValidCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwic3ViIjoiZGlkOnByaXNtOnN1YmplY3Q0NTYiLCJuYmYiOjE3MDg5OTIwMDAsImV4cCI6MTg2Njc1ODQwMCwidmMiOnsiQGNvbnRleHQiOlsiaHR0cHM6Ly93d3cudzMub3JnLzIwMTgvY3JlZGVudGlhbHMvdjEiXX19.sig";

    public DIDCommActionProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _secretResolverMock = new Mock<ISecretResolver>();
        _didDocResolverMock = new Mock<IDidDocResolver>();
        
        _processor = new DIDCommActionProcessor(_mediatorMock.Object, _secretResolverMock.Object, _didDocResolverMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create execution context with sender and recipient DIDs
        var inputContext = new Dictionary<string, string>
        {
            { "senderpeerdid", SenderPeerDid },
            { "recipientpeerdid", RecipientPeerDid },
            { "credential", ValidCredential }
        };
        var executionContext = new ExecutionContext(Guid.NewGuid(), new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(inputContext));
        
        _processingContext = new ActionProcessingContext(
            executionContext,
            new List<ActionOutcome>(),
            null, 
            CancellationToken.None
        );
        
        // Set up mock for PeerDidResolver
        var mockPeerDidDoc = "{\"id\":\"did:peer:recipient456\",\"authentication\":[{\"id\":\"key-1\",\"type\":\"Secp256k1\",\"controller\":\"did:peer:recipient456\",\"publicKeyJwk\":{\"kty\":\"EC\",\"crv\":\"secp256k1\",\"x\":\"abc\",\"y\":\"def\"}}],\"keyAgreement\":[{\"id\":\"key-2\",\"type\":\"Secp256k1\",\"controller\":\"did:peer:recipient456\",\"publicKeyJwk\":{\"kty\":\"EC\",\"crv\":\"secp256k1\",\"x\":\"ghi\",\"y\":\"jkl\"}}],\"service\":[{\"id\":\"service-1\",\"type\":\"DIDCommMessaging\",\"serviceEndpoint\":{\"uri\":\"https://example.com/endpoint\",\"routingKeys\":[]}}]}";
    }
    
    [Fact]
    public void ActionType_ShouldBeDIDComm()
    {
        _processor.ActionType.Should().Be(EActionType.DIDComm);
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingRecipient_ShouldFail()
    {
        // Arrange
        var input = new DIDCommAction
        {
            Type = EDIDCommType.Message,
            SenderPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "senderpeerdid"
            },
            RecipientPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "nonexistentrecipient" // This doesn't exist in context
            }
        };
        
        var action = new Action
        {
            Type = EActionType.DIDComm,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("recipient Peer-DID is not provided");
    }
    
    [Fact]
    public async Task ProcessAsync_WithInvalidRecipientDid_ShouldFail()
    {
        // Arrange
        var input = new DIDCommAction
        {
            Type = EDIDCommType.Message,
            SenderPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "senderpeerdid"
            },
            RecipientPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "recipientpeerdid"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.DIDComm,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("recipient Peer-DID could not be resolved");
    }
    
    [Fact]
    public async Task ProcessAsync_WithInvalidDidDoc_ShouldFail()
    {
        // Arrange
        var input = new DIDCommAction
        {
            Type = EDIDCommType.Message,
            SenderPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "senderpeerdid"
            },
            RecipientPeerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "recipientpeerdid"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.DIDComm,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("he recipient Peer-DID could not be resolved: Does not match peer DID regexp: Blocktrust.PeerDID.Types.PeerDi");
    }

}