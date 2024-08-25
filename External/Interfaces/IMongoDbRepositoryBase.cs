namespace Muonroi.BuildingBlock.External.Interfaces
{
    public interface IMongoDbRepositoryBase<T> where T : MMongoDbEntity
    {
        IMongoCollection<T> FindAll(ReadPreference? readPreference = null);

        Task CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(string id);
    }
}