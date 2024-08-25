namespace Muonroi.BuildingBlock.External.InternalEvents
{
    public class MEntityCreatedEvent<T>(T entity) : INotification where T : MEntity
    {
        public T Data { get; set; } = entity;
    }
}