namespace Muonroi.BuildingBlock.External.Common.Enums
{
    public enum SystemEnum
    {
        /// <summary>
        /// An unhandled exception occurred while processing the request.
        /// </summary>
        UnhandledException = 0,

        /// <summary>
        /// Username or password is incorrect.
        /// </summary>
        InvalidCredentials = 1,

        /// <summary>
        /// Permission not found for this user.
        /// </summary>
        PermissionNotFound = 2,

        /// <summary>
        /// Role already exists.
        /// </summary>
        RoleAlreadyExists = 3,

        /// <summary>
        /// Role not found.
        /// </summary>
        RoleNotFound = 4,

        /// <summary>
        /// User not found.
        /// </summary>
        UserNotFound = 5,

        /// <summary>
        /// User has no permissions.
        /// </summary>
        UserHasNoPermissions = 6
    }
}
