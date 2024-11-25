using System;

namespace Agenix.Core.Annotations;

/// <summary>
///     The AgenixFrameworkAttribute is used to mark fields or parameters
///     that are part of the Agenix framework. This attribute helps in
///     identifying and managing Agenix framework specific components
///     throughout the application.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class AgenixFrameworkAttribute : Attribute
{
}