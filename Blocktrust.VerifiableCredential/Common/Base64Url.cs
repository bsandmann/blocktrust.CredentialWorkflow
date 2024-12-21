namespace Blocktrust.VerifiableCredential.Common;

/// <summary>
/// https://github.com/dvsekhvalnov/jose-jwt/blob/master/jose-jwt/util/Base64Url.cs
/// </summary>
public class Base64Url
{
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

    /// <summary>
    /// Decodes the given base64url string into a byte array
    /// </summary>
    /// <param name="input">base64 string</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte[] Decode(string input)
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
}