namespace Blocktrust.CredentialWorkflow.Web.Common;

using FluentResults;
using Microsoft.AspNetCore.Components.Authorization;

public static class AuthenticationHelper
{
    /// <summary>
    /// Return the tenantId and the username
    /// </summary>
    /// <param name="authenticationStateProvider"></param>
    /// <returns></returns>
    public static async Task<Result<(Guid, string?)>> GetTenantIdAndUsername(AuthenticationStateProvider authenticationStateProvider)
    {
        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var userClaims = authenticationState.User.Claims;
        var tenantId = authenticationState.User.FindFirst("tenantId")?.Value;
        if (tenantId is null)
        {
            return Result.Fail("Could not find tenantId in the claims");
        }

        var isParsed = Guid.TryParse(tenantId, out Guid tenantGuid);
        if (!isParsed)
        {
            return Result.Fail("The tenantId could not be parsed from the claims");
        }

        return Result.Ok((tenandId: tenantGuid, username: authenticationState.User.Identity?.Name));
    }
}