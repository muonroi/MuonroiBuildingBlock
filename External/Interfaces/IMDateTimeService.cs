namespace MBuildingBlock.External.Interfaces
{
    public interface IMDateTimeService
    {
        DateTime Now();

        DateTime UtcNow();

        DateTime Today();

        DateTime UtcToday();
    }
}