namespace Muonroi.BuildingBlock.External.Response
{
    [NotMapped]
    public class MErrorResult
    {
        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public List<object> ErrorValues { get; set; }

        public MErrorResult()
        {
            ErrorValues = [];
        }

        public override string ToString()
        {
            return ErrorValues != null && ErrorValues.Count > 0
                ? "[" + ErrorCode + ": " + ErrorMessage + " (" + string.Join(',', ErrorValues) + ")]"
                : "[" + ErrorCode + ": " + ErrorMessage + "]";
        }
    }
}