namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;

public static class Converter
{
    public static object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var isDateTime = reader.TryGetDateTime(out DateTime date);
                if (isDateTime)
                {
                    if (date.Date.Hour.Equals(0) && date.Minute.Equals(0) && date.Second.Equals(0))
                    {
                        return reader.GetString();
                    }

                    return date;
                }

                return reader.GetString();
            }
            case JsonTokenType.False:
            {
                return false;
            }
            case JsonTokenType.True:
            {
                return true;
            }
            case JsonTokenType.Null:
            {
                return null;
            }
            case JsonTokenType.Number:
            {
                return reader.TryGetInt64(out long result) ? result : reader.GetDecimal();
            }
            case JsonTokenType.StartObject:
            {
                return new DictionaryStringObjectJsonConverter().Read(ref reader, null, options);
            }
            case JsonTokenType.StartArray:
            {
                var list = new List<object>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    var extractedValue = ExtractValue(ref reader, options);
                    if (extractedValue != null)
                    {
                        list.Add(extractedValue);
                    }
                }

                return list;
            }
            default:
            {
                throw new JsonException($"Token '{reader.TokenType}' is not supported");
            }
        }
    }

    public static void WriteValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else if (value is string stringValue)
        {
            writer.WriteStringValue(stringValue);
        }
        else if (value is bool boolValue)
        {
            writer.WriteBooleanValue(boolValue);
        }
        else if (value is int intValue)
        {
            writer.WriteNumberValue(intValue);
        }
        else if (value is long longValue)
        {
            writer.WriteNumberValue(longValue);
        }
        else if (value is float floatValue)
        {
            writer.WriteNumberValue(floatValue);
        }
        else if (value is double doubleValue)
        {
            writer.WriteNumberValue(doubleValue);
        }
        else if (value is decimal decimalValue)
        {
            writer.WriteNumberValue(decimalValue);
        }
        else if (value is DateTime dateTimeValue)
        {
            writer.WriteStringValue(dateTimeValue);
        }
        else if (value is Dictionary<string, object> dictionaryValue)
        {
            new DictionaryStringObjectJsonConverter().Write(writer, dictionaryValue, options);
        }
        else if (value is IList<object> listValue)
        {
            writer.WriteStartArray();

            foreach (var item in listValue)
            {
                WriteValue(writer, item, options);
            }

            writer.WriteEndArray();
        }
        else
        {
            throw new NotSupportedException($"Type '{value.GetType()}' is not supported.");
        }
    }
}