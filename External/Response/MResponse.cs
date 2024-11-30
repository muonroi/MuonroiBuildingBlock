namespace Muonroi.BuildingBlock.External.Response
{
    public class MResponse<T> : MVoidMethodResult
    {
        public T? Result { get; set; }
        public MErrorResult? Error { get; set; }

        public void AddResultFromErrorList(IEnumerable<MErrorResult> errorMessages)
        {
            foreach (MErrorResult errorMessage in errorMessages)
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

            objectResult.StatusCode = StatusCodes.Status200OK
                ;

            return objectResult;
        }
    }
}