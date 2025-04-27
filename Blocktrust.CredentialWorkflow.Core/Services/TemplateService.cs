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
                    """
                    {
                      "triggers": {
                        "<GUID_1>": {
                          "type": "HttpRequest",
                          "input": {
                            "$type": "incomingRequest",
                            "method": "POST",
                            "parameters": {
                              "SubjectDid": {
                                "type": "string",
                                "required": true,
                                "description": ""
                              },
                              "Name": {
                                "type": "string",
                                "required": true,
                                "description": ""
                              },
                              "Age": {
                                "type": "string",
                                "required": true,
                                "description": ""
                              }
                            },
                            "id": "<GUID_1>"
                          }
                        }
                      },
                      "actions": {
                        "<GUID_2>": {
                          "type": "IssueW3CCredential",
                          "input": {
                            "$type": "issueW3cCredential",
                            "subjectDid": {
                              "source": "triggerInput",
                              "path": "SubjectDid"
                            },
                            "issuerDid": {
                              "source": "appSettings",
                              "path": "DefaultIssuerDid"
                            },
                            "validUntil": "2026-04-30T00:00:00",
                            "claims": {
                              "subject-name": {
                                "type": "TriggerProperty",
                                "value": "",
                                "parameterReference": {
                                  "source": "triggerInput",
                                  "path": "Name"
                                }
                              },
                              "subject-age": {
                                "type": "TriggerProperty",
                                "value": "",
                                "parameterReference": {
                                  "source": "triggerInput",
                                  "path": "Age"
                                }
                              }
                            },
                            "id": "<GUID_2>"
                          },
                          "runAfter": [
                            "<GUID_1>"
                          ]
                        },
                        "<GUID_3>": {
                          "type": "JwtTokenGenerator",
                          "input": {
                            "$type": "jwtTokenGenerator",
                            "issuer": {
                              "source": "static",
                              "path": "",
                              "defaultValue": "<HOST_URL>/<GUID_TENANT>"
                            },
                            "audience": {
                              "source": "static",
                              "path": "",
                              "defaultValue": "https://mynewapp,com"
                            },
                            "subject": {
                              "source": "triggerInput",
                              "path": "SubjectDid",
                              "defaultValue": ""
                            },
                            "expiration": {
                              "source": "triggerInput",
                              "path": "Age",
                              "defaultValue": ""
                            },
                            "claims": {},
                            "claimsFromPreviousAction": true,
                            "previousActionId": "<GUID_2>",
                            "id": "<GUID_3>"
                          },
                          "runAfter": [
                            "<GUID_2>"
                          ]
                        }
                      }
                    }
                    """
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
                    "{\"trigger\":{\"type\":\"HttpRequest\",\"parameters\":[{\"name\":\"credential\",\"type\":\"string\"}]},\"actions\":[{\"type\":\"ValidateCredential\",\"id\":\"<GUID_1>\",\"inputs\":{\"credential\":\"{{trigger.credential}}\",\"tenantId\":\"<GUID_TENANT>\"}},{\"type\":\"SendResponse\",\"id\":\"<GUID_2>\",\"inputs\":{\"result\":\"{{actions.<GUID_1>.result}}\"}}]}"
                )
            };
        }

        public Result<List<Template>> ListTemplates()
        {
            return Result.Ok(_templates);
        }

        public Result<Template> GetTemplateById(Guid id, Guid? tenantId = null, string hostUrl = null)
        {
            var template = _templates.FirstOrDefault(t => t.Id == id);

            if (template == null)
            {
                return Result.Fail($"Template with ID {id} not found");
            }

            // Process template body to replace GUID and host URL placeholders
            var processedTemplateBody = ProcessTemplatePlaceholders(template.TemplateBody, tenantId, hostUrl);

            // Create a new template with the processed body
            var processedTemplate = new Template(
                template.Id,
                template.Name,
                template.Description,
                processedTemplateBody
            );

            return Result.Ok(processedTemplate);
        }

        /// <summary>
        /// Processes a template body to replace all placeholders with their actual values.
        /// </summary>
        /// <param name="templateBody">The template body containing placeholders</param>
        /// <param name="tenantId">Optional tenant ID to use for <GUID_TENANT> placeholders</param>
        /// <param name="hostUrl">Optional host URL to use for <HOST_URL> placeholders</param>
        /// <returns>Processed template body with all placeholders replaced</returns>
        /// <remarks>
        /// This method handles several types of placeholders:
        /// - <GUID>: Replaced with a new unique GUID each time
        /// - <GUID_X> (where X is a number): All occurrences of the same placeholder are replaced with the same GUID
        /// - <GUID_TENANT>: Replaced with the provided tenant ID (if available)
        /// - <HOST_URL>: Replaced with the provided host URL (if available)
        /// </remarks>
        private string ProcessTemplatePlaceholders(string templateBody, Guid? tenantId = null, string hostUrl = null)
        {
            var result = templateBody;
            var guidPlaceholderMap = new Dictionary<string, string>();

            // Replace each occurrence of <GUID> with a new Guid
            result = Regex.Replace(result, "<GUID>", match => Guid.NewGuid().ToString());

            // Replace each occurrence of <GUID_TENANT> with the tenant's Guid (if provided)
            if (tenantId.HasValue)
            {
                result = result.Replace("<GUID_TENANT>", tenantId.Value.ToString());
            }

            // Process <HOST_URL> placeholders
            if (!string.IsNullOrEmpty(hostUrl))
            {
                // Ensure the URL doesn't end with a trailing slash
                var normalizedHostUrl = hostUrl.TrimEnd('/');

                // Simply replace the <HOST_URL> placeholder with the normalized host URL
                result = result.Replace("<HOST_URL>", normalizedHostUrl);

                // Fix any placeholder combinations that might have resulted in improper URL formatting
                result = NormalizeUrls(result);
            }

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

            // Final pass to normalize URLs after all replacements
            return NormalizeUrls(result);
        }

        /// <summary>
        /// Normalizes URLs in the given string by ensuring proper slash formatting.
        /// </summary>
        /// <param name="input">String containing URLs that need normalization</param>
        /// <returns>String with normalized URLs</returns>
        private string NormalizeUrls(string input)
        {
            // Find URL patterns in the string
            var urlPattern = new Regex(@"(https?://[^/\s]+)(/+)([^/\s]*)");

            // Fix double slashes in URLs (except after the scheme part)
            return urlPattern.Replace(input, match =>
            {
                var baseUrl = match.Groups[1].Value; // e.g., "https://example.com"
                var slashes = match.Groups[2].Value; // one or more slashes
                var path = match.Groups[3].Value; // the rest of the path

                // Always ensure a single slash between base URL and path
                return $"{baseUrl}/{path}";
            });
        }
    }
}