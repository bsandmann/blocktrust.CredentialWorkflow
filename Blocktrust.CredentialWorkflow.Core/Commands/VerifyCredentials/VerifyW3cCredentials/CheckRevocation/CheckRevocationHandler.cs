using System.IO.Compression;
using System.Text.Json;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckRevocation;

public class CheckRevocationHandler : IRequestHandler<CheckRevocationRequest, Result<bool>>
{
    private readonly HttpClient _httpClient;

    public CheckRevocationHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<bool>> Handle(CheckRevocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Credential.CredentialStatus == null)
            {
                return Result.Ok(false); // No revocation status to check
            }

            var statusListCredential = await FetchStatusListCredential(
                request.Credential.CredentialStatus.StatusListCredential,
                cancellationToken);

            var isRevoked = CheckRevocationStatus(statusListCredential, 
                request.Credential.CredentialStatus.StatusListIndex);

            return Result.Ok(isRevoked);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Failed to check revocation status").CausedBy(ex));
        }
    }

    private async Task<StatusList2021Credential> FetchStatusListCredential(string url, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<StatusList2021Credential>(response, options);
        }
        catch (Exception e)
        {
            throw new Exception("Error during fetching status list credential", e);
        }
    }

    private bool CheckRevocationStatus(StatusList2021Credential statusListCredential, int? statusListIndex)
    {
        var decodedList = DecodeBase64Url(statusListCredential.CredentialSubject.EncodedList);
        var decompressedList = Decompress(decodedList);

        int byteIndex = (int)(statusListIndex / 8)!;
        int bitIndex = (int)(statusListIndex % 8)!;

        return (decompressedList[byteIndex] & (1 << (7 - bitIndex))) != 0;
    }

    /// <summary>
    /// Decodes the given base64url string into a byte array
    /// </summary>
    /// <param name="input">base64 string</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte[] DecodeBase64Url(string input)
    {
        var output = input;
        output = output.Replace('-', '+'); // 62nd char of encoding
        output = output.Replace('_', '/'); // 63rd char of encoding
        switch (output.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2: output += "=="; break; // Two pad chars
            case 3: output += "="; break; // One pad char
            default: throw new System.ArgumentOutOfRangeException(nameof(input), "Illegal base64url string!");
        }
        var converted = Convert.FromBase64String(output); // Standard base64 decoder
        return converted;
    }

    /// <summary>
    /// Encodes the given byte array into a base64url string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Encode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0]; // Remove any trailing '='s
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }
    
    private byte[] Decompress(byte[] compressedData)
    {
        try
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            decompressStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        catch (InvalidDataException)
        {
            return compressedData;
        }
    }
}

public class StatusList2021Credential
{
    public CredentialSubject CredentialSubject { get; set; }
}

public class CredentialSubject
{
    public string EncodedList { get; set; }
}