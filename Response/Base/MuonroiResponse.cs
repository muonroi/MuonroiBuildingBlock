namespace MuonroiBuildingBlock.Response.Base
{
    public class MuonroiResponse<T> : VoidMethodResult
    {
        public T? Result { get; set; }
        public MuonroiErrorResult? Error { get; set; }

        public void AddResultFromErrorList(IEnumerable<MuonroiErrorResult> errorMessages)
        {
            foreach (MuonroiErrorResult errorMessage in errorMessages)
            {
                AddErrorMessage(errorMessage);
            }
        }

        public override IActionResult GetActionResult()
        {
            ObjectResult objectResult = new(this);
            if (StatusCode.HasValue)
            {
                objectResult.StatusCode = StatusCode;
                return objectResult;
            }

            objectResult.StatusCode = IsOK ? (Result != null) ? 200 : 204 : 500;

            return objectResult;
        }
    }
}