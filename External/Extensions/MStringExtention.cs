namespace Muonroi.BuildingBlock.External.Extensions
{
    public static class MStringExtention
    {
        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string? Truncate(this string str, int maxLength)
        {
            return str == null ? null : str.Length <= maxLength ? str : str.Left(maxLength);
        }

        public static string? DecryptConfigurationValue(IConfiguration configuration, string? value, bool isSecrectDefault, string sereckey = "")
        {
            return string.IsNullOrEmpty(value)
                ? value
                : isSecrectDefault
                ? configuration.GetCryptConfigValueCipherText(value)
                : configuration.GetCryptConfigValue(value, sereckey);
        }

        /// <summary>
        /// Converts the input string to a Base64-encoded string.
        /// </summary>
        /// <param name="plainText">The string to be encoded.</param>
        /// <returns>A Base64-encoded string.</returns>
        public static string ToBase64String(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Converts a Base64-encoded string back to a regular string.
        /// </summary>
        /// <param name="base64EncodedData">The Base64-encoded string.</param>
        /// <returns>The decoded regular string.</returns>
        public static string FromBase64String(this string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return string.Empty;
            }

            byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Left(this string str, int len)
        {
            return str == null
                ? throw new ArgumentNullException(nameof(str))
                : str.Length < len
                ? throw new ArgumentException("len argument can not be bigger than given string's length!")
                : str[..len];
        }
    }
}