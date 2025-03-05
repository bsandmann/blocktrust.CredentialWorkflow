using System.Net;
using System.Net.Http;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    public MockHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
        string content = "{\"error\":\"Mocked unreachable call\"}")
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Always return an error response, simulating an unreachable or failing endpoint
        var responseMessage = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content)
        };
        return Task.FromResult(responseMessage);
    }
}