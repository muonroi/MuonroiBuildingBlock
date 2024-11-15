namespace Muonroi.BuildingBlock.External.Interfaces;

public interface IMRepository<T> where T : MEntity
{
    IMUnitOfWork UnitOfWork { get; }

    T Add(T newEntity);

    T Update(T updateEntity);

    Task<bool> DeleteAsync(T deleteEntity);

    Task ExecuteTransactionAsync(Func<Task<MVoidMethodResult>> action);

    Task RollbackTransactionAsync();

    Task<int> AddBatchAsync(IEnumerable<T> newEntities);

    Task<int> UpdateBatchAsync(Expression<Func<T, bool>> predicate, Action<T> updateAction);

    Task<int> DeleteBatchAsync(Expression<Func<T, bool>> predicate);

    Task<bool> SoftRestoreAsync(T entity);

    Task BulkInsertAsync(IEnumerable<T> entities);
}