namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcTermsOfUseConverter : JsonConverter<List<CredentialOrPresentationTermsOfUse>>
{
    public override List<CredentialOrPresentationTermsOfUse>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var termsOfUse = ReadtermsOfUse(ref reader, options);
            return new List<CredentialOrPresentationTermsOfUse>() { termsOfUse };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var termsOfUses = new List<CredentialOrPresentationTermsOfUse>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (termsOfUses.Count == 1)
                    {
                        return new List<CredentialOrPresentationTermsOfUse>()
                        {
                            new CredentialOrPresentationTermsOfUse()
                            {
                                Id = termsOfUses[0].Id,
                                Type = termsOfUses[0].Type,
                                AdditionalData = termsOfUses[0].AdditionalData,
                                SerializationOption = new SerializationOption()
                                {
                                    UseArrayEvenForSingleElement = true
                                }
                            }
                        };
                    }

                    return termsOfUses;
                }

                var termsOfUse = ReadtermsOfUse(ref reader, options);
                termsOfUses.Add(termsOfUse);
            }
        }

        return null;
    }

    private static CredentialOrPresentationTermsOfUse ReadtermsOfUse(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Uri? id = null;
        string type = string.Empty;
        IDictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialOrPresentationTermsOfUse()
                {
                    Id = id,
                    Type = type,
                    AdditionalData = additionalData,
                };
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"JsonTokenType was not {nameof(JsonTokenType.PropertyName)}");
            }

            var propertyName = reader.GetString();

            _ = reader.Read();
            object? val = Converter.ExtractValue(ref reader, options);

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new JsonException("Failed to get property name");
            }

            if (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                if (val is string)
                {
                    if (Uri.TryCreate((string)val, UriKind.Absolute, out Uri? issuerUri))
                    {
                        id = issuerUri;
                    }
                }
            }
            else if (propertyName.Equals("type", StringComparison.InvariantCultureIgnoreCase))
            {
                if (val is string)
                {
                    type = (string)val;
                }
            }
            else if (val != null)
            {
                if (additionalData is null)
                {
                    additionalData = new Dictionary<string, object>();
                }

                additionalData.Add(propertyName, val);
            }
        }

        return new CredentialOrPresentationTermsOfUse() { Type = String.Empty };
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialOrPresentationTermsOfUse> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            if (value[0].SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                WriteTermsOfUse(writer, value[0]);
                writer.WriteEndArray();
            }
            else
            {
                WriteTermsOfUse(writer, value[0]);
            }
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var termsOfUse in value)
            {
                WriteTermsOfUse(writer, termsOfUse);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteTermsOfUse(Utf8JsonWriter writer, CredentialOrPresentationTermsOfUse credentialOrPresentationTermsOfUse)
    {
        if ((credentialOrPresentationTermsOfUse.AdditionalData is null || credentialOrPresentationTermsOfUse.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            if (credentialOrPresentationTermsOfUse.Id is not null)
            {
                writer.WriteString("id", credentialOrPresentationTermsOfUse.Id.OriginalString);
            }

            writer.WriteString("type", credentialOrPresentationTermsOfUse.Type);
            writer.WriteEndObject();
        }
        else if (credentialOrPresentationTermsOfUse.AdditionalData is not null && credentialOrPresentationTermsOfUse.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the id is the first property
            // And serializing all in one go.
            Dictionary<string, object> newTempDictionary;
            if (credentialOrPresentationTermsOfUse.Id is not null)
            {
                newTempDictionary = new Dictionary<string, object>
                {
                    { "id", credentialOrPresentationTermsOfUse.Id.OriginalString },
                    { "type", credentialOrPresentationTermsOfUse.Type }
                };
            }
            else
            {
                newTempDictionary = new Dictionary<string, object>
                {
                    { "type", credentialOrPresentationTermsOfUse.Type }
                };
            }

            foreach (var keyValuePair in credentialOrPresentationTermsOfUse.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialOrPresentationTermsOfUse.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialOrPresentationTermsOfUse.AdditionalData);
        }
    }
}