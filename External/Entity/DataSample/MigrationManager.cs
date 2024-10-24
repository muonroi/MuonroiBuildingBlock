namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication app)
            where TContext : MDbContext
        {
            using (IServiceScope scope = app.Services.CreateScope())
            {
                IPermissionSyncService permissionSyncService = scope.ServiceProvider.GetRequiredService<IPermissionSyncService>();

                permissionSyncService.SyncPermissionsAsync().Wait();

                TContext context = scope.ServiceProvider.GetRequiredService<TContext>();

                context.Database.Migrate();

                new InitialHostDbBuilder<TContext>(context).Create();


            }
            return app;
        }
    }
}