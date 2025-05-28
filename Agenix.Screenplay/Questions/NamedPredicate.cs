namespace Agenix.Screenplay.Questions;


using System;

/// <summary>
/// A predicate with a name that can be used for better readability and debugging.
/// </summary>
/// <typeparam name="T">The type of the input to the predicate</typeparam>
public class NamedPredicate<T>(string name, Predicate<T> predicate)
{
    public string Name => name;

    public Predicate<T> InnerPredicate => predicate;

    public override string ToString()
    {
        return name;
    }

    public bool Invoke(T obj)
    {
        return predicate(obj);
    }

    public NamedPredicate<T> And(Predicate<T> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) && other(x));
    }

    public NamedPredicate<T> Not()
    {
        return new NamedPredicate<T>(name, x => !predicate(x));
    }

    public NamedPredicate<T> Or(Predicate<T> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) || other(x));
    }
}