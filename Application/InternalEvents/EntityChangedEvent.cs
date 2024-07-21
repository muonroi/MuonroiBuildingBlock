namespace MuonroiBuildingBlock.Application.InternalEvents
{
    public class EntityChangedEvent<T>(T entity) : INotification where T : EntityBase
    {
        public T Data { get; set; } = entity;
    }
}