namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

/// <summary>
/// Converts <see cref="CredentialOrPresentationContext" />
/// Based on DictionaryTKeyEnumTValueConverter
/// at https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to.
/// https://w3c.github.io/did-imp-guide/
/// </summary>
public class ContextConverter : JsonConverter<CredentialOrPresentationContext>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(CredentialOrPresentationContext);
    }


    public override CredentialOrPresentationContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //The DID JSON-LD context starts either with a single string, array of strings, array of objects and strings
        //or is an object that can contain whatever elements.
        var tokenType = reader.TokenType;
        if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("@context"))
        {
            _ = reader.Read();
        }

        if (tokenType == JsonTokenType.String)
        {
            if (reader.ValueTextEquals("@context"))
            {
                _ = reader.Read();
            }

            var ctx = reader.GetString();
            var contexts = new List<object>();
            if (ctx != null)
            {
                contexts.Add(ctx);
            }

            return new CredentialOrPresentationContext() { Contexts = contexts };
        }
        else if (tokenType == JsonTokenType.StartArray)
        {
            var contexts = new List<object>();
            var additionalData = new Dictionary<string, object>();
            var objectList = JsonSerializer.Deserialize<object[]>(ref reader);
            if (objectList != null)
            {
                foreach (var obj in objectList)
                {
                    if (obj is JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            contexts.Add(jsonElement.GetString() ?? string.Empty);
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var property in jsonElement.EnumerateObject())
                            {
                                additionalData[property.Name] = property.Value;
                            }
                        }
                        else
                        {
                            // Is is wrong, but should work for now
                            contexts.Add(obj);
                        }
                    }
                    else
                    {
                        contexts.Add(obj);
                    }
                }
            }

            var context = new CredentialOrPresentationContext() { Contexts = contexts };
            if(additionalData.Count > 0)
            {
                context = context with { AdditionalData = additionalData };
            }

            if (contexts.Count == 1)
            {
                
                context = context with { SerializationOption = new SerializationOption() { UseArrayEvenForSingleElement = true }};
            }

            return context;
        }
        else if (tokenType == JsonTokenType.StartObject)
        {
            while (reader.Read())
            {
                Dictionary<string, object>? additionalData = null;
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new CredentialOrPresentationContext()
                    {
                        AdditionalData = additionalData
                    };
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"JsonTokenType was not {nameof(JsonTokenType.PropertyName)}");
                }

                var propertyName = reader.GetString();
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                _ = reader.Read();
                object? val = ExtractValue(ref reader, options);
                if (val != null)
                {
                    if (additionalData is null)
                    {
                        additionalData = new Dictionary<string, object>();
                    }

                    additionalData.Add(propertyName, val);
                }
            }
        }

        return new CredentialOrPresentationContext();
    }


    public override void Write(Utf8JsonWriter writer, CredentialOrPresentationContext value, JsonSerializerOptions options)
    {
        if (value.Contexts?.Count == 1)
        {
            if (value.SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(value.Contexts.ElementAt(0).ToString());

                if (value.AdditionalData?.Count > 0)
                {
                    JsonSerializer.Serialize(writer, value.AdditionalData);
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStringValue(value.Contexts.ElementAt(0).ToString());
            }
        }
        else if (value.Contexts?.Count > 1)
        {
            writer.WriteStartArray();
            for (int i = 0; i < value.Contexts.Count; ++i)
            {
                if (value.Contexts.ElementAt(i) is string)
                {
                    writer.WriteStringValue(value.Contexts.ElementAt(i).ToString());
                }
                else
                {
                    JsonSerializer.Serialize(writer, value.Contexts.ElementAt(i));
                }
            }

            if (value.AdditionalData?.Count > 0)
            {
                JsonSerializer.Serialize(writer, value.AdditionalData);
            }

            writer.WriteEndArray();
        }
        else if (value.Contexts?.Count == 0)
        {
            if (value.AdditionalData?.Count > 0)
            {
                JsonSerializer.Serialize(writer, value.AdditionalData);
            }
        }
    }

    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return Converter.ExtractValue(ref reader, options);
    }
}