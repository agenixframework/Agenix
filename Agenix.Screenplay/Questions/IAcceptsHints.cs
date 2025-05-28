namespace Agenix.Screenplay.Questions;

public interface IAcceptsHints
{
    void Apply<T>(ISet<T> hints) where T : IQuestionHint;

}