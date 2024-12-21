namespace Blocktrust.VerifiableCredential.Common.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.VC;
using JwtModels;

public class VerifiableCredentialListConverter : JsonConverter<List<VerifiableCredential>>
{
    public override List<VerifiableCredential> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var credentials = new List<VerifiableCredential>();
        var newOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = options.DefaultIgnoreCondition,
            Converters =
            {
                // new VerifiableCredentialConverter()
            }
        };
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var credential = JsonSerializer.Deserialize<VerifiableCredential>(ref reader, newOptions);
            if (credential is not null)
            {
                credential.DataModelType = DataModelTypeEvaluator.Evaluate(credential);
                credentials.Add(credential);
            }
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (credentials.Count == 1)
                    {
                        credentials[0] = credentials[0] with { SerializationOption = new SerializationOption() { UseArrayEvenForSingleElement = true } };
                        credentials[0].DataModelType = DataModelTypeEvaluator.Evaluate(credentials[0]);
                    }

                    return credentials;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var credential = JsonSerializer.Deserialize<VerifiableCredential>(ref reader, options);
                    if (credential is not null)
                    {
                        credential.DataModelType = DataModelTypeEvaluator.Evaluate(credential);
                        credentials.Add(credential);
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    // We assume the inner VerifiableCredential is a JWT 
                    var base64EncodedCredential = reader.GetString();
                    if (base64EncodedCredential is not null)
                    {
                        var result = JwtParser.Parse(base64EncodedCredential);
                        if (result.IsSuccess)
                        {
                            result.Value.VerifiableCredentials.ForEach(p => p.JwtParsingArtefact =
                                p.JwtParsingArtefact is null
                                    ? new JwtParsingArtefact()
                                    {
                                        EmbeddedCredentialInPresentationAsJwt = true
                                    }
                                    : p.JwtParsingArtefact with { EmbeddedCredentialInPresentationAsJwt = true });
                            credentials.AddRange(result.Value.VerifiableCredentials);
                        }
                    }
                }
            }
        }

        return credentials;
    }

    public override void Write(Utf8JsonWriter writer, List<VerifiableCredential> value, JsonSerializerOptions options)
    {
        //Check if we need to encode as JWT
        bool encodeAsJwt = value.Any(v => v.JwtParsingArtefact?.EmbeddedCredentialInPresentationAsJwt == true);

        if (encodeAsJwt)
        {
            List<string> encodedCredentials = new List<string>();
            foreach (var credential in value)
            {
                if (credential.JwtParsingArtefact?.JwtSignature is not null)
                {
                    // We use an already exisitng JWT signature
                    var jwtBuilderOptions = new JwtBuilderOptions();
                    var jwtModelResult = JwtBuilder.Build(new List<VerifiableCredential>() { credential }, credential.JwtParsingArtefact?.JwtAlg, credential.JwtParsingArtefact?.JwtVerificationKeyId, jwtBuilderOptions, credential.JwtParsingArtefact?.JwtAdditionalClaims?.ToDictionary());
                    var signedCredential = JwtBuilder.SignJwt(jwtModelResult.Value, credential.JwtParsingArtefact!.JwtSignature);
                    var jwt = JwtBuilder.EncodeAsJwt(signedCredential);
                    encodedCredentials.Add(jwt);
                }
                else if (credential.JwtParsingArtefact?.PrivateKeyForSigningJwtCredentialsInsidePresentations is not null && credential.JwtParsingArtefact?.JwtAlg is not null && credential.JwtParsingArtefact?.JwtAlg != "none")
                {
                    // we create a new signature
                    if (credential.JwtParsingArtefact?.PrivateKeyForSigningJwtCredentialsInsidePresentations == "DUMMY_KEY_FOR_SIGNING_EMBEDDED_CREDENTIALS")
                    {
                        //FOR TESTING PURPOSES
                        var jwtBuilderOptions = new JwtBuilderOptions();
                        var jwtModelResult = JwtBuilder.Build(new List<VerifiableCredential>() { credential }, credential.JwtParsingArtefact?.JwtAlg, credential.JwtParsingArtefact?.JwtVerificationKeyId, jwtBuilderOptions, credential.JwtParsingArtefact?.JwtAdditionalClaims?.ToDictionary());
                        var signedCredential = JwtBuilder.SignJwt(jwtModelResult.Value, "SOME_DUMMY_SIGNATURE");
                        var jwt = JwtBuilder.EncodeAsJwt(signedCredential);
                        encodedCredentials.Add(jwt);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (credential.JwtParsingArtefact?.JwtAlg == "none")
                {
                    var jwtBuilderOptions = new JwtBuilderOptions()
                    {
                        BuildJwtWithoutSignature = true
                    };
                    var jwtModelResult = JwtBuilder.Build(new List<VerifiableCredential>() { credential }, credential.JwtParsingArtefact?.JwtAlg, credential.JwtParsingArtefact?.JwtVerificationKeyId, jwtBuilderOptions, credential.JwtParsingArtefact?.JwtAdditionalClaims?.ToDictionary());
                    var jwt = JwtBuilder.EncodeAsJwt(jwtModelResult.Value);
                    encodedCredentials.Add(jwt);
                }
                else
                {
                    throw new Exception("Invalid options data");
                }
            }

            // Write as a JSON array of strings
            JsonSerializer.Serialize(writer, encodedCredentials, options);
        }
        else
        {
            if (value.Count == 1 && (value[0].SerializationOption is null || value[0].SerializationOption?.UseArrayEvenForSingleElement != true))
            {
                // Write a single object
                JsonSerializer.Serialize(writer, value[0], options);
            }
            else
            {
                // Write as an array of objects
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}