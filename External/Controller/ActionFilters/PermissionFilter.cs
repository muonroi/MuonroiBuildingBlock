namespace Muonroi.BuildingBlock.External.Controller.ActionFilters
{
    public class PermissionFilter<TPermission> : IAsyncActionFilter where TPermission : Enum
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Endpoint? endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                _ = await next();
                return;
            }

            IReadOnlyList<PermissionAttribute<TPermission>>? permissionAttributes = endpoint?.Metadata.GetOrderedMetadata<PermissionAttribute<TPermission>>();

            if (permissionAttributes == null || !permissionAttributes.Any())
            {
                _ = await next();
                return;
            }

            long? userPermissionsBitmask = GetPermissionsFromToken(context.HttpContext.User);

            if (userPermissionsBitmask == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            foreach (PermissionAttribute<TPermission> attribute in permissionAttributes)
            {
                long requiredPermission = Convert.ToInt64(attribute.RequiredPermission);

                if (!HasPermission(userPermissionsBitmask.Value, requiredPermission))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            _ = await next();
        }

        private static long? GetPermissionsFromToken(ClaimsPrincipal userClaims)
        {
            string? permissionClaim = userClaims.FindFirst(ClaimConstants.Permission)?.Value;

            return long.TryParse(permissionClaim, out long permissionsBitmask) ? permissionsBitmask : null;
        }

        private static bool HasPermission(long userPermissions, long requiredPermission)
        {
            return (userPermissions & requiredPermission) == requiredPermission;
        }
    }
}
