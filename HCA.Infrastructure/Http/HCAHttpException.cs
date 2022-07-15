using System;
namespace HCA.Infrastructure.Http;

public class HCAHttpException : Exception
{
    public HCAHttpException(string statusCode, string message) : base($"{statusCode}: {message}")
    {
    }
}

