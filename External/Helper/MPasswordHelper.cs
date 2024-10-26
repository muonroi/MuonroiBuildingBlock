namespace Muonroi.BuildingBlock.External.Helper
{
    public static class MPasswordHelper
    {
        public static string HashPassword(string password, out string salt)
        {
            salt = MSaltHelper.CreateSalt();
            string saltedPassword = password + salt;
            return BCrypts.HashPassword(saltedPassword);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypts.Verify(enteredPassword, storedHash);
        }
    }
}