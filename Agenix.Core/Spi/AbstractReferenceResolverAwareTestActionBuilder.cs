namespace Agenix.Core.Spi;

/// <summary>
/// Abstract base class for building test actions with reference resolver capabilities.
/// This class is intended to be inherited by other test action builders that require
/// access to a reference resolver to resolve dependencies or references during their construction or execution.
/// </summary>
/// <typeparam name="T">The type of the test action that this builder creates.</typeparam>
/// <remarks>
/// This class implements both <see cref="ITestActionBuilder{T}.IDelegatingTestActionBuilder{T}"/> and <see cref="IReferenceResolverAware"/>.
/// It facilitates delegation to another implementation of <see cref="ITestActionBuilder{T}"/> and seamlessly integrates with a reference resolver
/// through the <see cref="SetReferenceResolver"/> method.
/// </remarks>
/// <example>
/// This class should be extended by concrete implementations to provide the logic for constructing specific types of test actions.
/// </example>
public abstract class AbstractReferenceResolverAwareTestActionBuilder<T> : ITestActionBuilder<T>.IDelegatingTestActionBuilder<T>, IReferenceResolverAware where T : ITestAction
{
    protected IReferenceResolver referenceResolver;

    protected ITestActionBuilder<T> _delegate;

    public ITestActionBuilder<T> Delegate => _delegate;
    
    public void SetReferenceResolver(IReferenceResolver newReferenceResolver)
    {
        if (referenceResolver != null)
        {
            referenceResolver = newReferenceResolver;

            if (Delegate is IReferenceResolverAware referenceResolverAware)
            {
                referenceResolverAware.SetReferenceResolver(referenceResolver);
            }
        }
    }

    public abstract T Build();
}