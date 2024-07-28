namespace MBuildingBlock.External.InternalEvents
{
    public class MEntityDeletedEvent<T>(T entity) : INotification where T : MEntity
    {
        public T Data { get; set; } = entity;
    }
}