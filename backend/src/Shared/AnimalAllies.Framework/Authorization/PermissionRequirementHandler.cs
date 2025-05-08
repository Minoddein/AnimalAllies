using AnimalAllies.Framework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Framework.Authorization;

public class PermissionRequirementHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAttribute permission)
    {
        if (context.User.Identity is null || !context.User.Identity.IsAuthenticated
                                          || httpContextAccessor.HttpContext is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        UserScopedData userScopedData =
            httpContextAccessor.HttpContext.RequestServices.GetRequiredService<UserScopedData>();

        if (userScopedData.Permissions.Contains(permission.Code))
        {
            context.Succeed(permission);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}