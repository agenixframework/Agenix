namespace Agenix.Api.Annotations;

/// <summary>
///     Represents an attribute used to specify configuration classes for a given target.
/// </summary>
/// <remarks>
///     This attribute can be applied to classes or interfaces to indicate the associated configuration classes.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class AgenixConfigurationAttribute : Attribute
{
    /// <summary>
    ///     Classes for the configuration.
    /// </summary>
    public Type[] Classes { get; set; } = Array.Empty<Type>();
}