namespace MuonroiBuildingBlock.Startup.Configurations
{
    public class CustomBadRequest : VoidMethodResult
    {
        public CustomBadRequest(ActionContext context)
        {
            if (context != null)
            {
                ConstructErrorMessages(context);
            }
            else
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        public CustomBadRequest(ErrorResponseContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            ConstructErrorMessages(context);
        }

        private void ConstructErrorMessages(ErrorResponseContext context)
        {
            AddErrorMessage(context.ErrorCode, context.Message, [context.MessageDetail ?? string.Empty]);
        }

        private void ConstructErrorMessages(ActionContext context)
        {
            foreach (KeyValuePair<string, ModelStateEntry> item in context.ModelState)
            {
                string key = item.Key;
                _ = item.Value.Errors;
                if (item.Value.Errors.Count > 0)
                {
                    string format = "FORNAT_ERROR";
                    AddErrorMessage("ERR_COM_INVALID_FORMAT", string.Format(CultureInfo.InvariantCulture, format, key), [Helpers.GenerateErrorResult(key, "cannot get the value!")]);
                }
            }
        }

        private string GetErrorMessage(ModelError error)
        {
            return string.IsNullOrEmpty(error.ErrorMessage) ? "The input was not valid." : error.ErrorMessage;
        }
    }
}