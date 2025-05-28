using Agenix.Api.Util;
using NUnit.Framework;

namespace Agenix.Screenplay.Consequence;

/// <summary>
/// Abstract base class representing a consequence that can be evaluated in the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type associated with the consequence.</typeparam>
public abstract class BaseConsequence<T> : IConsequence<T>
{
    private Type _complaintType;
    private string _complaintDetails;
    protected Optional<string> Explanation = Optional<string>.Empty;
    protected Optional<string> SubjectText = Optional<string>.Empty;
    private readonly List<IPerformable> _setupActions = [];

    /// <summary>
    /// Processes the given exception and determines the type of error.
    /// If the exception is an assertion error or a recognized system error, it may return the exception itself.
    /// Otherwise, it returns null.
    /// </summary>
    /// <param name="actualError">The exception to process and evaluate.</param>
    /// <return>
    /// The processed exception if it is an assertion error or recognized system error; otherwise, null.
    /// </return>
    protected Exception ErrorFrom(Exception actualError)
    {
        if (actualError is AssertionException)
        {
            
        }
        else if (IsErrorException(actualError))
        {
            return actualError;
        }
        
        return null;
    }

    /// <summary>
    /// Evaluates whether the given exception is classified as an error exception.
    /// Error exceptions include system exceptions, out of memory exceptions, and stack overflow exceptions.
    /// </summary>
    /// <param name="ex">The exception to classify as an error exception or not.</param>
    /// <return>
    /// true if the exception is a system exception, out-of-memory exception, or stack overflow exception; otherwise, false.
    /// </return>
    private bool IsErrorException(Exception ex)
    {
        return ex is SystemException || 
               ex is OutOfMemoryException || 
               ex is StackOverflowException;
    }

    /// <summary>
    /// Throws a complaint-specific exception if a complaint type has been specified.
    /// Uses the provided exception as the cause of the complaint.
    /// </summary>
    /// <param name="actualError">The exception to include as the underlying cause of the complaint.</param>
    protected void ThrowComplaintTypeErrorIfSpecified(Exception actualError)
    {
        if (_complaintType != null)
        {
            throw Complaint.From(_complaintType, _complaintDetails, actualError);
        }
    }

    public abstract void EvaluateFor(Actor actor);

    /// <summary>
    /// Specifies an alternative complaint type to use when the consequence is evaluated to an error.
    /// This allows customization of the type of exception or complaint to be thrown in case of failure.
    /// </summary>
    /// <param name="complaintType">The type of complaint or exception to be used when the consequence fails.</param>
    /// <return>
    /// An updated consequence with the specified complaint type configured.
    /// </return>
    public virtual IConsequence<T> OrComplainWith(Type complaintType)
    {
        return OrComplainWith(complaintType, null);
    }

    /// <summary>
    /// Specifies the type of complaint and optional details for the consequence.
    /// This method provides flexibility to customize the complaint triggered upon evaluation failure.
    /// </summary>
    /// <param name="complaintType">The type of the complaint to be used, typically represented as an exception type.</param>
    /// <param name="complaintDetails">Optional detailed message or additional information associated with the complaint.</param>
    /// <return>
    /// The current consequence instance configured with the specified complaint type and details.
    /// </return>
    public virtual IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails)
    {
        _complaintType = complaintType;
        _complaintDetails = complaintDetails;
        return this;
    }

    /// <summary>
    /// Adds a specified performable action to the setup actions for the consequence.
    /// This action will be performed in the context of the Screenplay pattern when evaluating the consequence.
    /// </summary>
    /// <param name="performable">The performable action to be executed during the setup phase of the consequence.</param>
    /// <return>
    /// The current instance of the consequence, allowing method chaining.
    /// </return>
    public virtual IConsequence<T> WhenAttemptingTo(IPerformable performable)
    {
        _setupActions.Add(performable);
        return this;
    }

    public virtual IConsequence<T> Because(string explanation)
    {
        Explanation = Optional<string>.Of(explanation);
        return this;
    }

    protected string InputValues()
    {
        return string.Join(",", 
            _setupActions.Where(action => action is IRecordsInputs)
                        .Cast<IRecordsInputs>()
                        .Select(action => action.GetInputValues()));
    }

    protected string AddRecordedInputValuesTo(string message)
    {
        if (string.IsNullOrEmpty(InputValues()))
        {
            return message;
        }
        return $"{message} [{InputValues()}]";
    }

    public IConsequence<T> After(params IPerformable[] actions)
    {
        _setupActions.AddRange(actions);
        return this;
    }

    protected void PerformSetupActionsAs(Actor actor)
    {
        actor.AttemptsTo(Actor.ErrorHandlingMode.IGNORE_EXCEPTIONS, _setupActions.ToArray());
    }
}
