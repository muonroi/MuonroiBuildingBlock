namespace MuonroiBuildingBlock.Application.InternalEvents
{
    public class EntityCreatedEvent<T>(T entity) : INotification where T : EntityBase
    {
        public T Data { get; set; } = entity;
    }
}