namespace Blocktrust.CredentialWorkflow.Web.Services;

using Common;
using Core.Commands.Tenant.GetTenantInformation;
using Core.Domain.Tenant;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;

public class AppStateService
{
    public bool IsInitialized { get; set; }
    public string? UserName { get; set; }
    public Tenant Tenant { get; private set; } = new Tenant();

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void PropagateUpdate()
    {
        NotifyStateChanged();
    }

    /// <summary>
    /// Executes the initial request to get the tenant information and populates the "Session"
    /// </summary>
    /// <param name="authenticationStateProvider"></param>
    /// <param name="logger"></param>
    /// <param name="cts"></param>
    /// <param name="mediator"></param>
    public async Task Initialize(AuthenticationStateProvider authenticationStateProvider, ILogger logger, CancellationTokenSource cts, IMediator mediator)
    {
        var tenandGuid = await AuthenticationHelper.GetTenantIdAndUsername(authenticationStateProvider);
        if (tenandGuid.IsFailed)
        {
            logger.LogError("Failed to get tenant id and username");
            return;
        }

        var (tenandId, username) = tenandGuid.Value;
        var tenantInformationResult = await mediator.Send(new GetTenantInformationRequest(tenandId), cts.Token);
        if (tenantInformationResult.IsFailed)
        {
            logger.LogError("Failed to get tenant information for tenant id on initial request");
            return;
        }

        this.UserName = username;
        this.Tenant = tenantInformationResult.Value.Tenant;
        this.IsInitialized = true;
    }
}