using HCA.FileProcessor.Enums;

namespace HCA.FileProcessor.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LineParserAttribute : Attribute
{
    public ParserType ParserType;

    public LineParserAttribute(ParserType parserType = ParserType.None)
    {
        ParserType = parserType;
    }
}

