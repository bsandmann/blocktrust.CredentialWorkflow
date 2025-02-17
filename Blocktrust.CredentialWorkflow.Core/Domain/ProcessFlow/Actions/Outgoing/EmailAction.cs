using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

public class EmailAction : ActionInput
{
    [JsonPropertyName("to")]
    public ParameterReference To { get; set; }
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
    
    [JsonPropertyName("body")]
    public string Body { get; set; }
    
    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterReference> Parameters { get; set; }
    
    [JsonPropertyName("attachments")]
    public List<ParameterReference> Attachments { get; set; }

    public EmailAction()
    {
        Id = Guid.NewGuid();
        To = new ParameterReference { Source = ParameterSource.Static };
        Subject = string.Empty;
        Body = string.Empty;
        Parameters = new Dictionary<string, ParameterReference>();
        Attachments = new List<ParameterReference>();
    }
}