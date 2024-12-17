namespace Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey
{
    public class IssuingKey
    {
        public Guid IssuingKeyId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string KeyType { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}