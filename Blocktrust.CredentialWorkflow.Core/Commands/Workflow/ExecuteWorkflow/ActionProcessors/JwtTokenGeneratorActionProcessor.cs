using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;
using FluentResults;
using MediatR;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Services;
using VerifiableCredential;
using Action = Domain.ProcessFlow.Actions.Action;

public class JwtTokenGeneratorActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public JwtTokenGeneratorActionProcessor(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }

    public EActionType ActionType => EActionType.JwtTokenGenerator;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (JwtTokenGeneratorAction)action.Input;

        try
        {
            // Get issuer
            var issuer = await ParameterResolver.GetParameterFromExecutionContext(
                input.Issuer, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(issuer))
            {
                var errorMessage = "No issuer provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get audience
            var audience = await ParameterResolver.GetParameterFromExecutionContext(
                input.Audience, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(audience))
            {
                var errorMessage = "No audience provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get subject
            var subject = await ParameterResolver.GetParameterFromExecutionContext(
                input.Subject, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            // Parse expiration
            var expirationStr = await ParameterResolver.GetParameterFromExecutionContext(
                input.Expiration, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(expirationStr) || !int.TryParse(expirationStr, out int expirationSeconds))
            {
                var errorMessage = "Invalid expiration provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get claims
            var claims = new Dictionary<string, string>();

            if (input.ClaimsFromPreviousAction && input.PreviousActionId.HasValue)
            {
                var outcomeJson = context.ActionOutcomes.FirstOrDefault()?.OutcomeJson;
                if (string.IsNullOrWhiteSpace(outcomeJson))
                {
                    var errorMessage = "Cannot find valid previous action outcome.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                if (outcomeJson.StartsWith("ey"))
                {
                    var credentialResult = JwtParser.Parse(outcomeJson);
                    if (credentialResult.IsFailed)
                    {
                        var errorMessage = $"Could not parse credential from previous action outcome: {credentialResult.Errors.FirstOrDefault()?.Message}";
                        actionOutcome.FinishOutcomeWithFailure(errorMessage);
                        return Result.Fail(errorMessage);
                    }

                    var credential = credentialResult.Value;
                    if (credential.VerifiableCredentials.FirstOrDefault() != null)
                    {
                        var subjects = credential.VerifiableCredentials.FirstOrDefault().CredentialSubjects;
                        if (subjects != null)
                        {
                            foreach (var claim in subjects.FirstOrDefault().AdditionalData)
                            {
                                var key = claim.Key;
                                var claimValue = claim.Value;

                                if (claimValue is string stringValue)
                                {
                                    claims[key] = stringValue;
                                }
                                else if (claimValue is JsonElement jsonElement)
                                {
                                    claims[key] = jsonElement.ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    var errorMessage = "Unexpected outcome format from previous action.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }
            else
            {
                //Process manually defined claims
                foreach (var claimPair in input.Claims)
                {
                    var key = claimPair.Key;
                    var claimValue = claimPair.Value;

                    string resolvedValue;
                    if (claimValue.Type == ClaimValueType.Static)
                    {
                        resolvedValue = claimValue.Value;
                    }
                    else if (claimValue.Type == ClaimValueType.TriggerProperty && claimValue.ParameterReference != null)
                    {
                        resolvedValue = await ParameterResolver.GetParameterFromExecutionContext(
                            claimValue.ParameterReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
                    }
                    else
                    {
                        continue; // Skip invalid claim
                    }

                    if (!string.IsNullOrEmpty(resolvedValue))
                    {
                        claims[key] = resolvedValue;
                    }
                }
            }

            // try
            // {
            //     string token = GenerateJwtToken(
            //         issuer: issuer, // e.g., "https://my-prototype.example.com"
            //         subject: subject, // e.g., "did:example:12345"
            //         audience: audience, // e.g., "https://my-api.example.com"
            //         signingKey: mySecureKey, // Your securely loaded Signing Key object
            //         signingAlgorithm: algorithm, // e.g., SecurityAlgorithms.RsaSha256
            //         expirationMinutes: 15, // e.g., 15
            //         additionalClaims: claims
            //     );
            //     actionOutcome.FinishOutcomeWithSuccess(token);
            //     return Result.Ok();
            // }
            // catch (Exception ex)
            // {
            //     // Log the error
            //     // Return appropriate error response
            //     throw; // Or handle gracefully
            // }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing JWT token generation: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        //TODO
        actionOutcome.FinishOutcomeWithFailure("");
        return Result.Fail("");
    }

    /// <summary>
    /// Generates a JWT bearer token with standard claims.
    /// </summary>
    /// <param name="issuer">The 'iss' claim - identifier of your prototype.</param>
    /// <param name="subject">The 'sub' claim - identifier of the user (e.g., DID from VC).</param>
    /// <param name="audience">The 'aud' claim - identifier of the intended recipient(s).</param>
    /// <param name="signingKey">The security key used to sign the token. KEEP THIS SECURE.</param>
    /// <param name="signingAlgorithm">The algorithm to use for signing (e.g., SecurityAlgorithms.HmacSha256 or SecurityAlgorithms.RsaSha256).</param>
    /// <param name="expirationMinutes">How many minutes the token should be valid for.</param>
    /// <param name="additionalClaims">Optional list of any other claims to include (e.g., derived from VC credentialSubject).</param>
    /// <returns>The generated JWT token string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required arguments are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if expirationMinutes is not positive.</exception>
    public static string GenerateJwtToken(
        string issuer,
        string subject,
        string audience,
        SecurityKey signingKey,
        string signingAlgorithm,
        int expirationMinutes,
        IEnumerable<Claim> additionalClaims = null)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(issuer)) throw new ArgumentNullException(nameof(issuer));
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));
        if (string.IsNullOrWhiteSpace(audience)) throw new ArgumentNullException(nameof(audience));
        if (signingKey == null) throw new ArgumentNullException(nameof(signingKey));
        if (string.IsNullOrWhiteSpace(signingAlgorithm)) throw new ArgumentNullException(nameof(signingAlgorithm));
        if (expirationMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(expirationMinutes), "Expiration must be positive.");

        // 1. Security Token Handler: The main class for JWT operations.
        var tokenHandler = new JwtSecurityTokenHandler();

        // 2. Timestamps: Use UTC for consistency.
        var issuedAt = DateTime.UtcNow;
        var expires = issuedAt.AddMinutes(expirationMinutes);

        // 3. Claims: Define the information carried by the token.
        // Start with essential claims. 'jti' (JWT ID) is recommended for uniqueness.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token identifier
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Issued At timestamp
        };

        // Add any custom claims passed in (e.g., roles, specific data from VC)
        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        // Create the identity associated with the claims
        var claimsIdentity = new ClaimsIdentity(claims);

        // 4. Signing Credentials: Combine the key and algorithm.
        var signingCredentials = new SigningCredentials(signingKey, signingAlgorithm);

        // 5. Security Token Descriptor: Gathers all information needed to create the token.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity, // The identity/claims for the token
            Issuer = issuer, // 'iss' claim: Who issued the token
            Audience = audience, // 'aud' claim: Who the token is for
            Expires = expires, // 'exp' claim: When the token expires (UTC)
            IssuedAt = issuedAt, // 'iat' claim: When the token was issued (UTC) - Optional if iat claim is added manually
            NotBefore = issuedAt, // 'nbf' claim: Token is not valid before this time (optional) - Usually same as IssuedAt
            SigningCredentials = signingCredentials // How the token is signed
        };

        // 6. Create the Token Object: Use the handler and descriptor.
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        // 7. Serialize the Token: Convert the token object into the compact JWT string format.
        var jwtString = tokenHandler.WriteToken(securityToken);

        return jwtString;
    }
}