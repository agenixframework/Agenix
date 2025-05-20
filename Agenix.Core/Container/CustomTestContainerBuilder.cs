using Agenix.Api;
using Agenix.Api.Container;
using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// <summary>
///     Represents a builder pattern class for creating instances of test action containers.
///     This class extends the <see cref="AbstractTestContainerBuilder{T, dynamic}" /> and implements
///     the <see cref="ITestAction" /> interface.
/// </summary>
/// <typeparam name="T">The type of test action container that implements <see cref="ITestActionContainer" />.</typeparam>
/// <remarks>
///     This class builds a test action container of type <typeparamref name="T" /> by providing concrete
///     implementations for building and executing test actions within the container. It checks for existing actions
///     and decides the container's build path accordingly.
/// </remarks>
public class CustomTestContainerBuilder<T>(T container) : AbstractTestContainerBuilder<T, dynamic>, ITestAction
    where T : ITestActionContainer
{
    private readonly T _container = container;

    public void Execute(TestContext context)
    {
        base.Build().Execute(context);
    }

    protected override T DoBuild()
    {
        return _container;
    }

    public override T Build()
    {
        if (_container.GetActions().Count > 0) return _container;

        return base.Build();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="CustomTestContainerBuilder{C}" /> using the specified container.
    /// </summary>
    /// <param name="container">An instance of a container type that implements <see cref="ITestActionContainer" />.</param>
    /// <typeparam name="C">The type of the container used, which implements <see cref="ITestActionContainer" />.</typeparam>
    /// <returns>
    ///     A new instance of <see cref="AbstractTestContainerBuilder{C, dynamic}" /> initialized with the specified
    ///     container.
    /// </returns>
    public static AbstractTestContainerBuilder<C, dynamic> Container<C>(C container)
        where C : ITestActionContainer
    {
        return new CustomTestContainerBuilder<C>(container);
    }
}