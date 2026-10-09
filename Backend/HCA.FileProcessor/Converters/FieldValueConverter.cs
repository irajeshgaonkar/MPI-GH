using System.Globalization;
using HCA.Infrastructure.Extensions;
using HCA.Models.Enums;

namespace HCA.FileProcessor.Converters;

public class FieldValueConverter : IFieldValueConverter
{
    public static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    public dynamic Convert(Type type, string value)
    {
        if (typeof(DateTime) == type || typeof(DateTime?) == type)
        {
            DateTime.TryParse(value, culture, DateTimeStyles.None, out var dateTime);
            return dateTime;
        }

        if (typeof(DateOnly) == type || typeof(DateOnly?) == type)
        {
            DateOnly.TryParse(value, culture, DateTimeStyles.None, out var dateOnly);
            return dateOnly;
        }

        if (typeof(TimeOnly) == type || typeof(TimeOnly?) == type)
        {
            TimeOnly.TryParse(value, culture, DateTimeStyles.None, out var timeOnly);
            return timeOnly;
        }

        if (typeof(ApiCallType) == type)
        {
            return value.ParseToApiCallType();
        }

        if (typeof(bool) == type || typeof(bool?) == type)
        {
            bool.TryParse(value, out bool boolValue);
            return boolValue;
        }

        return value;
    }

    public dynamic? GetDefaultValue(Type type)
    {
        if (typeof(DateTime) == type) return DateTime.MinValue;
        if (typeof(DateTime?) == type) return null;
        if (typeof(string) == type) return string.Empty;
        if (typeof(bool) == type) return false;
        if (typeof(bool?) == type) return null;
        if (typeof(TimeOnly) == type) return TimeOnly.MinValue;
        if (typeof(TimeOnly?) == type) return null;
        if (typeof(DateOnly) == type) return DateOnly.MinValue;
        if (typeof(DateOnly?) == type) return null;
        return null;
    }
}

