namespace Blocktrust.VerifiableCredential.Common;

public record LanguageModel
{
    /// <summary>
    /// The actual name e.g."Université de Exemple",
    /// </summary>
    public required string Value { get; set; }
    
    /// <summary>
    ///  eg. "rtl"
    /// </summary>
    public string? Direction { get; set; }
}