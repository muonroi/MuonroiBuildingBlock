namespace Muonroi.BuildingBlock.External.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions option = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = false
        };

        public static string Serialize(this object obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, option);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}