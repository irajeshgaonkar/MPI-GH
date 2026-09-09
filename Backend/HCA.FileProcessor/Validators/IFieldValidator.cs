using HCA.FileProcessor.Enums;

namespace HCA.FileProcessor.Validators;

public interface IFieldValidator
{
    bool Validate(string value, ValidationType type = ValidationType.None);
}

