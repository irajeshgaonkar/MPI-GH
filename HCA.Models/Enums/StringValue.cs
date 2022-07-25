namespace HCA.Models.Enums;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class StringValueAttribute : Attribute
{
    public StringValueAttribute(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}