using System;

namespace Agenix.Core;

/// <summary>
///     A builder class that creates instances of ITestAction using a provided function delegate.
/// </summary>
public class FuncITestActionBuilder<T> : ITestActionBuilder<T> where T : ITestAction
{
    private readonly Func<T> _actionProvider;

    public FuncITestActionBuilder(Func<T> actionProvider)
    {
        _actionProvider = actionProvider;
    }

    /// Builds and returns an instance of ITestAction by invoking the provided action delegate.
    /// <return>
    ///     An instance of ITestAction.
    /// </return>
    public T Build()
    {
        return _actionProvider.Invoke();
    }
}