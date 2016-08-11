using Bridge;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// The String global object is a constructor for strings, or a sequence of characters.
    /// </summary>
    [External]
    [Name("System.String")]
    public sealed class String : IEnumerable, IEnumerable<char>, IComparable<String>, IEquatable<String>
    {
        [FieldProperty]
        public int Length
        {
            [Template("#{this}")]
            get
            {
                return 0;
            }
        }

        [InlineConst]
        public const string Empty = "";

        [Template("System.String.build({value})")]
        public String(char[] value)
        {
        }

        [Template("System.String.build({c}, {count})")]
        public String(char c, int count)
        {
        }

        [Template("System.String.build({value}, {startIndex}, {length})")]
        public extern String(char[] value, int startIndex, int length);

        /// <summary>
        /// Indicates whether the specified string is null or an Empty string.
        /// </summary>
        /// <param name="value">The string to test. </param>
        /// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
        public static extern bool IsNullOrEmpty(string value);

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns>true if the value parameter is null or String.Empty, or if value consists exclusively of white-space characters. </returns>
        public static extern bool IsNullOrWhiteSpace(string value);

        /// <summary>
        /// Determines whether two specified String objects have the same value.
        /// </summary>
        /// <param name="a">The first string to compare, or null. </param>
        /// <param name="b">The second string to compare, or null. </param>
        /// <returns>true if the value of a is the same as the value of b; otherwise, false. If both a and b are null, the method returns true.</returns>
        public static extern bool Equals(string a, string b);

        /// <summary>
        /// Determines whether two specified String objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.
        /// </summary>
        /// <param name="a">The first string to compare, or null. </param>
        /// <param name="b">The second string to compare, or null. </param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison.</param>
        /// <returns>true if the value of a is the same as the value of b; otherwise, false. If both a and b are null, the method returns true.</returns>
        public static extern bool Equals(string a, string b, StringComparison comparisonType);

        /// <summary>
        /// Determines whether this string and a specified String object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.
        /// </summary>
        /// <param name="value">The string to compare to this instance.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared. </param>
        /// <returns>true if the value of the value parameter is the same as this string; otherwise, false.</returns>
        public extern bool Equals(string value, StringComparison comparisonType);

        /// <summary>
        /// Determines whether this instance and another specified String object have the same value.
        /// </summary>
        /// <param name="value">The string to compare to this instance.</param>
        /// <returns>true if the value of the value parameter is the same as this string; otherwise, false.</returns>
        public extern bool Equals(string value);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="string1">Strings to concatenate to this string.</param>
        /// <param name="string2">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(string string1, string string2);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="string1">Strings to concatenate to this string.</param>
        /// <param name="string2">Strings to concatenate to this string.</param>
        /// <param name="string3">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(string string1, string string2, string string3);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="string1">Strings to concatenate to this string.</param>
        /// <param name="string2">Strings to concatenate to this string.</param>
        /// <param name="string3">Strings to concatenate to this string.</param>
        /// <param name="string4">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(string string1, string string2, string string3, string string4);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="strings">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(params string[] strings);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="object1">Strings to concatenate to this string.</param>
        /// <param name="object2">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(object object1, object object2);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="object1">Strings to concatenate to this string.</param>
        /// <param name="object2">Strings to concatenate to this string.</param>
        /// <param name="object3">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(object object1, object object2, object object3);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="object1">Strings to concatenate to this string.</param>
        /// <param name="object2">Strings to concatenate to this string.</param>
        /// <param name="object3">Strings to concatenate to this string.</param>
        /// <param name="object4">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(object object1, object object2, object object3, object object4);

        /// <summary>
        /// The concat() method combines the text of two or more strings and returns a new string.
        /// </summary>
        /// <param name="objects">Strings to concatenate to this string.</param>
        /// <returns></returns>
        public static extern string Concat(params object[] objects);


        public static extern string Concat(IEnumerable<string> values);

        /// <summary>
        /// The compare() method compares two specified String objects and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare.</param>
        /// <param name="strB">The second string to compare.</param>
        /// <returns></returns>
        public static extern int Compare(string strA, string strB);

        /// <summary>
        /// The compare() method compares two specified String objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare.</param>
        /// <param name="strB">The second string to compare.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <returns></returns>
        public static extern int Compare(string strA, string strB, bool ignoreCase);

        /// <summary>
        /// The compare() method compares substrings of two specified String objects and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare.</param>
        /// <param name="indexA">The position of the substring within strA.</param>
        /// <param name="strB">The second string to compare.</param>
        /// <param name="indexB">The position of the substring within strB.</param>
        /// <param name="length">The maximum number of characters in the substrings to compare.</param>
        /// <returns></returns>
        public static extern int Compare(string strA, int indexA, string strB, int indexB, int length);

        /// <summary>
        /// The compare() method compares substrings of two specified String objects and returns an integer that indicates their relative position in the sort order.
        /// </summary>
        /// <param name="strA">The first string to compare.</param>
        /// <param name="indexA">The position of the substring within strA.</param>
        /// <param name="strB">The second string to compare.</param>
        /// <param name="indexB">The position of the substring within strB.</param>
        /// <param name="length">The maximum number of characters in the substrings to compare.</param>
        /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
        /// <returns></returns>
        public static extern int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase);

        public static extern int Compare(string strA, string strB, StringComparison comparisonType);
    
        public static extern int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture);

        public static extern int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType);

        public static extern int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture);

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A character to search for.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
        public int IndexOf(char searchValue)
        {
            return -1;
        }

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value, starting the search at fromIndex. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A character to search for.</param>
        /// <param name="fromIndex">The location within the calling string to start the search from.</param>
        /// <returns>The zero-based index position of value if that character is found, or -1 if it is not.</returns>
        public int IndexOf(char searchValue, int fromIndex)
        {
            return -1;
        }

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A string representing the value to search for.</param>
        /// <returns></returns>
        public int IndexOf(string searchValue)
        {
            return -1;
        }

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value, starting the search at fromIndex. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A string representing the value to search for.</param>
        /// <param name="fromIndex">The location within the calling string to start the search from.</param>
        /// <returns></returns>
        public int IndexOf(string searchValue, int fromIndex)
        {
            return -1;
        }

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value. The search starts at a specified character position and
        /// examines a specified number of character positions. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A character to search for.</param>
        /// <param name="fromIndex">The location within the calling string to start the search from.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns></returns>
        public int IndexOf(char searchValue, int fromIndex, int count)
        {
            return -1;
        }

        /// <summary>
        /// The indexOf() method returns the index within the calling String object of the first occurrence of the specified value. The search starts at a specified character position and
        /// examines a specified number of character positions. Returns -1 if the value is not found.
        /// </summary>
        /// <param name="searchValue">A string representing the value to search for.</param>
        /// <param name="fromIndex">The location within the calling string to start the search from.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <returns></returns>
        public int IndexOf(string searchValue, int fromIndex, int count)
        {
            return -1;
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in the current System.String object. A parameter specifies the type of search
        ///  to use for the specified string.
        /// </summary>
        /// <param name="searchValue">The string to search for.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The zero-based index position of value if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.</returns>
        public int IndexOf(string searchValue, StringComparison comparisonType)
        {
            return -1;
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in the current System.String object. Parameters specify the starting search
        ///  position in the current string and the type of search to use for the specified string.
        /// </summary>
        /// <param name="searchValue">The string to search for.</param>
        /// <param name="fromIndex">The search starting position.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The zero-based index position of value if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.</returns>
        public int IndexOf(string searchValue, int fromIndex, StringComparison comparisonType)
        {
            return -1;
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in the current System.String object. Parameters specify the starting search
        ///  position in the current string, the number of characters in the current string
        ///  to search, and the type of search to use for the specified string.
        /// </summary>
        /// <param name="searchValue">The string to search for.</param>
        /// <param name="fromIndex">The search starting position.</param>
        /// <param name="count">The number of character positions to examine.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The zero-based index position of value if that string is found, or -1 if it is not. If value is System.String.Empty, the return value is 0.</returns>
        public int IndexOf(string searchValue, int fromIndex, int count, StringComparison comparisonType)
        {
            return -1;
        }

        public extern int LastIndexOf(char ch);

        public extern int LastIndexOf(string subString);

        public extern int LastIndexOf(string subString, int startIndex);

        public extern int LastIndexOf(char ch, int startIndex, int count);

        public extern int LastIndexOf(string subString, int startIndex, int count);

        public extern int LastIndexOf(char ch, int startIndex);

        public extern int LastIndexOfAny(params char[] ch);

        public extern int LastIndexOfAny(char[] ch, int startIndex);

        public extern int LastIndexOfAny(char[] ch, int startIndex, int count);

        /// <summary>
        /// The replace() method returns a new string with some or all matches of a pattern replaced by a replacement.  The pattern can be a string or a Regex, and the replacement can be a string or a function to be called for each match.
        /// </summary>
        /// <param name="substr">A String that is to be replaced by newSubStr.</param>
        /// <param name="newSubStr">The String that replaces the substring received from parameter #1. A number of special replacement patterns are supported; see the "Specifying a string as a parameter" section below.</param>
        /// <returns></returns>
        public extern string Replace(string substr, string newSubStr);

        public extern string Replace(char oldChar, char replaceChar);

        public extern string[] Split(params char[] separator);

        public extern string[] Split(char[] separator, int limit);

        public extern string[] Split(char[] separator, int limit, StringSplitOptions options);

        public extern string[] Split(char[] separator, StringSplitOptions options);

        public extern string[] Split(string[] separator, StringSplitOptions options);

        public extern string[] Split(string[] separator, int limit, StringSplitOptions options);

        public extern string[] Split(string separator);

        public extern string[] Split(char separator);

        public extern string[] Split(char separator, int limit);

        public extern string[] Split(string separator, int limit);

        /// <summary>
        /// The substr() method returns the characters in a string beginning at the specified location through the specified number of characters.
        /// </summary>
        /// <param name="start">Location at which to begin extracting characters. If a negative number is given, it is treated as strLength+start where strLength = to the length of the string (for example, if start is -3 it is treated as strLength-3.)</param>
        /// <returns></returns>
        public extern string Substr(int start);

        /// <summary>
        /// The substr() method returns the characters in a string beginning at the specified location through the specified number of characters.
        /// </summary>
        /// <param name="start">Location at which to begin extracting characters. If a negative number is given, it is treated as strLength+start where strLength = to the length of the string (for example, if start is -3 it is treated as strLength-3.)</param>
        /// <returns></returns>
        [Name("substr")]
        public extern string Substring(int start);

        /// <summary>
        /// The substr() method returns the characters in a string beginning at the specified location through the specified number of characters.
        /// </summary>
        /// <param name="start">Location at which to begin extracting characters. If a negative number is given, it is treated as strLength+start where strLength = to the length of the string (for example, if start is -3 it is treated as strLength-3.)</param>
        /// <param name="length">The number of characters to extract.</param>
        /// <returns></returns>
        public extern string Substr(int start, int length);

        /// <summary>
        /// The substr() method returns the characters in a string beginning at the specified location through the specified number of characters.
        /// </summary>
        /// <param name="start">Location at which to begin extracting characters. If a negative number is given, it is treated as strLength+start where strLength = to the length of the string (for example, if start is -3 it is treated as strLength-3.)</param>
        /// <param name="length">The number of characters to extract.</param>
        /// <returns></returns>
        [Name("substr")]
        public extern string Substring(int start, int length);

        /// <summary>
        /// The toLower() method returns the calling string value converted to lowercase.
        /// </summary>
        /// <returns></returns>
        [Name("lower")]
        public extern string ToLower();

        /// <summary>
        /// The toUpper() method returns the calling string value converted to uppercase.
        /// </summary>
        /// <returns></returns>
        [Name("upper")]
        public extern string ToUpper();

        /// <summary>
        /// The trim() method removes whitespace from both ends of a string. Whitespace in this context is all the whitespace characters (space, tab, no-break space, etc.) and all the line terminator characters (LF, CR, etc.).
        /// </summary>
        /// <returns>The trimmed string</returns>
        public extern string Trim();

        public extern string Trim(params char[] values);

        public extern string TrimStart(params char[] values);

        public extern string TrimEnd(params char[] values);

        public extern string TrimStart();

        public extern string TrimEnd();

        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="value">The string to seek. </param>
        /// <returns>true if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
        public extern bool Contains(string value);

        public extern bool EndsWith(string suffix);

        public extern bool StartsWith(string prefix);

        /// Summary:
        ///     Replaces the format item in a specified string with the string representation
        ///     of a corresponding object in a specified array.
        ///
        /// Parameters:
        ///   format:
        ///     A composite format string.
        ///
        ///   args:
        ///     An object array that contains zero or more objects to format.
        ///
        /// Returns:
        ///     A copy of format in which the format items have been replaced by the string representation
        ///     of the corresponding objects in args.
        ///
        public static extern string Format(string format, params object[] args);

        ///
        /// Summary:
        ///     Replaces one or more format items in a specified string with the string representation
        ///     of a specified object.
        ///
        /// Parameters:
        ///   format:
        ///     A composite format string.
        ///
        ///   arg0:
        ///     The object to format.
        ///
        /// Returns:
        ///     A copy of format in which any format items are replaced by the string representation
        ///     of arg0.
        public static extern String Format(String format, object arg0);

        ///
        /// Summary:
        ///     Replaces the format items in a specified string with the string representations
        ///     of corresponding objects in a specified array. A parameter supplies culture-specific
        ///     formatting information.
        ///
        /// Parameters:
        ///   provider:
        ///     An object that supplies culture-specific formatting information.
        ///
        ///   format:
        ///     A composite format string.
        ///
        ///   args:
        ///     An object array that contains zero or more objects to format.
        ///
        /// Returns:
        ///     A copy of format in which the format items have been replaced by the string representation
        ///     of the corresponding objects in args.
        public static extern String Format(IFormatProvider provider, String format, params object[] args);

        ///
        /// Summary:
        ///     Replaces the format items in a specified string with the string representation
        ///     of two specified objects.
        ///
        /// Parameters:
        ///   format:
        ///     A composite format string.
        ///
        ///   arg0:
        ///     The first object to format.
        ///
        ///   arg1:
        ///     The second object to format.
        ///
        /// Returns:
        ///     A copy of format in which format items are replaced by the string representations
        ///     of arg0 and arg1.
        public static extern String Format(String format, object arg0, object arg1);

        ///
        /// Summary:
        ///     Replaces the format items in a specified string with the string representation
        ///     of three specified objects.
        ///
        /// Parameters:
        ///   format:
        ///     A composite format string.
        ///
        ///   arg0:
        ///     The first object to format.
        ///
        ///   arg1:
        ///     The second object to format.
        ///
        ///   arg2:
        ///     The third object to format.
        ///
        /// Returns:
        ///     A copy of format in which the format items have been replaced by the string representations
        ///     of arg0, arg1, and arg2.
        public static extern String Format(String format, object arg0, object arg1, object arg2);

        public int IndexOfAny(char[] anyOf)
        {
            return -1;
        }

        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return -1;
        }

        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            return -1;
        }

        public extern char[] ToCharArray();

        public extern char[] ToCharArray(int startIndex, int count);

        public static bool operator ==(string s1, string s2)
        {
            return false;
        }

        public static bool operator !=(string s1, string s2)
        {
            return false;
        }

        public char this[int index]
        {
            get
            {
                return default(char);
            }
        }

        public extern CharEnumerator GetEnumerator();

        extern IEnumerator<char> IEnumerable<char>.GetEnumerator();

        extern IEnumerator IEnumerable.GetEnumerator();

        public extern int CompareTo(string other);

        public extern string Insert(int startIndex, string value);

        public static extern string Join(string separator, params string[] args);

        public static extern string Join(string separator, params object[] args);

        public static extern string Join(string separator, IEnumerable<string> args);

        public static extern string Join<T>(string separator, IEnumerable<T> args);

        public static extern string Join(string separator, string[] args, int startIndex, int count);

        public extern string PadLeft(int totalWidth);

        public extern string PadLeft(int totalWidth, char ch);

        public extern string PadRight(int totalWidth);

        public extern string PadRight(int totalWidth, char ch);

        public extern string Remove(int index);

        public extern string Remove(int index, int count);

        [Template("tostring({this})")]
        public extern override string ToString();
    }
}
