namespace Agenix.Core;

/// Abstract base class for building test actions.
/// @typeparam T The type of test action being built.
/// @typeparam TS The type of the builder implementation.
/// /
public abstract class AbstractTestActionBuilder<T, TS> : ITestActionBuilder<T>
    where T : ITestAction
    where TS : ITestActionBuilder<T>
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

    /**
     * Sets the test action name.
     * @param name the test action name.
     * @return
     */
    public TS Name(string name)
    {
        this.name = name;
        return self;
    }

    /**
     * Sets the description.
     * @param description
     * @return
     */
    public TS Description(string description)
    {
        this.description = description;
        return self;
    }

    /**
     * Obtains the name.
     * @return
     */
    public string GetName()
    {
        return name;
    }

    /**
     * Obtains the description.
     * @return
     */
    public string GetDescription()
    {
        return description;
    }
}