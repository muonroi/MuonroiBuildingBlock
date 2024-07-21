namespace MuonroiBuildingBlock.Attribute
{
    public class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<string> errors = context.ModelState.Values
                 .Where(modelState => modelState.ValidationState == ModelValidationState.Invalid)
                 .SelectMany(modelState => modelState.Errors)
                 .Select(error => error.ErrorMessage)
                 .ToList();
                MuonroiResponse<object> applicationResponse = new()
                {
                    Result = null,
                    Error = new MuonroiErrorResult
                    {
                        ErrorValues = { errors }
                    }
                };
                context.Result = new OkObjectResult(applicationResponse);
            }
        }
    }
}