using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Shopping.Exceptions;

public class ThisTakesTooLongException : AssertionException
{
    public ThisTakesTooLongException(string detailMessage) : base(detailMessage)
    {
    }

    public ThisTakesTooLongException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
