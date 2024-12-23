namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

public class VcRefreshServiceConverter : JsonConverter<List<CredentialRefreshService>>
{
    public override List<CredentialRefreshService>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var refreshService = ReadRefreshService(ref reader, options);
            return new List<CredentialRefreshService>() { refreshService };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var refreshServices = new List<CredentialRefreshService>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return refreshServices;
                }

                var refreshService = ReadRefreshService(ref reader, options);
                refreshServices.Add(refreshService);
            }
        }

        return null;
    }

    private static CredentialRefreshService ReadRefreshService(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Uri id = null!;
        string type = string.Empty;
        IDictionary<string, object>? additionalData = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CredentialRefreshService()
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

        return new CredentialRefreshService() { Id = new Uri(String.Empty), Type = String.Empty };
    }

    public override void Write(Utf8JsonWriter writer, List<CredentialRefreshService> value, JsonSerializerOptions options)
    {
        if (value.Count == 1)
        {
            WriteRefreshService(writer, value[0]);
        }
        else if (value.Count > 1)
        {
            writer.WriteStartArray();
            foreach (var refreshService in value)
            {
                WriteRefreshService(writer, refreshService);
            }

            writer.WriteEndArray();
        }
    }

    private static void WriteRefreshService(Utf8JsonWriter writer, CredentialRefreshService credentialRefreshService)
    {
        if ((credentialRefreshService.AdditionalData is null || credentialRefreshService.AdditionalData.Count == 0))
        {
            writer.WriteStartObject();
            writer.WriteString("id", credentialRefreshService.Id.OriginalString);
            writer.WriteString("type", credentialRefreshService.Type);
            writer.WriteEndObject();
        }
        else if (credentialRefreshService.AdditionalData is not null && credentialRefreshService.AdditionalData.Count > 0)
        {
            // Reordering the dictionary to make sure the id is the first property
            // And serializing all in one go.
            var newTempDictionary = new Dictionary<string, object>
            {
                { "id", credentialRefreshService.Id.OriginalString },
                { "type", credentialRefreshService.Type }
            };
            foreach (var keyValuePair in credentialRefreshService.AdditionalData)
            {
                newTempDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }

            JsonSerializer.Serialize(writer, newTempDictionary);
        }
        else if (credentialRefreshService.AdditionalData?.Count > 0)
        {
            JsonSerializer.Serialize(writer, credentialRefreshService.AdditionalData);
        }
    }
}