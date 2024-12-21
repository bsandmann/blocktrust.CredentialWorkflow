namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter for <see cref="System.Text.Json"/>
/// </summary>
public sealed class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type? typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
        }

        var dic = new Dictionary<string, object>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dic;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("JsonTokenType was not PropertyName");
            }

            string? propertyName = reader.GetString();
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new JsonException("Failed to get property name");
            }

            _ = reader.Read();
            var extractedValue = ExtractValue(ref reader, options);
            if (extractedValue != null)
            {
                dic.Add(propertyName, extractedValue);
            }
        }

        return dic;
    }


    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        writer.WriteStartObject();

        foreach (var keyValuePair in value)
        {
            writer.WritePropertyName(keyValuePair.Key);
            Converter.WriteValue(writer, keyValuePair.Value, options);
        }

        writer.WriteEndObject();
    }


    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return Converter.ExtractValue(ref reader, options);
    }
}