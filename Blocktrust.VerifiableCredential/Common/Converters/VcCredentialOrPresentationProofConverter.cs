namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcCredentialOrPresentationProofConverter : JsonConverter<List<CredentialOrPresentationProof>>
{
    public override List<CredentialOrPresentationProof>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var credentialProof = ReadCredentialProof(ref reader, options);
            return new List<CredentialOrPresentationProof>() { credentialProof };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var credentialProofs = new List<CredentialOrPresentationProof>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (credentialProofs.Count == 1)
                    {
                        return new List<CredentialOrPresentationProof>()
                        {
                            new CredentialOrPresentationProof()
                            {
                                Type = credentialProofs[0].Type,
                                AdditionalData = credentialProofs[0].AdditionalData,
                                SerializationOption = new SerializationOption()
                                {
                                    UseArrayEvenForSingleElement = true
                                }
                            }
                        };
                    }

                    return credentialProofs;
                }

                var credentialProof = ReadCredentialProof(ref reader, options);
                credentialProofs.Add(credentialProof);
            }
        }

        return null;
    }

    private static CredentialOrPresentationProof ReadCredentialProof(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        string type = String.Empty;
        Dictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialOrPresentationProof()
                {
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

            if (propertyName.Equals("type", StringComparison.OrdinalIgnoreCase))
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

        return new CredentialOrPresentationProof() { Type = string.Empty };
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialOrPresentationProof> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            if (value[0].SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                WriteCredentialProof(writer, value[0]);
                writer.WriteEndArray();
            }
            else
            {
                WriteCredentialProof(writer, value[0]);
            }
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var credentialProof in value)
            {
                WriteCredentialProof(writer, credentialProof);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteCredentialProof(Utf8JsonWriter writer, CredentialOrPresentationProof credentialOrPresentationProof)
    {
        if ((credentialOrPresentationProof.AdditionalData is null || credentialOrPresentationProof.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            writer.WriteString("type", credentialOrPresentationProof.Type);
            writer.WriteEndObject();
        }
        else if (credentialOrPresentationProof.AdditionalData is not null && credentialOrPresentationProof.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the type is the first property
            // And serializing all in one go.
            var newTempDictionary = new Dictionary<string, object> { { "type", credentialOrPresentationProof.Type } };
            foreach (var keyValuePair in credentialOrPresentationProof.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialOrPresentationProof.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialOrPresentationProof.AdditionalData);
        }
    }
}