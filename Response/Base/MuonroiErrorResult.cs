namespace MuonroiBuildingBlock.Response.Base
{
    [NotMapped]
    public class MuonroiErrorResult
    {
        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public List<object> ErrorValues { get; set; }

        public MuonroiErrorResult()
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