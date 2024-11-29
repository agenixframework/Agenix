namespace Agenix.Core.Condition;

/// <summary>
///     Represents an abstract base class for defining specific conditions.
///     This class provides an implementation of the ICondition interface,
///     managing the name of the condition and requiring derived classes to
///     implement the core condition functionalities.
/// </summary>
public abstract class AbstractCondition : ICondition
{
    /// The condition name
    private readonly string _name;

    /// Represents an abstract condition that serves as a base class for specific condition implementations.
    /// Implements the ICondition interface and provides functionality for condition name management.
    /// /
    public AbstractCondition()
    {
        _name = GetType().Name;
    }

    /// Represents an abstract condition that implements the ICondition interface.
    /// Provides basic functionality for retrieving the name of the condition.
    /// /
    public AbstractCondition(string name)
    {
        _name = name;
    }

    public string GetName()
    {
        return _name;
    }

    public abstract bool IsSatisfied(TestContext context);
    public abstract string GetSuccessMessage(TestContext context);
    public abstract string GetErrorMessage(TestContext context);
}