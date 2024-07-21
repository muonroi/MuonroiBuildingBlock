namespace MuonroiBuildingBlock.Services.Interfaces
{
    public interface IDateTimeService
    {
        DateTime Now();

        DateTime UtcNow();

        DateTime Today();

        DateTime UtcToday();
    }
}