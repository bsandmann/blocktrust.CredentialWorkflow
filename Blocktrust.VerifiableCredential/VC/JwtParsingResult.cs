namespace Blocktrust.VerifiableCredential.VC;

public record ParsedJwt
{
    public ParsedJwt(List<VerifiableCredential> verifiableCredentials)
    {
        VerifiableCredentials = verifiableCredentials;
    }

    public ParsedJwt(List<VerifiablePresentation> verifiablePresentations)
    {
        VerifiablePresentations = verifiablePresentations;
    }

    public List<VerifiableCredential> VerifiableCredentials { get; } = new List<VerifiableCredential>();
    public List<VerifiablePresentation> VerifiablePresentations { get; } = new List<VerifiablePresentation>();
}