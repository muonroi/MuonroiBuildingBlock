namespace MBuildingBlock.External.Extentions
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