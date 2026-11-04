using System.Text.RegularExpressions;

namespace Skaar.ValueType;

/// <summary>
/// This is a helper class for value types
/// </summary>
public static class Helper
{
    /// <summary>
    /// Provides methods for cleaning.
    /// </summary>
    public static class Clean
    {
        /// <summary>
        /// A trivial implementation that returns the value unchanged.
        /// </summary>
        public static ReadOnlySpan<char> Default(ReadOnlySpan<char> value) => value;
        /// <summary>
        /// Removes all non-digit characters from the input value.
        /// </summary>
        public static ReadOnlySpan<char> RemoveNonDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsDigit);
        /// <summary>
        /// Removes all non-letter and non-digit characters from the input value.
        /// </summary>
        public static ReadOnlySpan<char> RemoveNonLettersOrDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsLetterOrDigit);
        /// <summary>
        /// Removes all whitespace characters from the input value.
        /// </summary>
        public static ReadOnlySpan<char> RemoveWhitespace(ReadOnlySpan<char> rawValue) => Filter(rawValue, c => !char.IsWhiteSpace(c));
        /// <summary>
        /// Trims whitespace from the start and end of the input value.
        /// </summary>
        public static ReadOnlySpan<char> Trim(ReadOnlySpan<char> rawValue) => rawValue.Trim();
        /// <summary>
        /// Filters the input value based on a predicate.
        /// </summary>
        public static ReadOnlySpan<char> Filter(ReadOnlySpan<char> rawValue, Predicate<char> predicate)
        {
            var buffer = rawValue.Length <= 256 ? stackalloc char[rawValue.Length] : new char[rawValue.Length];

            int j = 0;
            for (int i = 0; i < rawValue.Length; i++)
            {
                if (predicate(rawValue[i]))
                {
                    buffer[j++] = rawValue[i];
                }
            }
            var result = new char[j];
            buffer[..j].CopyTo(result);
            return result;
        }
    }

    /// <summary>
    /// Provides methods for validation.
    /// </summary>
    public static class Validate
    {
        /// <summary>
        /// The value is valid if it is not empty.
        /// </summary>
        public static bool Default(ReadOnlySpan<char> value) => !value.IsEmpty;
        /// <summary>
        /// Checks if the value contains only digits.
        /// </summary>
        public static bool IsDigits(ReadOnlySpan<char> value) => All(value, char.IsDigit);
        /// <summary>
        /// Validates all characters based on a predicate.
        /// </summary>
        public static bool All(ReadOnlySpan<char> value, Predicate<char> predicate)
        {
            foreach (var c in value)
            {
                if (!predicate(c))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Validates if the value matches a regular expression.
        /// </summary>
        public static bool IsMatch(ReadOnlySpan<char> value, Regex regex) => regex.IsMatch(value);
    }
}
