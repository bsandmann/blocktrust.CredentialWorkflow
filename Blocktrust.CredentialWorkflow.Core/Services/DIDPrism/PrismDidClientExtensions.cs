using DidPrismResolverClient;

namespace Blocktrust.CredentialWorkflow.Core.Services.DIDPrism
{
    /// <summary>
    /// Extension methods for PrismDidClient
    /// </summary>
    public static class PrismDidClientExtensions
    {
        private static readonly System.Reflection.FieldInfo _optionsField;

        static PrismDidClientExtensions()
        {
            // Use reflection to access the private options field
            _optionsField = typeof(PrismDidClient).GetField("_options", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
        }

        /// <summary>
        /// Gets the options used by the PrismDidClient
        /// </summary>
        /// <param name="client">The client instance</param>
        /// <returns>The client options</returns>
        public static PrismDidClientOptions GetOptions(this PrismDidClient client)
        {
            if (_optionsField == null)
            {
                return new PrismDidClientOptions(); // Fallback if reflection fails
            }

            return _optionsField.GetValue(client) as PrismDidClientOptions;
        }
    }
}