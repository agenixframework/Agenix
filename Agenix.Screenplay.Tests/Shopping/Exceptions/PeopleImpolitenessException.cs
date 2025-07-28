using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Shopping.Exceptions;

public class PeopleAreSoImpoliteException : AssertionException
{
    public PeopleAreSoImpoliteException(Exception cause)
        : base(cause.Message, cause)
    {
    }

    public PeopleAreSoImpoliteException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
