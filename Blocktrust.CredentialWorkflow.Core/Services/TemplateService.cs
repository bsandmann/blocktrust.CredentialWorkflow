using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.Template;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services
{
    public class TemplateService
    {
        private readonly List<Template> _templates;

        public TemplateService()
        {
            _templates = new List<Template>
            {
                new Template(
                    Guid.NewGuid(),
                    "Create DID with HttpRequest",
                    "Template for creating a new DID using an HTTP request trigger",
                    "{\"trigger\":{\"type\":\"HttpRequest\",\"parameters\":[{\"name\":\"did\",\"type\":\"string\"},{\"name\":\"method\",\"type\":\"string\"}]},\"actions\":[{\"type\":\"CreateDID\",\"id\":\"<GUID_1>\",\"inputs\":{\"did\":\"{{trigger.did}}\",\"method\":\"{{trigger.method}}\"}}]}"
                ),
                new Template(
                    Guid.NewGuid(),
                    "Update DID with HttpRequest",
                    "Template for updating an existing DID using an HTTP request trigger",
                    "{\"trigger\":{\"type\":\"HttpRequest\",\"parameters\":[{\"name\":\"did\",\"type\":\"string\"},{\"name\":\"updates\",\"type\":\"object\"}]},\"actions\":[{\"type\":\"UpdateDID\",\"id\":\"<GUID>\",\"inputs\":{\"did\":\"{{trigger.did}}\",\"updates\":\"{{trigger.updates}}\"}}]}"
                ),
                new Template(
                    Guid.NewGuid(),
                    "Validate W3C Credential",
                    "Template for validating a W3C Verifiable Credential and returning the result",
                    "{\"trigger\":{\"type\":\"HttpRequest\",\"parameters\":[{\"name\":\"credential\",\"type\":\"string\"}]},\"actions\":[{\"type\":\"ValidateCredential\",\"id\":\"<GUID_1>\",\"inputs\":{\"credential\":\"{{trigger.credential}}\"}},{\"type\":\"SendResponse\",\"id\":\"<GUID_2>\",\"inputs\":{\"result\":\"{{actions.<GUID_1>.result}}\"}}]}"
                )
            };
        }

        public Result<List<Template>> ListTemplates()
        {
            return Result.Ok(_templates);
        }

        public Result<Template> GetTemplateById(Guid id)
        {
            var template = _templates.FirstOrDefault(t => t.Id == id);
            
            if (template == null)
            {
                return Result.Fail($"Template with ID {id} not found");
            }

            // Process template body to replace GUID placeholders
            var processedTemplateBody = ProcessTemplateGuidPlaceholders(template.TemplateBody);
            
            // Create a new template with the processed body
            var processedTemplate = new Template(
                template.Id,
                template.Name,
                template.Description,
                processedTemplateBody
            );

            return Result.Ok(processedTemplate);
        }

        private string ProcessTemplateGuidPlaceholders(string templateBody)
        {
            var result = templateBody;
            var guidPlaceholderMap = new Dictionary<string, string>();
            
            // Replace each occurrence of <GUID> with a new Guid
            result = Regex.Replace(result, "<GUID>", match => Guid.NewGuid().ToString());
            
            // Find all occurrences of <GUID_X> (where X is a number)
            var guidPattern = new Regex("<GUID_([0-9]+)>");
            var matches = guidPattern.Matches(templateBody);
            
            // Process each unique placeholder
            foreach (Match match in matches)
            {
                var placeholder = match.Value;
                
                // If we haven't seen this placeholder before, generate a new GUID for it
                if (!guidPlaceholderMap.ContainsKey(placeholder))
                {
                    guidPlaceholderMap[placeholder] = Guid.NewGuid().ToString();
                }
                
                // Replace all occurrences of this placeholder with its corresponding GUID
                result = result.Replace(placeholder, guidPlaceholderMap[placeholder]);
            }
            
            return result;
        }
    }
}