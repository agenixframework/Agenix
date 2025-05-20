using Agenix.Api;

namespace Agenix.Core;

/// Abstract base class for building test actions.
/// @typeparam T The type of test action being built.
/// @typeparam TS The type of the builder implementation.
/// /
public abstract class AbstractTestActionBuilder<T, TS> : ITestActionBuilder<T>
    where T : ITestAction
    where TS : class
{
    private string description;

    private string name;
    protected TS self;

    protected AbstractTestActionBuilder()
    {
        self = (TS)(object)this;
    }

    /// Builds the test action.
    /// @return The build instance of the test action.
    /// /
    public abstract T Build();

    /// Sets the test action name.
    /// <param name="name">The test action name.</param>
    /// <return>The builder instance with the updated name.</return>
    public TS Name(string name)
    {
        this.name = name;
        return self;
    }

    /// Sets the description for the test action.
    /// @param description the description of the test action.
    /// @return The builder instance with the updated description.
    public TS Description(string description)
    {
        this.description = description;
        return self;
    }

    /// Obtains the name of the test action.
    /// @return The name of the test action.
    public string GetName()
    {
        return name;
    }

    /// Obtains the description.
    /// @return The description of the test action.
    public string GetDescription()
    {
        return description;
    }
}