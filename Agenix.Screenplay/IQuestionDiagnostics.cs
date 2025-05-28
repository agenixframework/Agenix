namespace Agenix.Screenplay;

/// <summary>
/// Represents an interface that provides diagnostic information for question-related features.
/// </summary>
public interface IQuestionDiagnostics
{
    Type OnError();
}