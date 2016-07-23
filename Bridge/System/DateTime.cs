using Bridge;

namespace System
{
    [External]
    [Name("Bridge.DateTime")]
    public struct DateTime : IComparable, IComparable<DateTime>, IEquatable<DateTime>, IFormattable
    {
        public static readonly DateTime MaxValue;

        public static readonly DateTime MinValue;

        public DateTime(long value)
        {
        }

        public DateTime(long value, DateTimeKind kind)
        {
        }

        public DateTime(int year, int month, int day)
        {
        }

        public DateTime(int year, int month, int day, int hours, int minutes, int seconds)
        {
        }

        public DateTime(int year, int month, int day, int hours, int minutes, int seconds, DateTimeKind kind) {
        }

        [Template("System.DateTime({year}, {month}, {day}, {hours}, {minutes}, {seconds}, {milliseconds}, 2)")]
        public DateTime(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds) {
        }

        public DateTime(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds, DateTimeKind kind) {
        }

        public static DateTime Now
        {
            get
            {
                return default(DateTime);
            }
        }

        public static DateTime UtcNow
        {
            get
            {
                return default(DateTime);
            }
        }

        public static DateTime Today
        {
            get
            {
                return default(DateTime);
            }
        }

        public override string ToString()
        {
            return null;
        }

        public string ToString(string format)
        {
            return null;
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return null;
        }

        public static DateTime Parse(string value)
        {
            return default(DateTime);
        }

        public static DateTime Parse(string value, IFormatProvider provider)
        {
            return default(DateTime);
        }

        public static DateTime Parse(string value, IFormatProvider provider, bool utc)
        {
            return default(DateTime);
        }

        public static DateTime Parse(string value, bool utc)
        {
            return default(DateTime);
        }

        public static bool TryParse(string value, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParse(string value, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParse(string value, IFormatProvider provider, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParse(string value, IFormatProvider provider, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public static DateTime ParseExact(string value, string format)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string format, bool utc)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string[] formats)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string[] formats, bool utc)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string format, IFormatProvider provider)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string format, IFormatProvider provider, bool utc)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string[] formats, IFormatProvider provider)
        {
            return default(DateTime);
        }

        public static DateTime ParseExact(string value, string[] formats, IFormatProvider provider, bool utc)
        {
            return default(DateTime);
        }

        public static bool TryParseExact(string value, string format, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string format, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string[] formats, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string[] formats, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string format, IFormatProvider provider, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string format, IFormatProvider provider, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string[] formats, IFormatProvider provider, out DateTime result)
        {
            result = default(DateTime);
            return false;
        }

        public static bool TryParseExact(string value, string[] formats, IFormatProvider provider, out DateTime result, bool utc)
        {
            result = default(DateTime);
            return false;
        }

        public string ToDateString()
        {
            return null;
        }

        public string ToLocaleDateString()
        {
            return null;
        }

        public string ToLocaleTimeString()
        {
            return null;
        }

        public static DateTime operator -(DateTime d, TimeSpan t)
        {
            return default(DateTime);
        }

        public static DateTime operator +(DateTime d, TimeSpan t)
        {
            return default(DateTime);
        }

        public static TimeSpan operator -(DateTime a, DateTime b)
        {
            return default(TimeSpan);
        }

        public TimeSpan Subtract(DateTime value)
        {
            return default(TimeSpan);
        }

        public static bool operator ==(DateTime a, DateTime b)
        {
            return false;
        }

        public static bool operator !=(DateTime a, DateTime b)
        {
            return false;
        }

        public static bool operator <(DateTime a, DateTime b)
        {
            return false;
        }

        public static bool operator >(DateTime a, DateTime b)
        {
            return false;
        }

        public static bool operator <=(DateTime a, DateTime b)
        {
            return false;
        }

        public static bool operator >=(DateTime a, DateTime b)
        {
            return false;
        }

        public DateTime Date
        {
            get
            {
                return default(DateTime);
            }
        }

        public int Day
        {
            get
            {
                return 0;
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return 0;
            }
        }

        public int DayOfYear
        {
            get
            {
                return 0;
            }
        }

        public int Hour
        {
            get
            {
                return 0;
            }
        }

        public int Millisecond
        {
            get
            {
                return 0;
            }
        }

        public int Minute
        {
            get
            {
                return 0;
            }
        }

        public int Month
        {
            get
            {
                return 0;
            }
        }

        public int Second
        {
            get
            {
                return 0;
            }
        }

        public int Year
        {
            get
            {
                return 0;
            }
        }

        public DateTime AddDays(double value)
        {
            return default(DateTime);
        }

        public DateTime AddHours(double value)
        {
            return default(DateTime);
        }

        public DateTime AddMilliseconds(double value)
        {
            return default(DateTime);
        }

        public DateTime AddMinutes(double value)
        {
            return default(DateTime);
        }

        public DateTime AddMonths(int months)
        {
            return default(DateTime);
        }

        public DateTime AddSeconds(double value)
        {
            return default(DateTime);
        }

        public DateTime AddYears(int value)
        {
            return default(DateTime);
        }

        public static int DaysInMonth(int year, int month)
        {
            return 0;
        }

        public static bool IsLeapYear(int year)
        {
            return false;
        }

        public int CompareTo(DateTime other)
        {
            return 0;
        }

        [Name("compareToObj")]
        public int CompareTo(object other)
        {
            return 0;
        }

        public static int Compare(DateTime t1, DateTime t2)
        {
            return 0;
        }

        public bool Equals(DateTime other)
        {
            return false;
        }

        [Name("equalsObj")]
        public override bool Equals(object other) {
            return false;
        }

        public override int GetHashCode() {
            return 0;
        }

        public static bool Equals(DateTime t1, DateTime t2)
        {
            return false;
        }

        public bool IsDaylightSavingTime()
        {
            return false;
        }

        public DateTime ToUniversalTime()
        {
            return default(DateTime);
        }

        public DateTime ToLocalTime()
        {
            return default(DateTime);
        }

        public extern DateTime Add(TimeSpan value);

        public extern DateTime AddTicks(long value);

        [Name("subtractTs")]
        public extern DateTime Subtract(TimeSpan value);

        public TimeSpan TimeOfDay
        {
            get
            {
                return default(TimeSpan);
            }
        }

        public long Ticks
        {
            get
            {
                return 0;
            }
        }
    }
}
