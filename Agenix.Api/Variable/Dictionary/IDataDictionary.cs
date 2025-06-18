using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core;

namespace Agenix.Api.Variable.Dictionary;

public interface IDataDictionary : IMessageProcessor

{
    // Common non-generic methods
}

/// <summary>
///     Data dictionary interface describes a mechanism to modify message content (payload) with global dictionary
///     elements.
///     Dictionary translates element values to those defined in the dictionary.
///     <para>
///         Dictionary takes part in message construction for inbound and outbound messages in agenix.
///     </para>
/// </summary>
/// <typeparam name="T">The type of the key element</typeparam>
/// <since>1.4</since>
public interface IDataDictionary<T> : IMessageProcessor, IMessageDirectionAware, IScoped, InitializingPhase,
    IDataDictionary
{
    /// <summary>
    ///     Gets the data dictionary name.
    /// </summary>
    /// <returns>The dictionary name</returns>
    string Name { get; }

    /// <summary>
    ///     Gets the path mapping strategy.
    /// </summary>
    PathMappingStrategy PathMappingStrategy { get; }

    /// <summary>
    ///     Translate value with given path in message content.
    /// </summary>
    /// <typeparam name="R">The type of the value to translate</typeparam>
    /// <param name="key">The key element in message content</param>
    /// <param name="value">Current value</param>
    /// <param name="context">The current test context</param>
    /// <returns>Translated value</returns>
    R Translate<R>(T key, R value, TestContext context);
}

/// <summary>
///     Possible mapping strategies for identifying matching dictionary items
///     with path comparison.
/// </summary>
public enum PathMappingStrategy
{
    EXACT,
    ENDS_WITH,
    STARTS_WITH
}
