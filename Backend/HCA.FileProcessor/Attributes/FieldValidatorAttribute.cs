using HCA.FileProcessor.Enums;

namespace HCA.FileProcessor.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class FieldValidatorAttribute : Attribute
{
    public ValidationType ValidationType;

    public string ErrorMessage;

    public FieldValidatorAttribute(ValidationType validationType)
    {
        ValidationType = validationType;
    }
}

