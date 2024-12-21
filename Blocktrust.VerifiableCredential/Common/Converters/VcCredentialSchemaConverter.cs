namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcCredentialSchemaConverter : JsonConverter<List<CredentialSchema>>
{
    public override List<CredentialSchema>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var credentialSchema = ReadCredentialSchema(ref reader, options);
            return new List<CredentialSchema>() { credentialSchema };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var credentialSchemas = new List<CredentialSchema>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (credentialSchemas.Count == 1)
                    {
                        return new List<CredentialSchema>()
                        {
                            new CredentialSchema()
                            {
                                Id = credentialSchemas[0].Id,
                                Type = credentialSchemas[0].Type,
                                AdditionalData = credentialSchemas[0].AdditionalData,
                                SerializationOption = new SerializationOption()
                                {
                                    UseArrayEvenForSingleElement = true
                                }
                            }
                        };
                    }

                    return credentialSchemas;
                }

                var credentialSchema = ReadCredentialSchema(ref reader, options);
                credentialSchemas.Add(credentialSchema);
            }
        }

        return null;
    }

    private static CredentialSchema ReadCredentialSchema(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Uri id = null!;
        string type = string.Empty;
        IDictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialSchema()
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

        return new CredentialSchema() { Id = new Uri(String.Empty), Type = String.Empty };
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialSchema> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            if (value[0].SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                WriteCredentialSchema(writer, value[0]);
                writer.WriteEndArray();
            }
            else
            {
                WriteCredentialSchema(writer, value[0]);
            }
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var credentialSchema in value)
            {
                WriteCredentialSchema(writer, credentialSchema);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteCredentialSchema(Utf8JsonWriter writer, CredentialSchema credentialSchema)
    {
        if ((credentialSchema.AdditionalData is null || credentialSchema.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            writer.WriteString("id", credentialSchema.Id.OriginalString);
            writer.WriteString("type", credentialSchema.Type);
            writer.WriteEndObject();
        }
        else if (credentialSchema.AdditionalData is not null && credentialSchema.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the id is the first property
            // And serializing all in one go.
            var newTempDictionary = new Dictionary<string, object>
            {
                { "id", credentialSchema.Id.OriginalString },
                { "type", credentialSchema.Type }
            };
            foreach (var keyValuePair in credentialSchema.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialSchema.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialSchema.AdditionalData);
        }
    }
}