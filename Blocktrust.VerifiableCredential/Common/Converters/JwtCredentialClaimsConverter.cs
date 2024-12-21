namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using JwtModels;

public class JwtCredentialClaimsConverter : JsonConverter<JwtCredentialClaims>
{
    public override void Write(Utf8JsonWriter writer, JwtCredentialClaims value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        WriteProperty(writer, "exp", value.Exp, options);
        WriteProperty(writer, "iss", value.Iss, options);
        WriteProperty(writer, "nbf", value.Nbf, options);
        WriteProperty(writer, "jti", value.Jti, options);
        WriteProperty(writer, "sub", value.Sub, options);
        WriteProperty(writer, "aud", value.Aud, options);

        if (value.VerifiableCredentials is not null)
        {
            WriteListOrObject(writer, "vc", value.VerifiableCredentials, options);
        }
        else if (value.VerifiablePresentations is not null)
        {
            if (value.Algorithm is not null && (value.BuildJwtWithoutSignature is not null || value.PrivateKey is not null))
            {
                value.VerifiablePresentations
                    .ForEach(p => p.VerifiableCredentials?
                        .ForEach(q => q.JwtParsingArtefact =
                            q.JwtParsingArtefact is null
                                ? new JwtParsingArtefact()
                                {
                                    JwtAlg = value.Algorithm,
                                    PrivateKeyForSigningJwtCredentialsInsidePresentations = value.PrivateKey,
                                    EmbeddedCredentialInPresentationAsJwt = true
                                }
                                : q.JwtParsingArtefact with
                                {
                                    JwtAlg = value.Algorithm,
                                    PrivateKeyForSigningJwtCredentialsInsidePresentations = value.PrivateKey,
                                    EmbeddedCredentialInPresentationAsJwt = true
                                }));
            }

            WriteListOrObject(writer, "vp", value.VerifiablePresentations, options);
        }

        if (value.AdditionalClaims != null)
        {
            foreach (var claim in value.AdditionalClaims)
            {
                WriteProperty(writer, claim.Key, claim.Value, options);
            }
        }

        writer.WriteEndObject();
    }

    private void WriteListOrObject<T>(Utf8JsonWriter writer, string propertyName, List<T>? items, JsonSerializerOptions options)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        writer.WritePropertyName(propertyName);

        if (items.Count == 1)
        {
            JsonSerializer.Serialize(writer, items[0], options);
        }
        else
        {
            JsonSerializer.Serialize(writer, items, options);
        }
    }

    private void WriteProperty<T>(Utf8JsonWriter writer, string propertyName, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            return;
        }

        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }

    public override JwtCredentialClaims Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}