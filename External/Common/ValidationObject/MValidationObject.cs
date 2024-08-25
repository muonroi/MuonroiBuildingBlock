namespace Muonroi.BuildingBlock.External.Common.ValidationObject
{
    public class MValidationObject
    {
        protected List<MErrorResult> _errorMessages = [];

        [JsonIgnore]
        public IReadOnlyCollection<MErrorResult> ErrorMessages => _errorMessages;

        protected void AddValidationError(string errorCode, string propertyName, object propertyValue)
        {
            AddValidationError(errorCode, [MHelpers.GenerateErrorResult(propertyName, propertyValue)]);
        }

        protected void AddValidationError(string errorCode, List<object> errorValues)
        {
            _errorMessages.Add(new MErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = MHelpers.GetErrorMessage(errorCode),
                ErrorValues = errorValues
            });
        }

        protected void AddValidationErrors(IEnumerable<MErrorResult> errorMessages)
        {
            _errorMessages.AddRange(errorMessages);
        }

        public virtual bool IsValid()
        {
            ValidationContext validationContext = new(this, null, null);
            List<ValidationResult> list = [];
            if (!Validator.TryValidateObject(this, validationContext, list, validateAllProperties: true))
            {
                foreach (ValidationResult item in list)
                {
                    if (item.ErrorMessage == null)
                    {
                        continue;
                    }

                    MErrorResult errorResult = new()
                    {
                        ErrorCode = item.ErrorMessage,
                        ErrorMessage = MHelpers.GetErrorMessage(item.ErrorMessage)
                    };
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo? property = validationContext.ObjectType.GetProperty(memberName);
                        if (property != null)
                        {
                            object? value = property.GetValue(validationContext.ObjectInstance, null);
                            if (value != null)
                            {
                                errorResult.ErrorValues.Add(MHelpers.GenerateErrorResult(memberName, value));
                            }
                        }
                    }
                    _errorMessages.Add(errorResult);
                }
            }

            return _errorMessages.Count == 0;
        }
    }
}