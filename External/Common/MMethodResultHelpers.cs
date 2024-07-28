namespace MBuildingBlock.External.Common
{
    public static class MMethodResultHelpers
    {
        public static string GetErrorMessage(string errorCode)
        {
            return MHelpers.GetErrorMessage(errorCode);
        }

        public static void AddApiErrorMessage(this MVoidMethodResult errorResult, string errorCode, string[] errorValues)
        {
            errorResult.AddErrorMessage(errorCode, GetErrorMessage(errorCode), errorValues);
        }
    }
}