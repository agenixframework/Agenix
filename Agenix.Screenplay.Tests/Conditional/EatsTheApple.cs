namespace Agenix.Screenplay.Tests.Conditional;

/// <summary>
///     Represents an action performed by an actor where an apple is consumed.
/// </summary>
/// <remarks>
///     This class is designed to be used within a screenplay-like testing framework,
///     implementing the <see cref="IPerformable" /> interface to define a task that can be executed by an actor.
/// </remarks>
public class EatsTheApple : IPerformable
{
    public readonly Apple Apple;

    /// <summary>
    ///     Represents an action in which an actor consumes an apple as part of a screenplay-like pattern.
    /// </summary>
    public EatsTheApple() { }

    public EatsTheApple(Apple apple)
    {
        Apple = apple;
    }

    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        Apple.Eat();
        return Task.CompletedTask;
    }
}
