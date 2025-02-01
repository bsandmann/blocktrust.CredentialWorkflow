namespace Blocktrust.CredentialWorkflow.Core.Domain.Common;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// Define an enum for the HTTP methods you expect to handle:

public class SimplifiedHttpContext
{
    // Convert enum to and from string values during JSON serialization
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpRequestMethod Method { get; set; }

    // Dictionary to hold all query parameters
    public Dictionary<string, string> QueryParameters { get; set; }

    // Raw request body content
    public string Body { get; set; }

    // Parameterless constructor for deserialization
    public SimplifiedHttpContext()
    {
        QueryParameters = new Dictionary<string, string>();
    }

    // Serialize this object to a JSON string
    public string ToJson()
    {
        // You can customize options here if needed
        var options = new JsonSerializerOptions
        {
            WriteIndented = false // for compact storage in DB
        };

        return JsonSerializer.Serialize(this, options);
    }

    // Deserialize from a JSON string back to a SimplifiedHttpContext object
    public static SimplifiedHttpContext FromJson(string json)
    {
        // Use the same options you used for serialization (if any)
        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        return JsonSerializer.Deserialize<SimplifiedHttpContext>(json, options);
    }
}