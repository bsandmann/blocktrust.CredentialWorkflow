using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

public class EmailAction : ActionInput
{
    public ParameterReference To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public Dictionary<string, ParameterReference> Parameters { get; set; }

    public EmailAction()
    {
        Id = Guid.NewGuid();
        To = new ParameterReference { Source = ParameterSource.Static };
        Subject = string.Empty;
        Body = string.Empty;
        Parameters = new Dictionary<string, ParameterReference>();
    }
}