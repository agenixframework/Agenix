namespace Agenix.Core;

/// Default implementation of AgenixContext provider. It facilitates the generation of
/// AgenixContext instances according to a specified creation strategy. The provider
/// can generate a new context instance upon each request or return a consistent,
/// singleton instance, as defined by the AgenixInstanceStrategy.
/// /
public class DefaultAgenixContextProvider(AgenixInstanceStrategy strategy) : IAgenixContextProvider
{
    public static string Spring = "spring";

    private static AgenixContext _context;

    public DefaultAgenixContextProvider() : this(AgenixInstanceStrategy.SINGLETON)
    {
    }

    /// <summary>
    ///     Creates an instance of the <see cref="AgenixContext" /> based on the current strategy.
    ///     If the strategy is set to NEW, or the context has not been initialized, a new instance
    ///     of <see cref="AgenixContext" /> is created. Otherwise, the existing context is returned.
    /// </summary>
    /// <returns>
    ///     An instance of the <see cref="AgenixContext" />. Depending on the strategy,
    ///     it may either be a new instance or a reused existing instance.
    /// </returns>
    public AgenixContext Create()
    {
        if (strategy.Equals(AgenixInstanceStrategy.NEW) || _context == null) _context = AgenixContext.Create();

        return _context;
    }
}