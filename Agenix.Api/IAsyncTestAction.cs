using Agenix.Api.Context;

namespace Agenix.Api;

/// <summary>
///     Defines an asynchronous test action interface, inheriting functionality from the
///     ITestAction interface and offering support for asynchronous execution.
/// </summary>
public interface IAsyncTestAction
{
    /// <summary>
    ///     Name of test action
    /// </summary>
    /// <returns>name as String</returns>
    string Name => GetType().Name;

    /// <summary>
    ///     Executes the test action asynchronously.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default);
}

/// <summary>
///     Represents an asynchronous delegate designed for creating and executing
///     test actions based on the provided TestContext. The delegate performs
///     actions asynchronously and is suited for scenarios requiring non-blocking
///     execution of test logic.
/// </summary>
public delegate Task TestActionAsync(TestContext context);
