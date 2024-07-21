namespace MuonroiBuildingBlock.Application.InternalEvents
{
    public class EntityDeletedEvent<T>(T entity) : INotification where T : EntityBase
    {
        public T Data { get; set; } = entity;
    }
}