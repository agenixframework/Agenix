using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Shopping.Exceptions;

public class PeopleAreSoRudeException : AssertionException
{
    public PeopleAreSoRudeException(string message, Exception cause)
        : base(message, cause)
    {
    }

    public PeopleAreSoRudeException(string message)
        : base(message)
    {
    }

    public PeopleAreSoRudeException(Exception cause)
        : base(cause?.Message, cause)
    {
    }
}
