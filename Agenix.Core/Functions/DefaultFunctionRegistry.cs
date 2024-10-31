namespace Agenix.Core.Functions;

/// <summary>
///     Default registry automatically adds default function library.
/// </summary>
public class DefaultFunctionRegistry : FunctionRegistry
{
    /// <summary>
    ///     Constructor initializes with default function library.
    /// </summary>
    public DefaultFunctionRegistry()
    {
        AddFunctionLibrary(new DefaultFunctionLibrary());
    }
}