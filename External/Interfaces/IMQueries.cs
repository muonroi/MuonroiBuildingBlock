namespace MBuildingBlock.External.Interfaces;

public interface IMQueries<T> where T : MEntity
{
    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByGuidAsync(Guid guid);

    Task<List<T>?> GetAllAsync(int? siteId = null);

    Task<MPagedResult<T>?> GetAllAsync(int page, int pageSize);
}