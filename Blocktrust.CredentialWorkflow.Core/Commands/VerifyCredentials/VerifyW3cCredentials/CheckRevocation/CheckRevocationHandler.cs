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

    private byte[] DecodeBase64Url(string input)
    {
        string padded = input.Length % 4 == 0
            ? input
            : input + "====".Substring(input.Length % 4);
        string converted = padded.Replace('_', '/').Replace('-', '+');
        return Convert.FromBase64String(converted);
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