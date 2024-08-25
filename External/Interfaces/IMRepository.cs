namespace Muonroi.BuildingBlock.External.Interfaces;

public interface IMRepository<T> where T : MEntity
{
    IMUnitOfWork UnitOfWork { get; }

    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByGuidAsync(Guid guid);

    Task<bool> AnyAsync(int id);

    Task<bool> AnyGuidAsync(Guid guid);

    T Add(T newEntity);

    T Update(T updateEntity);

    Task<bool> DeleteAsync(T deleteEntity);

    Task ExecuteTransactionAsync(Func<Task<MVoidMethodResult>> action);

    Task RollbackTransactionAsync();
}