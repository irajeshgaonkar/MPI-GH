namespace HCA.FileProcessor.Converters;

public interface IFieldValueConverter
{
    dynamic Convert(Type type, string value);

    dynamic? GetDefaultValue(Type type);
}

