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
                    "Template for creating a new DID using an HTTP request trigger and sending the result via email",
                    """
                    {
                      "triggers": {
                        "<GUID_1>": {
                          "type": "HttpRequest",
                          "input": {
                            "$type": "incomingRequest",
                            "method": "POST",
                            "parameters": {
                              "linkedDomainEndpoint": {
                                "type": "string",
                                "required": true,
                                "description": "Endpoint Url which goes into the service section"
                              },
                              "emailForDidResult": {
                                "type": "string",
                                "required": true,
                                "description": "Email to which the resulting DID will be send"
                              }
                            },
                            "id": "<GUID_1>"
                          }
                        }
                      },
                      "actions": {
                        "<GUID_2>": {
                          "type": "CreateDID",
                          "input": {
                            "$type": "createDID",
                            "useTenantRegistrar": false,
                            "registrarUrl": {
                              "source": "static",
                              "path": "https://opn.preprod.blocktrust.dev",
                              "defaultValue": "https://opn.preprod.blocktrust.dev"
                            },
                            "walletId": {
                              "source": "static",
                              "path": "beb041bbbc689c6762f7fb743735e9c39df25ad5",
                              "defaultValue": "beb041bbbc689c6762f7fb743735e9c39df25ad5"
                            },
                            "verificationMethods": [
                              {
                                "keyId": {
                                  "source": "static",
                                  "path": "",
                                  "defaultValue": "key-1"
                                },
                                "purpose": {
                                  "source": "static",
                                  "path": "authentication",
                                  "defaultValue": "authentication"
                                },
                                "curve": {
                                  "source": "static",
                                  "path": "Ed25519",
                                  "defaultValue": "Ed25519"
                                }
                              }
                            ],
                            "services": [
                              {
                                "serviceId": {
                                  "source": "static",
                                  "path": "",
                                  "defaultValue": "service-1"
                                },
                                "type": {
                                  "source": "static",
                                  "path": "",
                                  "defaultValue": "LinkedDomain"
                                },
                                "endpoint": {
                                  "source": "triggerInput",
                                  "path": "linkedDomainEndpoint",
                                  "defaultValue": "https://test.com"
                                }
                              }
                            ],
                            "id": "<GUID_2>"
                          },
                          "runAfter": [
                            "<GUID_1>"
                          ]
                        },
                        "<GUID_3>": {
                          "type": "Email",
                          "input": {
                            "$type": "email",
                            "to": {
                              "source": "triggerInput",
                              "path": "emailForDidResult"
                            },
                            "subject": "DID created",
                            "body": "Hey there!\nA DID was created upon your request:\n{{didResult}}\n",
                            "parameters": {
                              "didResult": {
                                "source": "actionOutcome",
                                "path": "<GUID_2>"
                              }
                            },
                            "attachments": [],
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
                    "Create W3C Credential based on form-input",
                    "Template for issuing a Credential based on a form input and sending it via email",
                    """
                    {
                      "triggers": {
                        "<GUID_1>": {
                          "type": "Form",
                          "input": {
                            "$type": "form",
                            "title": "Testresult Grade",
                            "description": "This form will generate a Credential and send it to the email provided",
                            "parameters": {
                              "Credential subject": {
                                "type": "string",
                                "required": true,
                                "description": "DID of the subject",
                                "defaultValue": "did:prism:123123123123123123123"
                              },
                              "Name": {
                                "type": "string",
                                "required": true,
                                "description": "Name of the subject"
                              },
                              "Grade": {
                                "type": "string",
                                "required": true,
                                "description": "Testresult grade",
                                "defaultValue": "A"
                              },
                              "Email": {
                                "type": "string",
                                "required": true,
                                "description": "Email where the Credential should be send to"
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
                              "path": "Credential subject"
                            },
                            "issuerDid": {
                              "source": "appSettings",
                              "path": "DefaultIssuerDid"
                            },
                            "validUntil": "2028-09-18T00:00:00",
                            "claims": {
                              "Name": {
                                "type": "TriggerProperty",
                                "value": "",
                                "parameterReference": {
                                  "source": "triggerInput",
                                  "path": "Name"
                                }
                              },
                              "Grade": {
                                "type": "TriggerProperty",
                                "value": "",
                                "parameterReference": {
                                  "source": "triggerInput",
                                  "path": "Grade"
                                }
                              },
                              "University": {
                                "type": "Static",
                                "value": "Blocktrust University",
                                "parameterReference": {
                                  "source": "triggerInput",
                                  "path": ""
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
                          "type": "Email",
                          "input": {
                            "$type": "email",
                            "to": {
                              "source": "triggerInput",
                              "path": "Email"
                            },
                            "subject": "Credential send by Email",
                            "body": "Hey. Here is your credential for {{name}}!\n\n{{credential}}",
                            "parameters": {
                              "name": {
                                "source": "triggerInput",
                                "path": "Name"
                              },
                              "credential": {
                                "source": "actionOutcome",
                                "path": "<GUID_2>"
                              }
                            },
                            "attachments": [],
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
                    "Validate W3C Credential & Issue JWT",
                    "Template for validating a W3C Verifiable Credential and Issuing a JWT",
                    """
                    {
                      "triggers": {
                        "<GUID_1>": {
                          "type": "HttpRequest",
                          "input": {
                            "$type": "incomingRequest",
                            "method": "POST",
                            "parameters": {
                              "credentialToCheck": {
                                "type": "string",
                                "required": true,
                                "description": "W3C Credential in jwt format"
                              },
                              "aud": {
                                "type": "string",
                                "required": true,
                                "description": "JWT Audiance (e.g. http://myapp.com)"
                              },
                              "emailForJwt": {
                                "type": "string",
                                "required": true,
                                "description": "Email to send the JWT to"
                              }
                            },
                            "id": "<GUID_1>"
                          }
                        }
                      },
                      "actions": {
                        "<GUID_2>": {
                          "type": "VerifyW3CCredential",
                          "input": {
                            "$type": "verifyW3cCredential",
                            "credentialReference": {
                              "source": "triggerInput",
                              "path": "credentialToCheck"
                            },
                            "checkSignature": true,
                            "checkStatus": false,
                            "checkSchema": false,
                            "checkTrustRegistry": false,
                            "checkExpiry": true,
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
                              "source": "triggerInput",
                              "path": "aud",
                              "defaultValue": ""
                            },
                            "subject": {
                              "source": "actionOutcome",
                              "path": "<GUID_2>",
                              "defaultValue": ""
                            },
                            "expiration": {
                              "source": "static",
                              "path": "",
                              "defaultValue": "36000"
                            },
                            "claims": {},
                            "claimsFromPreviousAction": true,
                            "previousActionId": "<GUID_2>",
                            "id": "<GUID_3>"
                          },
                          "runAfter": [
                            "<GUID_2>"
                          ]
                        },
                        "<GUID_4>": {
                          "type": "Email",
                          "input": {
                            "$type": "email",
                            "to": {
                              "source": "triggerInput",
                              "path": "emailForJwt"
                            },
                            "subject": "A Credential was validated and a JWT created",
                            "body": "{{jwt}}",
                            "parameters": {
                              "jwt": {
                                "source": "actionOutcome",
                                "path": "<GUID_3>"
                              }
                            },
                            "attachments": [],
                            "id": "<GUID_4>"
                          },
                          "runAfter": [
                            "<GUID_3>"
                          ]
                        }
                      }
                    }
                    """
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