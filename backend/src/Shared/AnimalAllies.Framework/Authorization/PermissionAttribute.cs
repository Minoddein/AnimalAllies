using Microsoft.AspNetCore.Authorization;

namespace AnimalAllies.Framework.Authorization;

public sealed class PermissionAttribute(string code) : AuthorizeAttribute(code), IAuthorizationRequirement
{
    public string Code { get; } = code;
}