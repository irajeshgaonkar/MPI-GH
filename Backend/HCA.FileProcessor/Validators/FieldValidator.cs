using System.Globalization;
using HCA.FileProcessor.Enums;
using HCA.Infrastructure.Extensions;

namespace HCA.FileProcessor.Validators;

public class FieldValidator : IFieldValidator
{
    public static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    private IDictionary<ValidationType, Func<string, bool>> _validators;

    public FieldValidator()
    {
        _validators = BuildValidators();
    }

    public IDictionary<ValidationType, Func<string, bool>> BuildValidators()
    {
        return new Dictionary<ValidationType, Func<string, bool>>
        {
            [ValidationType.Required] = ValidateRequired,
            [ValidationType.Date] = ValidateDate,
            [ValidationType.Time] = ValidateTime,
            [ValidationType.DateTime] = ValidateDateTime,
            [ValidationType.Email] = ValidateEmail,
            [ValidationType.Phone] = ValidatePhone
        };
    }

    public bool Validate(string value, ValidationType type = ValidationType.None)
    {
        var validationTypes = Enum.GetValues(typeof(ValidationType)).Cast<ValidationType>();

        foreach (var validationType in validationTypes)
        {
            if (type == validationType && _validators.ContainsKey(validationType))
            {
                var validator = _validators[validationType];
                if (!validator(value)) return false;
            }
        }

        return true;
    }

    public bool ValidateRequired(string value) => !value.IsEmpty();

    public bool ValidateDate(string value)
    {
        return DateOnly.TryParse(value, culture, DateTimeStyles.None, out var date);
    }

    public bool ValidateTime(string value)
    {
        return TimeOnly.TryParse(value, culture, DateTimeStyles.None, out var time);
    }

    public bool ValidateDateTime(string value)
    {
        return DateTime.TryParse(value, culture, DateTimeStyles.None, out var time);
    }

    public bool ValidateEmail(string email)
    {
        return true;
    }

    public bool ValidatePhone(string phone)
    {
        return true;
    }
}

