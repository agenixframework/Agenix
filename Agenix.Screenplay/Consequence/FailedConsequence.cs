namespace Agenix.Screenplay.Consequence;

/// <summary>
/// Represents a consequence that failed during its execution in the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type of value associated with the consequence that failed.</typeparam>
public class FailedConsequence<T>
{
    private readonly IConsequence<T> _consequence;
    private readonly Exception _runtimeExceptionCause;
    private readonly Exception _errorCause;

    public FailedConsequence(IConsequence<T> consequence, Exception cause)
    {
        _consequence = consequence;
        if (IsErrorException(cause))
        {
            _errorCause = cause;
            _runtimeExceptionCause = null;
        }
        else if (cause is Exception)
        {
            _errorCause = null;
            _runtimeExceptionCause = cause;
        }
        else
        {
            _errorCause = null;
            _runtimeExceptionCause = cause;
        }
    }

    public IConsequence<T> Consequence => _consequence;

    public Exception Cause => _runtimeExceptionCause ?? _errorCause;

    public void ThrowException()
    {
        if (_runtimeExceptionCause != null)
        {
            throw _runtimeExceptionCause;
        }
        throw _errorCause;
    }

    private bool IsErrorException(Exception ex)
    {
        // In C#, we'll consider certain exception types as "errors"
        return ex is SystemException || 
               ex is OutOfMemoryException || 
               ex is StackOverflowException;
    }
}
