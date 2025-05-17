using DidPrismResolverClient;

namespace Blocktrust.CredentialWorkflow.Core.Services.DIDPrism
{
    using DidPrismResolverClient.Models;

    /// <summary>
    /// A wrapper around PrismDidClient that uses a fallback configuration
    /// with different baseURL and ledger settings.
    /// </summary>
    public class FallbackPrismDidClient
    {
        private readonly PrismDidClient _prismDidClient;

        public FallbackPrismDidClient(PrismDidClient prismDidClient)
        {
            _prismDidClient = prismDidClient;
        }

        /// <summary>
        /// Resolves a DID document using the fallback Prism node.
        /// </summary>
        /// <param name="did">The DID to resolve</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The resolved DID document</returns>
        public Task<DidDocument> ResolveDidDocumentAsync(string did, CancellationToken cancellationToken = default)
        {
            return _prismDidClient.ResolveDidDocumentAsync(did, cancellationToken: cancellationToken);
        }
    }
}