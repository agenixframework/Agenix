namespace Agenix.Core.Spi;

/// An interface representing a component that is aware of a reference resolver.
/// Classes implementing this interface are expected to use a reference resolver
/// to resolve other components or services.
public interface IReferenceResolverAware
{
    /// Sets the reference resolver instance.
    /// <param name="referenceResolver">The reference resolver to be set.</param>
    void SetReferenceResolver(IReferenceResolver referenceResolver);
}