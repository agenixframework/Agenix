namespace Agenix.Screenplay.Questions;

/// <summary>
/// A question that returns a value previously remembered by the actor.
/// </summary>
/// <typeparam name="T">The type of the remembered value</typeparam>
public class Remembered<T> : IQuestion<T>
{
    private readonly string _key;

    private Remembered(string key)
    {
        _key = key;
    }

    public static Remembered<T> ValueOf(string key)
    {
        return new Remembered<T>(key);
    }

    public T AnsweredBy(Actor actor)
    {
        return actor.Recall<T>(_key);
    }
}
