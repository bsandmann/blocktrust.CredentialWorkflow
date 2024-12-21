namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcCredentialSubjectConverter : JsonConverter<List<CredentialSubject>>
{
    public override List<CredentialSubject>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var credentialSubject = ReadCredentialSubject(ref reader, options);
            return new List<CredentialSubject>() { credentialSubject };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var credentialSubjects = new List<CredentialSubject>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (credentialSubjects.Count == 1)
                    {
                        return new List<CredentialSubject>()
                        {
                            new CredentialSubject()
                            {
                                Id = credentialSubjects[0].Id,
                                AdditionalData = credentialSubjects[0].AdditionalData,
                                SerializationOption = new SerializationOption()
                                {
                                    UseArrayEvenForSingleElement = true
                                }
                            }
                        };
                    }

                    return credentialSubjects;
                }

                var credentialSubject = ReadCredentialSubject(ref reader, options);
                credentialSubjects.Add(credentialSubject);
            }
        }

        return null;
    }

    private static CredentialSubject ReadCredentialSubject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Uri? id = null;
        Dictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialSubject()
                {
                    Id = id,
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
            else if (val != null)
            {
                if (additionalData is null)
                {
                    additionalData = new Dictionary<string, object>();
                }

                additionalData.Add(propertyName, val);
            }
        }

        return new CredentialSubject();
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialSubject> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            if (value[0].SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                WriteCredentialSubject(writer, value[0]);
                writer.WriteEndArray();
            }
            else
            {
                WriteCredentialSubject(writer, value[0]);
            }
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var credentialSubject in value)
            {
                WriteCredentialSubject(writer, credentialSubject);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteCredentialSubject(Utf8JsonWriter writer, CredentialSubject credentialSubject)
    {
        if (credentialSubject.Id is not null && (credentialSubject.AdditionalData is null || credentialSubject.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            writer.WriteString("id", credentialSubject.Id.OriginalString);
            writer.WriteEndObject();
        }
        else if (credentialSubject.Id is not null && credentialSubject.AdditionalData is not null && credentialSubject.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the id is the first property
            // And serializing all in one go.
            var newTempDictionary = new Dictionary<string, object> { { "id", credentialSubject.Id.OriginalString } };
            foreach (var keyValuePair in credentialSubject.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialSubject.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialSubject.AdditionalData);
        }
    }
}