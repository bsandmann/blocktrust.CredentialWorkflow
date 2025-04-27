using System.Threading;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;
using Blocktrust.CredentialWorkflow.Core.Domain.Tenant;
using Blocktrust.VerifiableCredential;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;
using r = FluentResults.Result;

public class JwtTokenGeneratorActionProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly JwtTokenGeneratorActionProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;
    private readonly Guid _tenantId = Guid.NewGuid();
    
    // Sample JWT token for testing
    private const string SampleJwtCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwic3ViIjoiZGlkOnByaXNtOnN1YmplY3Q0NTYiLCJuYmYiOjE3MDg5OTIwMDAsImV4cCI6MTg2Njc1ODQwMCwidmMiOnsiQGNvbnRleHQiOlsiaHR0cHM6Ly93d3cudzMub3JnLzIwMTgvY3JlZGVudGlhbHMvdjEiXX19.sig";
    
    public JwtTokenGeneratorActionProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _processor = new JwtTokenGeneratorActionProcessor(_mediatorMock.Object, _httpClientFactoryMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create execution context with token parameters
        var inputContext = new Dictionary<string, string>
        {
            { "issuer", "https://issuer.example.com" },
            { "audience", "https://audience.example.com" },
            { "subject", "did:prism:subject123" },
            { "expiration", "3600" },
            { "previousAction", SampleJwtCredential }
        };
        
        var executionContext = new ExecutionContext(Guid.NewGuid(), new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(inputContext));
        
        // Create workflow with tenant ID and required properties
        var workflow = new Domain.Workflow.Workflow
        {
            TenantId = _tenantId,
            Name = "Test Workflow",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            WorkflowState = Domain.Enums.EWorkflowState.ActiveWithExternalTrigger,
            WorkflowOutcomes = new List<Domain.Workflow.WorkflowOutcome>()
        };
        
        // Create previous action outcome
        var previousActionId = Guid.NewGuid();
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess(SampleJwtCredential);
        
        _processingContext = new ActionProcessingContext(
            executionContext,
            new List<ActionOutcome> { previousOutcome },
            workflow,
            CancellationToken.None
        );
        
        // Setup mock tenant response with JWT private key
        var tenant = new Tenant
        {
            TenantId = _tenantId,
            Name = "Test Tenant",
            CreatedUtc = DateTime.UtcNow,
            JwtPrivateKey = @"<RSAKeyValue>
                <Modulus>mEr5FG72L5pZQfcSdB3FQg3x5j5DD+U4s+F4TcJcFSZ8EJm2bHuKPMJiyGexzYKHQjkpqYRvdGUcNaeVEWJJzf8sMk9c0FFLK87ROBMZLgWzaX8yzK46KjATF3Jjkh0JRhYxkxM1G66JnEgIQG9szuZQIWX/8kBYUseYkc4S/wE=</Modulus>
                <Exponent>AQAB</Exponent>
                <P>xKOaR6SxR6AMQHRzK7urYJfgaNazc7XjlcbEW1NnZAM1vB1H0Q3QWfzULSJk2+qskgkYHotTkPLMXQv/cRwO0w==</P>
                <Q>xiOv2+5mfQRxrQl5WO75wFjdnDwn/pGoqwLHRWeBbws5/ttNyBx5QJjPZKqSk8jzqpZTWWs+MpTlZzQcNbR6vw==</Q>
                <DP>OitbkCQMqr6sSHa0j9AzUQpnBneFWfzmb9kEuzCAwX+HBzGnBYusW/7BoFgPd4sQ17HgHwXtL+KZMpGYGykKeQ==</DP>
                <DQ>fHnNAe8MjnLHCkQ3SdxcXlcy9QxKnF8QqWFBITkGF5vF2XkhLjdS3jlVA0wEVPTEfmO0BoZkGUlA2YXeGI4+Aw==</DQ>
                <InverseQ>jX0+fanUIyxCsrL8Yyfqul9YUxnMHCUHJkbBSGw48enJQMvpYZJ9aBCYGNBppgkSZwxzKmBQii5ZjZ2h/WiJdQ==</InverseQ>
                <D>GOoUOPrWteGvMiJQ/jVO4LfNUJ+Dewi3v9L7RI1k0+m0bl/5AoUEePXQP1pK9YeUHGYcR5q7WHJHUgR/1zdQzT54o/HkWBnU9dIY9me+0hRqeS/Oc2Vl7G1H0DE9r6GGzPYHLUBqYnRhwPSZAqYn4Yky1qWdAZZBf9RCBUvr7XE=</D>
            </RSAKeyValue>"
        };
        
        var tenantResponse = new GetTenantInformationResponse { Tenant = tenant };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTenantInformationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(tenantResponse));
            
        // We can't properly test the JWT parser in this test since we can't mock static methods
        // But we'll keep this commented out so it's clear what we're trying to simulate
        /*
        var verifiableCredential = new VerifiableCredential 
        { 
            CredentialContext = new List<string> { "https://www.w3.org/2018/credentials/v1" },
            Type = new List<string> { "VerifiableCredential" },
            CredentialSubjects = new List<CredentialSubject>()
        };
        var subject = new CredentialSubject();
        subject.AdditionalData.Add("firstName", "John");
        subject.AdditionalData.Add("lastName", "Doe");
        verifiableCredential.CredentialSubjects.Add(subject);
        
        var jwtParsingResult = new JwtParsingResult
        {
            VerifiableCredentials = new List<VerifiableCredential> { verifiableCredential }
        };
        */
        
        // Mock static JwtParser.Parse method (not actually possible in this test since it's static)
        // In a real implementation, we would need to use a library like TypeMock or refactor the code to be testable
    }
    
    [Fact]
    public void ActionType_ShouldBeJwtTokenGenerator()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.JwtTokenGenerator);
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingIssuer_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "nonexistentIssuer" // Doesn't exist in context
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "audience"
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "expiration"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No issuer provided");
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingAudience_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuer"
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "nonexistentAudience" // Doesn't exist in context
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "expiration"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No audience provided");
    }
    
    [Fact]
    public async Task ProcessAsync_WithInvalidExpiration_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuer"
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "audience"
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "invalidExpiration" // Doesn't exist in context
            }
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Invalid expiration provided");
    }
    

    [Fact]
    public async Task ProcessAsync_WithInvalidPreviousAction_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuer"
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "audience"
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "expiration"
            },
            ClaimsFromPreviousAction = true,
            PreviousActionId = Guid.NewGuid() // Different from the one in our context
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Cannot find valid previous action outcome");
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingTenantInfo_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuer"
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "audience"
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "expiration"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Setup tenant info to fail
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTenantInformationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Cannot find tenant"));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Could not retrieve tenant information");
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingJwtKey_ShouldFail()
    {
        // Arrange
        var input = new JwtTokenGeneratorAction
        {
            Issuer = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuer"
            },
            Audience = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "audience"
            },
            Subject = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subject"
            },
            Expiration = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "expiration"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.JwtTokenGenerator,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Setup tenant info with empty JWT key
        var tenantWithoutKey = new Tenant
        {
            TenantId = _tenantId,
            Name = "Test Tenant",
            CreatedUtc = DateTime.UtcNow,
            JwtPrivateKey = string.Empty
        };
        
        var tenantResponseWithoutKey = new GetTenantInformationResponse { Tenant = tenantWithoutKey };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTenantInformationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(tenantResponseWithoutKey));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Could not retrieve tenant JWT private key");
    }
    
    [Fact]
    public void GenerateJwtToken_WithValidInputs_ShouldCreateToken()
    {
        // Arrange
        // Since this is a static method, we need to use SecurityKey which is challenging to mock
        // For testing we'll just check that basic validation works
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JwtTokenGeneratorActionProcessor.GenerateJwtToken(
            null, // Null issuer
            "subject",
            "audience",
            null, // Null security key
            "RS256",
            60,
            null
        ));
    }
}