namespace Agenix.Core;

/// <summary>
///     Test action builder.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITestActionBuilder<out T> where T : ITestAction
{
    /// <summary>
    ///     Builds new test action instance.
    /// </summary>
    /// <returns>the built test action.</returns>
    T Build();


    public interface IDelegatingTestActionBuilder<out TU> : ITestActionBuilder<TU> where TU : ITestAction
    {
        /// <summary>
        ///     Obtains the delegate test action builder.
        /// </summary>
        ITestActionBuilder<TU> Delegate { get; }
    }
}