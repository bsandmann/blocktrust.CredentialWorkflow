namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

public class ServiceEndpointConverter : JsonConverter<DidDocumentService>
{
    public override DidDocumentService Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected {JsonTokenType.StartObject} but found {reader.TokenType}.");
        }

        var didDocumentService = new DidDocumentService();

        // Read until we reach the end of this object.
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return didDocumentService;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                // Move the reader forward to the value token.
                reader.Read();

                switch (propertyName)
                {
                    case "id":
                        didDocumentService.Id = reader.GetString();
                        break;

                    case "type":
                        didDocumentService.Type = reader.GetString();
                        break;

                    case "serviceEndpoint":
                    {
                        // Examine the type of the next token and
                        // deserialize accordingly.
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.StartArray:
                                // Deserializes an array of strings.
                                didDocumentService.ServiceEndpointStringList =
                                    JsonSerializer.Deserialize<List<string>>(ref reader, options);
                                break;

                            case JsonTokenType.String:
                                // Deserializes a single string.
                                didDocumentService.ServiceEndpointString = reader.GetString();
                                break;

                            case JsonTokenType.StartObject:
                                // Deserializes into a dictionary or a custom object.
                                didDocumentService.ServiceEndpointObject =
                                    JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
                                break;

                            case JsonTokenType.Null:
                                // Service endpoint is null, so just move on.
                                break;

                            default:
                                throw new JsonException(
                                    $"Unexpected token {reader.TokenType} when reading 'serviceEndpoint'.");
                        }

                        break;
                    }
                    default:
                        // If you want to ignore extra properties, you could skip them here.
                        // Or you could throw an exception. This example will skip them.
                        reader.Skip();
                        break;
                }
            }
        }

        // If we exit the loop without returning, the JSON was malformed (no end object).
        throw new JsonException("Missing end object token in DidDocumentService.");
    }

    public override void Write(Utf8JsonWriter writer, DidDocumentService value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);

        if (!string.IsNullOrEmpty(value.Type))
        {
            writer.WriteString("type", value.Type);
        }

        writer.WritePropertyName("serviceEndpoint");
        if (value.ServiceEndpointStringList != null)
        {
            JsonSerializer.Serialize(writer, value.ServiceEndpointStringList, options);
        }
        else if (value.ServiceEndpointString != null)
        {
            writer.WriteStringValue(value.ServiceEndpointString);
        }
        else if (value.ServiceEndpointObject != null)
        {
            JsonSerializer.Serialize(writer, value.ServiceEndpointObject, options);
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WriteEndObject();
    }
}