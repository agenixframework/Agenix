using System;

namespace Agenix.Core;

/// <summary>
///     A builder class that creates instances of ITestAction using a provided function delegate.
/// </summary>
public class FuncITestActionBuilder(Func<ITestAction> actionProvider) : ITestActionBuilder<ITestAction>
{
    /// Builds and returns an instance of ITestAction by invoking the provided action delegate.
    /// <return>
    ///     An instance of ITestAction.
    /// </return>
    public ITestAction Build()
    {
        return actionProvider.Invoke();
    }
}