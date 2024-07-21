namespace MuonroiBuildingBlock.Contract.Interfaces;

public interface IBaseQueries<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByGuidAsync(Guid guid);

    Task<List<T>?> GetAllAsync(int? siteId = null);

    Task<PagingItems<T>?> GetAllAsync(int page, int pageSize);
}