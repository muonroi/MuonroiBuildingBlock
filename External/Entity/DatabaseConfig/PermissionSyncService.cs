namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class PermissionSyncService<TDbContext>(TDbContext context) : IPermissionSyncService
        where TDbContext : MDbContext
    {
        private readonly TDbContext _context = context;

        public async Task SyncPermissionsAsync()
        {
            Assembly assembly = typeof(TDbContext).Assembly;

            List<MPermission> permissionsFromEnum = assembly.GetTypes()
                .Where(t => t.IsEnum && t.Name.Contains("Permission"))
                .SelectMany(enumType => Enum.GetValues(enumType).Cast<Enum>())
                .Where(e => Convert.ToInt64(e) != 0)
                .Select(e => new MPermission
                {
                    Name = e.ToString(),
                    IsGranted = true,
                    Discriminator = "Permission"
                })
                .ToList();

            List<string> existingPermissions = await _context.Permissions
                .Select(p => p.Name)
                .ToListAsync();

            List<MPermission> newPermissions = permissionsFromEnum
                .Where(p => !existingPermissions.Contains(p.Name))
                .ToList();

            if (newPermissions.Count > 0)
            {
                await _context.Permissions.AddRangeAsync(newPermissions);
                _ = await _context.SaveChangesAsync();
            }
        }
    }
}
