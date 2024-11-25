using System;

namespace Agenix.Core.Annotations;

/// <summary>
///     Specifies that a field or parameter is a resource within the Agenix framework.
/// </summary>
/// <remarks>
///     The attribute can be applied to both fields and parameters, but it is not inherited by derived classes.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class AgenixResourceAttribute : Attribute
{
}