namespace Muonroi.BuildingBlock.External.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> entities) where T : class
        {
            if (entities == null || !entities.Any())
            {
                return;
            }

            using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                await dbContext.Set<T>().AddRangeAsync(entities).ConfigureAwait(false);
                _ = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }

}
