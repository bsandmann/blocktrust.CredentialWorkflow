namespace Blocktrust.CredentialWorkflow.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentResults;

    /// <summary>
    /// A thin, opinionated client‑side wrapper around the three <c>/registrar</c> endpoints exposed by OpenPrismNode.
    /// 
    /// * Inject <see cref="IHttpClientFactory"/> – the client is completely DI‑friendly.
    /// * Performs the **same minimal, synchronous validation** that the server does so that invalid requests fail fast.
    /// * All public members return <c>Result&lt;T&gt;</c> from FluentResults so that success / failure can be pattern‑matched easily.
    /// * Serialization uses System.Text.Json and matches the server’s <em>camelCase</em> contract.
    /// </summary>
    public sealed class OpenPrismNodeRegistrarClient
    {
        private static readonly HashSet<string> AllowedPurposes = new(StringComparer.Ordinal)
        {
            "authentication",
            "assertionMethod",
            "keyAgreement",
            "capabilityInvocation",
            "capabilityDelegation"
        };

        private static readonly HashSet<string> AllowedCurves = new(StringComparer.Ordinal)
        {
            "secp256k1",
            "Ed25519",
            "X25519"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public OpenPrismNodeRegistrarClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jsonOptions = new(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<Result<RegistrarResponseDto>> CreateDidAsync(
            string baseUrl,
            string walletId,
            List<Dictionary<string, string>> verificationMethods,
            List<Dictionary<string, string>> services,
            CancellationToken cancellationToken = default)
        {
            // Synchronous validation – mirrors server‑side rules (trimmed to essentials).
            var validationFailure = ValidateCreateParameters(baseUrl, walletId, verificationMethods, services);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = BuildCreateRequestBody(walletId, verificationMethods, services);
            var url = Combine(baseUrl, "/api/v1/registrar/create");

            return await SendAsync(url, body, cancellationToken);
        }

        public async Task<Result<RegistrarResponseDto>> GetCreateResultAsync(
            string baseUrl,
            string jobId,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateJobId(jobId);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = new { jobId };
            var url = Combine(baseUrl, "/api/v1/registrar/create");

            return await SendAsync(url, body, cancellationToken);
        }

        public async Task<Result<RegistrarResponseDto>> UpdateDidAsync(
            string baseUrl,
            string did,
            string walletId,
            List<string> didDocumentOperations,
            List<Dictionary<string, object>> didDocuments,
            List<Dictionary<string, string>>? verificationMethods = null,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateUpdateParameters(baseUrl, did, walletId, didDocumentOperations, didDocuments);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            object? secretSection = null;
            if (verificationMethods?.Any() == true)
            {
                secretSection = new
                {
                    verificationMethod = verificationMethods.Select(vm => new
                    {
                        id = vm["keyId"],
                        type = "JsonWebKey2020",
                        purpose = new[] { vm["purpose"] },
                        curve = vm["curve"]
                    })
                };
            }

            var body = new
            {
                options = new { walletId, storeSecrets = true, returnSecrets = true },
                secret = secretSection,
                didDocumentOperation = didDocumentOperations,
                didDocument = didDocuments,
            };

            var url = Combine(baseUrl, $"/api/v1/registrar/update/{Uri.EscapeDataString(did)}");

            return await SendAsync(url, body, cancellationToken);
        }
        
        public async Task<Result<RegistrarResponseDto>> UpdateDidAsync(
            string baseUrl,
            string walletId,
            List<Dictionary<string, object>> updateOperations,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return Result.Fail("BaseUrl must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(walletId))
            {
                return Result.Fail("WalletId must not be empty.");
            }

            if (updateOperations == null || !updateOperations.Any())
            {
                return Result.Fail("At least one update operation must be supplied.");
            }
            
            var body = new
            {
                options = new { walletId, storeSecrets = true, returnSecrets = true },
                updateOperations
            };
            
            var url = Combine(baseUrl, "/api/v1/registrar/update");

            return await SendAsync(url, body, cancellationToken);
        }
        
        public async Task<Result<RegistrarResponseDto>> UpdateDidAsync(
            string baseUrl,
            string did,
            string walletId,
            List<Dictionary<string, object>> updateOperations,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return Result.Fail("BaseUrl must not be empty.");
            }
            
            if (string.IsNullOrWhiteSpace(did))
            {
                return Result.Fail("DID must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(walletId))
            {
                return Result.Fail("WalletId must not be empty.");
            }

            if (updateOperations == null || !updateOperations.Any())
            {
                return Result.Fail("At least one update operation must be supplied.");
            }
            
            var body = new
            {
                options = new { walletId, storeSecrets = true, returnSecrets = true },
                updateOperations
            };
            
            var url = Combine(baseUrl, $"/api/v1/registrar/update/{Uri.EscapeDataString(did)}");

            return await SendAsync(url, body, cancellationToken);
        }

        public async Task<Result<RegistrarResponseDto>> GetUpdateResultAsync(
            string baseUrl,
            string did,
            string jobId,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateJobId(jobId);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = new { jobId };
            var url = Combine(baseUrl, $"/api/v1/registrar/update/{Uri.EscapeDataString(did)}");

            return await SendAsync(url, body, cancellationToken);
        }
        
        public async Task<Result<RegistrarResponseDto>> GetUpdateResultAsync(
            string baseUrl,
            string jobId,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateJobId(jobId);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = new { jobId };
            var url = Combine(baseUrl, "/api/v1/registrar/update");

            return await SendAsync(url, body, cancellationToken);
        }


        public async Task<Result<RegistrarResponseDto>> DeactivateDidAsync(
            string baseUrl,
            string did,
            string walletId,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateDeactivateParameters(baseUrl, did, walletId);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = new { options = new { walletId } };
            var url = Combine(baseUrl, $"/api/v1/registrar/deactivate/{Uri.EscapeDataString(did)}");

            return await SendAsync(url, body, cancellationToken);
        }

        public async Task<Result<RegistrarResponseDto>> GetDeactivateResultAsync(
            string baseUrl,
            string did,
            string jobId,
            CancellationToken cancellationToken = default)
        {
            var validationFailure = ValidateJobId(jobId);
            if (validationFailure is not null)
            {
                return Result.Fail(validationFailure);
            }

            var body = new { jobId };
            var url = Combine(baseUrl, $"/api/v1/registrar/deactivate/{Uri.EscapeDataString(did)}");

            return await SendAsync(url, body, cancellationToken);
        }


        private async Task<Result<RegistrarResponseDto>> SendAsync(string url, object body, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient("OpenPrismNode");
            using var response = await client.PostAsJsonAsync(url, body, _jsonOptions, ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return Result.Fail($"Server returned {(int)response.StatusCode} – {response.ReasonPhrase}. Body: {errorText}");
            }

            try
            {
                var dto = await response.Content.ReadFromJsonAsync<RegistrarResponseDto>(_jsonOptions, ct).ConfigureAwait(false);
                return dto is null
                    ? Result.Fail("Empty response body.")
                    : Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to parse response JSON. {ex.Message}");
            }
        }

        private static string? ValidateCreateParameters(
            string baseUrl,
            string walletId,
            List<Dictionary<string, string>> verificationMethods,
            List<Dictionary<string, string>> services)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "BaseUrl must not be empty.";
            }

            if (string.IsNullOrWhiteSpace(walletId))
            {
                return "WalletId must not be empty.";
            }

            if (verificationMethods is null || verificationMethods.Count == 0)
            {
                return "At least one verification method must be supplied.";
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var vm in verificationMethods)
            {
                bool TryGet(string key, out string value)
                {
                    value = vm.TryGetValue(key, out var v) ? v : string.Empty;
                    return !string.IsNullOrWhiteSpace(value);
                }

                if (!TryGet("keyId", out var keyId))
                {
                    return "Each verification method needs a keyId.";
                }

                if (keyId.Contains('#'))
                {
                    return "keyId must not contain '#'";
                }

                if (keyId.Contains("did:"))
                {
                    return "keyId must not contain 'did:'";
                }

                if (keyId.StartsWith("master", StringComparison.OrdinalIgnoreCase))
                {
                    return "keyId must not start with 'master'.";
                }

                if (!ids.Add(keyId))
                {
                    return $"Duplicate keyId '{keyId}'.";
                }

                if (!TryGet("purpose", out var purpose))
                {
                    return $"Verification method '{keyId}' needs a purpose.";
                }

                if (!AllowedPurposes.Contains(purpose))
                {
                    return $"Invalid purpose '{purpose}' for '{keyId}'.";
                }

                if (!TryGet("curve", out var curve))
                {
                    return $"Verification method '{keyId}' needs a curve.";
                }

                if (!AllowedCurves.Contains(curve))
                {
                    return $"Invalid curve '{curve}' for '{keyId}'.";
                }
            }

            if (services is not null)
            {
                var serviceIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var svc in services)
                {
                    bool TryGet(string key, out string value)
                    {
                        value = svc.TryGetValue(key, out var v) ? v : string.Empty;
                        return !string.IsNullOrWhiteSpace(value);
                    }

                    if (!TryGet("serviceId", out var sid))
                    {
                        return "Each service needs a serviceId.";
                    }

                    if (!serviceIds.Add(sid))
                    {
                        return $"Duplicate service id '{sid}'.";
                    }

                    if (!TryGet("type", out _))
                    {
                        return $"Service '{sid}' needs a type.";
                    }

                    if (!TryGet("endpoint", out _))
                    {
                        return $"Service '{sid}' needs an endpoint.";
                    }
                }
            }

            return null; // all good
        }

        private static string? ValidateUpdateParameters(
            string baseUrl,
            string did,
            string walletId,
            List<string> operations,
            List<Dictionary<string, object>> documents)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "BaseUrl must not be empty.";
            }

            if (string.IsNullOrWhiteSpace(did) || !did.StartsWith("did:prism:"))
            {
                return "Did must be provided in format 'did:prism:…'.";
            }

            if (string.IsNullOrWhiteSpace(walletId))
            {
                return "WalletId must not be empty.";
            }

            if (operations is null || operations.Count == 0)
            {
                return "At least one didDocumentOperation must be supplied.";
            }

            if (documents is null || documents.Count != operations.Count)
            {
                return "didDocumentOperation and didDocument arrays must have the same length.";
            }

            return null;
        }

        private static string? ValidateDeactivateParameters(string baseUrl, string did, string walletId)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "BaseUrl must not be empty.";
            }

            if (string.IsNullOrWhiteSpace(did) || !did.StartsWith("did:prism:"))
            {
                return "Did must be provided in format 'did:prism:…'.";
            }

            if (string.IsNullOrWhiteSpace(walletId))
            {
                return "WalletId must not be empty.";
            }

            return null;
        }

        private static string? ValidateJobId(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return "JobId must not be empty.";
            }

            return jobId.All(IsHex) && jobId.Length % 2 == 0 ? null : "JobId must be a valid hex string.";
        }

        private static bool IsHex(char c) => c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');

        private static string Combine(string baseUrl, string relative)
        {
            baseUrl = baseUrl.TrimEnd('/');
            return baseUrl + relative;
        }

        private static object BuildCreateRequestBody(
            string walletId,
            List<Dictionary<string, string>> verificationMethods,
            List<Dictionary<string, string>> services)
        {
            const string DefaultContext1 = "https://www.w3.org/ns/did/v1";
            const string DefaultContext2 = "https://w3id.org/security/suites/jws-2020/v1";


            var secretSection = new
            {
                verificationMethod = verificationMethods.Select(vm => new
                {
                    id = vm["keyId"],
                    type = "JsonWebKey2020",
                    purpose = new[] { vm["purpose"] },
                    curve = vm["curve"]
                })
            };

            var didDocSection = new Dictionary<string, object>
            {
                ["@context"] = new[] { DefaultContext1, DefaultContext2 },
                ["service"] = services.Select(s => new Dictionary<string, object>
                {
                    ["id"] = s["serviceId"],
                    ["type"] = s["type"],
                    ["serviceEndpoint"] = s["endpoint"]
                }).ToList<object>()
            };

            return new
            {
                method = "prism",
                options = new { walletId, storeSecrets = true, returnSecrets = true },
                secret = secretSection,
                didDocument = didDocSection
            };
        }
    }
}