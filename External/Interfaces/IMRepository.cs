namespace Muonroi.BuildingBlock.External.Interfaces;

public interface IMRepository<T> where T : MEntity
{
    IMUnitOfWork UnitOfWork { get; }

    T Add(T newEntity);

    T Update(T updateEntity);

    Task<bool> DeleteAsync(T deleteEntity);

    Task ExecuteTransactionAsync(Func<Task<MVoidMethodResult>> action);

    Task RollbackTransactionAsync();
}