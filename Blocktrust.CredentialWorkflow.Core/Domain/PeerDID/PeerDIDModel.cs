namespace Blocktrust.CredentialWorkflow.Core.Domain.PeerDID
{
    public class PeerDIDModel
    {
        public Guid PeerDIDEntityId { get; set; }
        public string Name { get; set; }
        public string PeerDID { get; set; }
        public Guid TenantEntityId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}