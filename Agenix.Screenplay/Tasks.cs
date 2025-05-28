using Agenix.Api.Util;

namespace Agenix.Screenplay;

/// <summary>
/// Provides utility methods to create and manage task instances that implement the IPerformable interface.
/// </summary>
public class Tasks
{
    /// <summary>
    /// Creates an instance of a specified step class, ensuring that the instance implements the IPerformable interface.
    /// </summary>
    /// <typeparam name="T">The type that implements the IPerformable interface.</typeparam>
    /// <param name="stepClass">The type of the step class to instantiate.</param>
    /// <param name="parameters">An array of parameters required to initialize the step class.</param>
    /// <returns>An instance of the specified step class cast to the type T.</returns>
    public static T Instrumented<T>(params object[] parameters) where T : IPerformable
    {
        return DynamicConstructorBuilder.CreateInstance<T>(parameters);
    }
}