using Microsoft.AspNetCore.Authorization;
using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Api.Attributtes;

/// <summary>
/// Represents a has permission attribute
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params EUserRole[] roles)
    {
        var loweredRoles = roles.Select(o => o.ToString().ToString()).ToArray();
        Roles = string.Join(", ", loweredRoles);
    } 
}

