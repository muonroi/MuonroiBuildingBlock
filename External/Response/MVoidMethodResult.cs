namespace MBuildingBlock.External.Response
{
    public class MVoidMethodResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime UtcTime { get; set; } = DateTime.UtcNow;

        private readonly List<MErrorResult> _errorMessages = [];

        public IReadOnlyCollection<MErrorResult> ErrorMessages => _errorMessages;

        public bool IsOK => _errorMessages.Count == 0;

        public int? StatusCode { get; set; }

        public void AddErrorMessage(MErrorResult errorResult)
        {
            _errorMessages.Add(errorResult);
        }

        public void AddErrorMessage(string errorCode, string errorMessage, params object[] arguments)
        {
            MErrorResult errorResult = new()
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
            if (arguments != null && arguments.Length != 0)
            {
                errorResult.ErrorValues.AddRange(from string item in arguments
                                                 select item);
            }

            AddErrorMessage(errorResult);
        }

        public void AddErrorMessage(string exceptionErrorMessage, string exceptionStackTrace = "")
        {
            AddErrorMessage("ERR_COM_API_SERVER_ERROR", "API_ERROR", exceptionErrorMessage, exceptionStackTrace, []);
        }

        private void AddErrorMessage(string errorCode, string errorMessage, string exceptionErrorMessage, string exceptionStackTrace, params object[] arguments)
        {
            _errorMessages.Add(new MErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = "Error: " + errorMessage + ", Exception Message: " + exceptionErrorMessage + ", Stack Trace: " + exceptionStackTrace,
                ErrorValues = [.. arguments]
            });
        }

        public virtual IActionResult GetActionResult()
        {
            ObjectResult objectResult = new(this);
            if (!StatusCode.HasValue)
            {
                objectResult.StatusCode = 500;
                return objectResult;
            }

            objectResult.StatusCode = StatusCode;
            return objectResult;
        }
    }
}