namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;

/// <summary>
/// Converts the type of <see cref="https://w3c.github.io/vc-data-model/#types"/> for Verifiable Credentials and Presentations.
/// </summary>
public class TypeConverter : JsonConverter<CredentialOrPresentationType>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(CredentialOrPresentationType);
    }


    public override CredentialOrPresentationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //The DID JSON-LD context starts either with a single string, array of strings, array of objects and strings
        //or is an object that can contain whatever elements.
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.String)
        {
            if (reader.ValueTextEquals("type"))
            {
                _ = reader.Read();
            }

            var readed = reader.GetString();
            if (!string.IsNullOrWhiteSpace(readed))
            {
                return new CredentialOrPresentationType()
                {
                    Type = new HashSet<string>() { readed }
                };
            }
        }
        else if (tokenType == JsonTokenType.StartArray)
        {
            var elements = new HashSet<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (elements.Count == 1)
                    {
                        return new CredentialOrPresentationType()
                        {
                            Type = elements,
                            SerializationOption = new SerializationOption() { UseArrayEvenForSingleElement = true }
                        };
                    }
                    else
                    {
                        return new CredentialOrPresentationType()
                        {
                            Type = elements
                        };
                    }
                }

                var typeName = reader.GetString();
                if (typeName is not null)
                {
                    elements.Add(typeName);
                }
            }
        }

        return new CredentialOrPresentationType() { Type = new HashSet<string>() };
    }


    public override void Write(Utf8JsonWriter writer, CredentialOrPresentationType value, JsonSerializerOptions options)
    {
        if (value.Type.Count == 1)
        {
            if (value.SerializationOption?.UseArrayEvenForSingleElement == true)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(value.Type.ElementAt(0));
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStringValue(value.Type.First());
            }
        }
        else
        {
            writer.WriteStartArray();
            for (int i = 0; i < value.Type.Count; ++i)
            {
                writer.WriteStringValue(value.Type.ElementAt(i));
            }

            writer.WriteEndArray();
        }
    }
}