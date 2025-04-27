using System;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Template
{
    public class Template
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string TemplateBody { get; }

        public Template(Guid id, string name, string description, string templateBody)
        {
            Id = id;
            Name = name;
            Description = description;
            TemplateBody = templateBody;
        }
    }
}