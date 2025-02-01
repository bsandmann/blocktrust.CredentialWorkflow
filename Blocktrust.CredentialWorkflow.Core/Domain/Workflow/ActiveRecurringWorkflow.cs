namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow
{
    using System;
    using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

    public class ActiveRecurringWorkflow
    {
        public Guid WorkflowEntityId { get; set; }

        public EWorkflowState WorkflowState { get; set; }

        public string? CronExpression { get; set; }

        public DateTime UpdatedUtc { get; set; }
    }
}