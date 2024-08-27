namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication app)
            where TContext : MDbContext, new()
        {
            using (IServiceScope scope = app.Services.CreateScope())
            {
                TContext context = scope.ServiceProvider.GetRequiredService<TContext>();
                context.Database.Migrate();

                new InitialHostDbBuilder<TContext>(context).Create();
            }
            return app;
        }
    }
}