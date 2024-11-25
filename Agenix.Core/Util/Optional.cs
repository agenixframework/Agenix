using System;

namespace Agenix.Core.Util;

public class Optional<T>
{
    private readonly T _value;

    private Optional()
    {
        IsPresent = false;
    }

    private Optional(T value)
    {
        _value = value;
        IsPresent = true;
    }

    public static Optional<T> Empty => new();

    public bool IsPresent { get; }

    public T Value => IsPresent ? _value : throw new InvalidOperationException("No value present");

    public static Optional<T> Of(T value)
    {
        return value != null ? new Optional<T>(value) : throw new ArgumentNullException(nameof(value));
    }

    public static Optional<T> OfNullable(T value)
    {
        return value != null ? new Optional<T>(value) : Empty;
    }

    public T OrElse(T other)
    {
        return IsPresent ? _value : other;
    }

    public T OrElseGet(Func<T> other)
    {
        return IsPresent ? _value : other();
    }


    public static Optional<T> From(T value)
    {
        return new Optional<T>(value);
    }
}