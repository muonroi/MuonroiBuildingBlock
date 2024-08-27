namespace Muonroi.BuildingBlock.External.Helper
{
    public static class MSaltHelper
    {
        public static string CreateSalt()
        {
            byte[] salt = new byte[24];

            using (RandomNumberGenerator csprng = RandomNumberGenerator.Create())
            {
                csprng.GetBytes(salt);
            }
            return global::System.Convert.ToBase64String(salt);
        }
    }
}