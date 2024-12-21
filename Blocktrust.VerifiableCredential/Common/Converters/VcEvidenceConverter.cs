namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcEvidenceConverter : JsonConverter<List<CredentialEvidence>>
{
    public override List<CredentialEvidence>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var evidence = ReadEvidence(ref reader, options);
            return new List<CredentialEvidence>() { evidence };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var evidences = new List<CredentialEvidence>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (evidences.Count == 1)
                    {
                        return new List<CredentialEvidence>()
                        {
                            new CredentialEvidence()
                            {
                                Id = evidences[0].Id,
                                Type = evidences[0].Type,
                                AdditionalData = evidences[0].AdditionalData,
                                SerializationOption = new SerializationOption()
                                {
                                    UseArrayEvenForSingleElement = true
                                }
                            }
                        };
                    }

                    return evidences;
                }

                var evidence = ReadEvidence(ref reader, options);
                evidences.Add(evidence);
            }
        }

        return null;
    }

    private static CredentialEvidence ReadEvidence(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Uri? id = null;
        IList<string> type = new List<string>();
        IDictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialEvidence()
                {
                    Id = id,
                    Type = type,
                    AdditionalData = additionalData
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
                    type = new List<string>() { (string)val };
                }
                else if (val is List<object>)
                {
                    type = ((List<object>)val).Where(p => p != null).Select(p => p.ToString()).ToList();
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

        return new CredentialEvidence() { Type = new List<string>() };
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialEvidence> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            if (value[0].SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                WriteEvidence(writer, value[0]);
                writer.WriteEndArray();
            }
            else
            {
                WriteEvidence(writer, value[0]);
            }
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var evidence in value)
            {
                WriteEvidence(writer, evidence);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteEvidence(Utf8JsonWriter writer, CredentialEvidence credentialEvidence)
    {
        if ((credentialEvidence.AdditionalData is null || credentialEvidence.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            if (credentialEvidence.Id is not null)
            {
                writer.WriteString("id", credentialEvidence.Id.OriginalString);
            }

            writer.WriteStartArray("type");
            foreach (var type in credentialEvidence.Type)
            {
                writer.WriteStringValue(type);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        else if (credentialEvidence.AdditionalData is not null && credentialEvidence.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the id is the first property
            Dictionary<string, object> newTempDictionary = new Dictionary<string, object>();
            if (credentialEvidence.Id is not null)
            {
                newTempDictionary.Add("id", credentialEvidence.Id.OriginalString);
            }

            newTempDictionary.Add("type", credentialEvidence.Type);

            foreach (var keyValuePair in credentialEvidence.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialEvidence.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialEvidence.AdditionalData);
        }
    }
}