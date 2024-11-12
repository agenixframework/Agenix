using Agenix.Core.Common;

namespace Agenix.Core.Actions;

/// <summary>
///     Abstract base class for test actions. Class provides a default name and description.
/// </summary>
public abstract class AbstractTestAction : ITestAction, INamed, IDescribed
{
    /// <summary>
    ///     Describing the test action
    /// </summary>
    protected string description;

    public AbstractTestAction()
    {
        Name = GetType().Name;
    }

    public AbstractTestAction(string name,
        AbstractTestActionBuilder<ITestAction, ITestActionBuilder<ITestAction>> builder)
    {
        Name = builder.GetName() ?? name;
        description = builder.GetDescription();
    }

    public string GetDescription()
    {
        return description;
    }

    public ITestAction SetDescription(string description)
    {
        this.description = description;
        return this;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     TestAction name injected as spring bean name
    /// </summary>
    public string Name { get; private set; }

    public virtual bool IsDisabled(TestContext context)
    {
        return false;
    }

    /// <summary>
    ///     Do basic logging and delegate execution to subclass.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Execute(TestContext context)
    {
        if (!IsDisabled(context)) DoExecute(context);
    }

    /// <summary>
    ///     Subclasses may add custom execution logic here.
    /// </summary>
    /// <param name="context"></param>
    public abstract void DoExecute(TestContext context);
}