namespace Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;

using System.ComponentModel.DataAnnotations;
using Domain.PeerDID;
using Microsoft.EntityFrameworkCore;

public class PeerDIDEntity
{
    public Guid PeerDIDEntityId { get; set; }

    [Unicode(true)]
    [MaxLength(200)]
    public string Name { get; set; }

    [Unicode(true)]
    [MaxLength(5000)]
    public string PeerDID { get; set; }

    public Guid TenantEntityId { get; set; }
    public DateTime CreatedUtc { get; init; }

    // Map this entity to the domain model
    public PeerDIDModel ToModel()
    {
        return new PeerDIDModel
        {
            PeerDIDEntityId = PeerDIDEntityId,
            Name = Name,
            PeerDID = PeerDID,
            TenantEntityId = TenantEntityId,
            CreatedUtc = CreatedUtc
        };
    }
}