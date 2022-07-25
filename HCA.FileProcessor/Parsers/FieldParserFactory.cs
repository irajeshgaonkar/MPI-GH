using HCA.FileProcessor.Attributes;
using HCA.FileProcessor.Enums;
using HCA.Infrastructure.Extensions;

namespace HCA.FileProcessor.Parsers;

public class FieldParserFactory
{
    public static Func<string, string> GetLineParser(Type type)
    {
        var attribute = Attribute.GetCustomAttribute(type, typeof(LineParserAttribute));
        if (null == attribute) return GetParser(ParserType.None);
        var preParserAttribute = (LineParserAttribute)attribute;
        if (null == preParserAttribute) return GetParser(ParserType.None);
        return GetParser(preParserAttribute.ParserType);
    }

    public static Func<string, string> GetParser(ParserType parserType)
    {
        if (ParserType.Header == parserType) return ParseHeaderField;
        return (value) => value.TrimValue();
    }

    private static string ParseHeaderField(string value)
    {
        if (value.IsEmpty()) return string.Empty;
        return value.RemoveDescriptionAndTrim();
    }
}

