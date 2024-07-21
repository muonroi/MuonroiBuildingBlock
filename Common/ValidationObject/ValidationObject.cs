namespace MuonroiBuildingBlock.Common.ValidationObject
{
    public class ValidationObject
    {
        protected List<MuonroiErrorResult> _errorMessages = [];

        [JsonIgnore]
        public IReadOnlyCollection<MuonroiErrorResult> ErrorMessages => _errorMessages;

        protected Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        protected void AddValidationError(string errorCode, string propertyName, object propertyValue)
        {
            AddValidationError(errorCode, [Helpers.GenerateErrorResult(propertyName, propertyValue)]);
        }

        protected void AddValidationError(string errorCode, List<object> errorValues)
        {
            _errorMessages.Add(new MuonroiErrorResult
            {
                ErrorCode = errorCode,
                ErrorMessage = Helpers.GetErrorMessage(errorCode, GetAssembly()),
                ErrorValues = errorValues
            });
        }

        protected void AddValidationErrors(IEnumerable<MuonroiErrorResult> errorMessages)
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

                    MuonroiErrorResult errorResult = new()
                    {
                        ErrorCode = item.ErrorMessage,
                        ErrorMessage = Helpers.GetErrorMessage(item.ErrorMessage, GetAssembly())
                    };
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo? property = validationContext.ObjectType.GetProperty(memberName);
                        if (property != null)
                        {
                            object? value = property.GetValue(validationContext.ObjectInstance, null);
                            if (value != null)
                            {
                                errorResult.ErrorValues.Add(Helpers.GenerateErrorResult(memberName, value));
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