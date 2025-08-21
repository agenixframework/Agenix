namespace Agenix.Screenplay.Tests.Shopping.Questions;

public class BrokenQuestion : IQuestion<int>
{
    private readonly bool _assertionFailure;

    public BrokenQuestion(bool assertionFailure)
    {
        _assertionFailure = assertionFailure;
    }

    public async Task<int> AnsweredBy(Actor actor)
    {
        await actor.AttemptsTo(
            ITask.Where("{0} attempts to do something that will throw an exception",
                _ =>
                {
                    if (_assertionFailure)
                    {
                        throw new InvalidOperationException("This is a broken question");
                    }

                    throw new ArgumentException("This is a broken question");
                }
            )
        );
        return 10;
    }

    public static BrokenQuestion ThatThrowsAnException()
    {
        return new BrokenQuestion(false);
    }

    public static BrokenQuestion ThatThrowsAnAssertionError()
    {
        return new BrokenQuestion(true);
    }
}
