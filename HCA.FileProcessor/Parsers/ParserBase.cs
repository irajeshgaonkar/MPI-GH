using System.Reflection;
using System.Text;
using HCA.FileProcessor.Attributes;
using HCA.FileProcessor.Converters;
using HCA.FileProcessor.Validators;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;

namespace HCA.FileProcessor.Parsers;

/// <summary>
/// Base Parser for Parsing a line (from csv file)
/// </summary>
/// <typeparam name="T">The output entity type that will be from the input line</typeparam>
public class LineParser<T> where T : new()
{
    private char _fieldDelimiter;

    private readonly Func<string, string> _preParser;

    private readonly IFieldValidator _fieldValidator;

    private readonly IFieldValueConverter _fieldValueConverter;

    /// <summary>
    /// <see cref="ParserBase{T}"/>
    /// </summary>
    /// <param name="fieldDelimiter">Delimiter for the fields (columns) in the line</param>
    public LineParser(char fieldDelimiter = ',')
    {
        _fieldDelimiter = fieldDelimiter;
        _preParser = FieldParserFactory.GetLineParser(typeof(T));
        _fieldValidator = new FieldValidator();
        _fieldValueConverter = new FieldValueConverter();
    }

    public T Parse(string line, out string errorMessage)
    {
        var values = GetValues(line);
        var model = Parse(values, out errorMessage);
        return model;
    }

    public string[]? ParseToValues(string line) {
        var values = GetValues(line);
        return values;
    }

    public T Parse(string[] values, out string errorMessage)
    {
        var type = typeof(T);
        var value = new T();
        StringBuilder errorMessageBuilder = new();

        foreach (var propertyInfo in type.GetProperties())
        {
            var propValue = GetPropertyValue(propertyInfo, values, ref errorMessageBuilder);

            if (null == propValue)
                propValue = _fieldValueConverter.GetDefaultValue(propertyInfo.PropertyType);

            if (null != propValue)
                propertyInfo.SetValue(value, propValue, null);
        }

        errorMessage = errorMessageBuilder.ToString();
        errorMessage = errorMessage.Length > 2 ? errorMessage[..^2] : errorMessage;
        return value;
    }

    private dynamic? GetPropertyValue(PropertyInfo propertyInfo, string[] values, ref StringBuilder validationMessageBuilder)
    {
        var value = GetFieldValue(propertyInfo, values);
        if (null == value) return value;
        var isValid = Validate(propertyInfo, value, ref validationMessageBuilder);
        if (!isValid) return null;
        return ConvertValue(propertyInfo, value);
    }

    private dynamic ConvertValue(PropertyInfo propertyInfo, string value)
    {
        var type = propertyInfo.PropertyType;
        if (null == type) return value;
        return _fieldValueConverter.Convert(type, value);
    }

    private bool Validate(PropertyInfo propertyInfo, string value, ref StringBuilder validationMessageBuilder)
    {
        var isFieldValid = true;
        var attributes = propertyInfo.GetCustomAttributes<FieldValidatorAttribute>(false);
        foreach(var attribute in attributes)
        {
            var isValid = _fieldValidator.Validate(value, attribute.ValidationType);
            if (!isValid)
                validationMessageBuilder.Append($"{attribute.ErrorMessage} | ");

            if (isFieldValid) isFieldValid = isValid;
        }

        return isFieldValid;
    }

    private string? GetFieldValue(PropertyInfo propertyInfo, string[] values)
    {
        var attribute = propertyInfo.GetCustomAttribute<FieldPositionAttribute>(false);
        if (null == attribute) return null;
        var value = HasValue(attribute, values)
                    ? values[attribute.Position]
                    : string.Empty;

        if (value.StringEquals("NULL")) value = string.Empty;

        value = _preParser(value);
        return value;
    }

    private bool HasValue(FieldPositionAttribute attribute, ICollection<string> values)
    {
        return attribute.Position != -1 &&
                    values.Count > attribute.Position;
    }

    private string[] GetValues(string line)
    {
        var values = line.SplitByChar(_fieldDelimiter);
        return values;
    }

    public bool IsEmptyLine(ICollection<string> values)
    {
        foreach (var value in values) if (!value.IsEmpty()) return false;
        return true;
    }
}

