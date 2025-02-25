 namespace Blocktrust.CredentialWorkflow.Core.Entities.Identity;

 using Microsoft.AspNetCore.Identity;
 using Tenant;

 public class ApplicationUser : IdentityUser
 {
     [PersonalData] public string? SomeOtherData { get; set; }

     public Guid? TenantEntityId { get; set; }
     public TenantEntity? TenantEntity { get; set; } = null!;
 }