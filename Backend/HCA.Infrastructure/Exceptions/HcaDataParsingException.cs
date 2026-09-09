namespace HCA.Infrastructure.Exceptions;

public class HcaDataParsingException : Exception
{
    public HcaDataParsingException(string message) : base(message)
    {
    }
}

public class HcaBadRequestException : Exception
{
    public HcaBadRequestException(string message) : base(message)
    {
    }
}

