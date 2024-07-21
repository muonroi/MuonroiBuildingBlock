namespace MuonroiBuildingBlock.Contract.Interfaces;

public interface IBaseRepository<T> where T : EntityBase
{
    IUnitOfWork UnitOfWork { get; }

    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByGuidAsync(Guid guid);

    Task<bool> AnyAsync(int id);

    Task<bool> AnyGuidAsync(Guid guid);

    T Add(T newEntity);

    T Update(T updateEntity);

    Task<bool> DeleteAsync(T deleteEntity);

    Task ExecuteTransactionAsync(Func<Task<VoidMethodResult>> action);
}