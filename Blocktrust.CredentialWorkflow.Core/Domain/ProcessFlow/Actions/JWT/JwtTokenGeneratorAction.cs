using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;

public class JwtTokenGeneratorAction : ActionInput
{
    [JsonPropertyName("issuer")]
    public ParameterReference Issuer { get; set; } = new ParameterReference
    {
        Source = ParameterSource.AppSettings,
        Path = "HostUrl"
    };
    
    [JsonPropertyName("audience")]
    public ParameterReference Audience { get; set; } = new ParameterReference
    {
        Source = ParameterSource.Static,
        DefaultValue = ""
    };
    
    [JsonPropertyName("subject")]
    public ParameterReference Subject { get; set; } = new ParameterReference
    {
        Source = ParameterSource.Static,
        DefaultValue = ""
    };
    
    [JsonPropertyName("expiration")]
    public ParameterReference Expiration { get; set; } = new ParameterReference
    {
        Source = ParameterSource.Static,
        DefaultValue = "3600" // Default: 1 hour
    };
    
    [JsonPropertyName("claims")]
    public Dictionary<string, ClaimValue> Claims { get; set; } = new();
    
    [JsonPropertyName("claimsFromPreviousAction")]
    public bool ClaimsFromPreviousAction { get; set; } = false;
    
    [JsonPropertyName("previousActionId")]
    public Guid? PreviousActionId { get; set; }
}