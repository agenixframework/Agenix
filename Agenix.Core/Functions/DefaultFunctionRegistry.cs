using Agenix.Api.Functions;

namespace Agenix.Core.Functions;

/// <summary>
///     Default registry automatically adds a default function library.
/// </summary>
public class DefaultFunctionRegistry : FunctionRegistry
{
    /// <summary>
    ///     Constructor initializes with a default function library.
    /// </summary>
    public DefaultFunctionRegistry()
    {
        AddFunctionLibrary(new DefaultFunctionLibrary());
    }
}