namespace Blocktrust.VerifiableCredential.Common.JwtModels;

public class JwtModel
{
    public JwtModel(Dictionary<string, object> headers, Dictionary<string, object> payload, string? signature, string? headersAsJson = null, string? payloadAsJson = null)
    {
        Headers = headers;
        HeadersAsJson = headersAsJson;
        Payload = payload;
        PayloadAsJson = payloadAsJson;
        Signature = signature;
    }

    public JwtModel(string headersAsJson, string payloadAsJson)
    {
        PayloadAsJson = payloadAsJson;
        HeadersAsJson = headersAsJson;
    }

    public Dictionary<string, object>? Headers { get; }
    public string? HeadersAsJson { get; }
    public Dictionary<string, object> Payload { get; }
    public string? PayloadAsJson { get; }
    public string? Signature { get; set; }
}