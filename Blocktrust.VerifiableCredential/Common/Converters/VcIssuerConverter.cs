namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

/// <summary>
/// Converts <see cref="CredentialIssuer" />
/// Based on DictionaryTKeyEnumTValueConverter
/// at https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to.
/// https://w3c.github.io/did-imp-guide/
/// </summary>
public class VcIssuerConverter : JsonConverter<CredentialIssuer>
{
    public override CredentialIssuer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var issuerId = reader.GetString();
            if (Uri.TryCreate(issuerId, UriKind.Absolute, out Uri? issuerUri))
            {
                return new CredentialIssuer(issuerUri);
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            Uri? issuerId = null;
            string? issuerName = null;
            string? issuerDescription = null;
            Dictionary<string, LanguageModel>? issuerNameLanguages = null;
            Dictionary<string, LanguageModel>? issuerDescriptionLanguages = null;
            Dictionary<string, object>? additionalData = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (issuerId is not null && issuerNameLanguages is null && issuerDescriptionLanguages is null)
                    {
                        return new CredentialIssuer(issuerId, issuerName, issuerDescription, null,null, additionalData);
                    }
                    else if (issuerId is not null && (issuerNameLanguages is not null || issuerDescriptionLanguages is not null))
                    {
                        return new CredentialIssuer(issuerId, issuerNameLanguages, issuerDescriptionLanguages,null, null, additionalData);
                    }
                }

                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        throw new JsonException("Failed to get property name");
                    }

                    _ = reader.Read();
                    var objectRead = ExtractValue(ref reader, options);
                    if (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (objectRead is string)
                        {
                            if (Uri.TryCreate((string)objectRead, UriKind.Absolute, out Uri? issuerUri))
                            {
                                issuerId = issuerUri;
                            }
                        }
                    }
                    else if (propertyName.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        if (objectRead is string)
                        {
                            issuerName = (string)objectRead;
                        }
                        else if (objectRead is List<object> dictionaryLanguageNames)
                        {
                            issuerNameLanguages = new Dictionary<string, LanguageModel>();
                            for (int i = 0; i < dictionaryLanguageNames.Count; i++)
                            {
                                var dict = (Dictionary<string, object>)dictionaryLanguageNames[i];
                                string? language = null;
                                string? value = null;
                                string? direction = null;
                                if (dict.TryGetValue("@language", out object? languageObj))
                                {
                                    language = languageObj.ToString();
                                }

                                if (dict.TryGetValue("@value", out object? valueObj))
                                {
                                    value = valueObj.ToString();
                                }

                                if (dict.TryGetValue("@direction", out object? directionObj))
                                {
                                    direction = directionObj.ToString();
                                }

                                if (language is not null && value is not null)
                                {
                                    issuerNameLanguages.TryAdd(language, new LanguageModel()
                                    {
                                        Value = value,
                                        Direction = direction
                                    });
                                }
                            }
                        }
                    }
                    else if (propertyName.Equals("description", StringComparison.OrdinalIgnoreCase))
                    {
                        if (objectRead is string)
                        {
                            issuerDescription = (string)objectRead;
                        }
                        else if (objectRead is List<object> dictionaryLanguageDescriptions)
                        {
                            issuerDescriptionLanguages = new Dictionary<string, LanguageModel>();
                            for (int i = 0; i < dictionaryLanguageDescriptions.Count; i++)
                            {
                                var dict = (Dictionary<string, object>)dictionaryLanguageDescriptions[i];
                                string? language = null;
                                string? value = null;
                                string? direction = null;
                                if (dict.TryGetValue("@language", out object? languageObj))
                                {
                                    language = languageObj.ToString();
                                }

                                if (dict.TryGetValue("@value", out object? valueObj))
                                {
                                    value = valueObj.ToString();
                                }

                                if (dict.TryGetValue("@direction", out object? directionObj))
                                {
                                    direction = directionObj.ToString();
                                }

                                if (language is not null && value is not null)
                                {
                                    issuerDescriptionLanguages.TryAdd(language, new LanguageModel()
                                    {
                                        Value = value,
                                        Direction = direction
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (additionalData is null)
                        {
                            additionalData = new Dictionary<string, object>();
                        }

                        additionalData.Add(propertyName, objectRead);
                    }
                }
                else
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }
            }
        }

        return null;
    }


    public override void Write(Utf8JsonWriter writer, CredentialIssuer value, JsonSerializerOptions options)
    {
        if (value.IssuerName is not null || value.IssuerDescription is not null)
        {
            writer.WriteStartObject();
            writer.WriteString("id", value.IssuerId.OriginalString);
            if (value.IssuerName is not null)
            {
                writer.WriteString("name", value.IssuerName);
            }

            if (value.IssuerDescription is not null)
            {
                writer.WriteString("description", value.IssuerDescription);
            }
            
            if(value.AdditionalData is not null)
            {
                foreach (var (key, value1) in value.AdditionalData)
                {
                    writer.WritePropertyName(key);
                    JsonSerializer.Serialize(writer, value1, options);
                }
            }

            writer.WriteEndObject();
        }
        else if (value.IssuerNameLanguages is not null || value.IssuerDescriptionLanguages is not null)
        {
            writer.WriteStartObject();
            writer.WriteString("id", value.IssuerId.OriginalString);
            if (value.IssuerNameLanguages is not null)
            {
                writer.WriteStartArray("name");
                foreach (var model in value.IssuerNameLanguages)
                {
                    writer.WriteStartObject();
                    writer.WriteString("@value", model.Value.Value);
                    writer.WriteString("@language", model.Key);
                    if (model.Value.Direction is not null)
                    {
                        writer.WriteString("@direction", model.Value.Direction);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            if (value.IssuerDescriptionLanguages is not null)
            {
                writer.WriteStartArray("description");
                foreach (var model in value.IssuerDescriptionLanguages)
                {
                    writer.WriteStartObject();
                    writer.WriteString("@value", model.Value.Value);
                    writer.WriteString("@language", model.Key);
                    if (model.Value.Direction is not null)
                    {
                        writer.WriteString("@direction", model.Value.Direction);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
            
            if(value.AdditionalData is not null)
            {
                foreach (var (key, value1) in value.AdditionalData)
                {
                    writer.WritePropertyName(key);
                    JsonSerializer.Serialize(writer, value1, options);
                }
            }
            

            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(value.IssuerId.OriginalString);
        }
    }

    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return Converter.ExtractValue(ref reader, options);
    }
}