namespace MuonroiBuildingBlock.Infrastructure.Extentions
{
    [DebuggerStepThrough]
    public static class Check
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>(T value, [InvokerParameterName][JetBrains.Annotations.NotNull] string parameterName)
        {
            return value == null ? throw new ArgumentNullException(parameterName) : value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotNullOrEmpty(string value, [InvokerParameterName][JetBrains.Annotations.NotNull] string parameterName)
        {
            return value.IsNullOrEmpty() ? throw new ArgumentException($"{parameterName} can not be null or empty!", parameterName) : value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotNullOrWhiteSpace(string value, [InvokerParameterName][JetBrains.Annotations.NotNull] string parameterName)
        {
            return value.IsNullOrWhiteSpace()
                ? throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName)
                : value;
        }

        [ContractAnnotation("value:null => halt")]
        public static ICollection<T> NotNullOrEmpty<T>(ICollection<T> value, [InvokerParameterName][JetBrains.Annotations.NotNull] string parameterName)
        {
            return value.IsNullOrEmpty() ? throw new ArgumentException(parameterName + " can not be null or empty!", parameterName) : value;
        }
    }
}