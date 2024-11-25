namespace Agenix.Core;

public interface IScoped
{
    /// <summary>
    ///     Marks component to be used in global scope. Global scoped components get added once on a project level and
    ///     automatically get applied to each processing of messages, actions, data and so on.
    /// </summary>
    /// <returns></returns>
    bool IsGlobalScope();
}