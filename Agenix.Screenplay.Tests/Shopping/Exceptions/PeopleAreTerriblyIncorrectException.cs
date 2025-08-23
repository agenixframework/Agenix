using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Shopping.Exceptions;

public class PeopleAreTerriblyIncorrectException : AssertionException
{
    public PeopleAreTerriblyIncorrectException(string message, Exception cause)
        : base(message, cause)
    {
    }

    public PeopleAreTerriblyIncorrectException(string message)
        : base(message)
    {
    }

    public PeopleAreTerriblyIncorrectException(Exception cause)
        : base(cause.Message, cause)
    {
    }
}
