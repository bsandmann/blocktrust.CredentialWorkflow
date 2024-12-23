namespace Blocktrust.VerifiableCredential.VC;

public static class DataModelTypeEvaluator
{
    public static EDataModelType Evaluate(VerifiableCredential credential)
    {
        if (credential.CredentialContext.Contexts is not null)
        {
            var contexts = credential.CredentialContext.Contexts.Select(p => p.ToString()).ToList();
            if (contexts.Contains("https://www.w3.org/2018/credentials/v1"))
            {
                if ((credential.IssuanceDate is not null || credential.ExpirationDate is not null) &&
                    (credential.ValidFrom is not null || credential.ValidUntil is not null))
                {
                    return EDataModelType.Invalid;
                }

                return EDataModelType.DataModel11;
            }
            else if (contexts.Contains("https://www.w3.org/ns/credentials/v2"))
            {
                if ((credential.IssuanceDate is not null || credential.ExpirationDate is not null) &&
                    (credential.ValidFrom is not null || credential.ValidUntil is not null))
                {
                    return EDataModelType.Invalid;
                }

                return EDataModelType.DataModel2;
            }
        }

        return EDataModelType.Undefined;
    }
    public static EDataModelType Evaluate(VerifiablePresentation presentation)
    {
        if (presentation.PresentationContext.Contexts is not null)
        {
            var contexts = presentation.PresentationContext.Contexts.Select(p => p.ToString()).ToList();
            if (contexts.Contains("https://www.w3.org/2018/credentials/v1"))
            {
                return EDataModelType.DataModel11;
            }
            else if (contexts.Contains("https://www.w3.org/ns/credentials/v2"))
            {
                return EDataModelType.DataModel2;
            }
        }

        return EDataModelType.Invalid;
    }
}