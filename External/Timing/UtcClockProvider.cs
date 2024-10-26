namespace Muonroi.BuildingBlock.External.Timing
{
    /// <summary>
    /// Implements <see cref="IClockProvider"/> to work with UTC times.
    /// </summary>
    public class UtcClockProvider : IClockProvider
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTimeKind Kind => DateTimeKind.Utc;

        public bool SupportsMultipleTimezone => true;

        public DateTime Normalize(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : dateTime;
        }

        internal UtcClockProvider()
        {
        }
    }
}