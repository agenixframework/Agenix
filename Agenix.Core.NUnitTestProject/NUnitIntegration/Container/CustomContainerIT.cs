using Agenix.Core.Annotations;
using Agenix.Core.Container;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class CustomContainerIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void ShouldExecuteReverseContainer()
    {
        _gherkin.When(Reverse().Actions(
            Echo("${text}"),
            Echo("Does it work"),
            CreateVariable("text", "Hello Agenix!")
        ));
    }

    /// <summary>
    ///     Creates and initializes a new instance of a container in which actions can be executed in reverse order.
    /// </summary>
    /// <returns>A builder object for the ReverseActionContainer, allowing for further configuration and execution of actions.</returns>
    private AbstractTestContainerBuilder<ReverseActionContainer, dynamic> Reverse()
    {
        return CustomTestContainerBuilder<ITestActionContainer>.Container(new ReverseActionContainer());
    }

    /// <summary>
    ///     ReverseActionContainer is a specialized implementation of the AbstractActionContainer that executes actions in
    ///     reverse order.
    /// </summary>
    /// <remarks>
    ///     This container overrides the DoExecute method to iterate through the list of actions in reverse, ensuring that the
    ///     last action
    ///     added is executed first and the first action added is executed last.
    /// </remarks>
    public class ReverseActionContainer : AbstractActionContainer
    {
        /// <summary>
        ///     Executes the list of actions in reverse order, where the last added action is executed first.
        /// </summary>
        /// <param name="context">The context in which the actions are to be executed, providing necessary runtime information.</param>
        public override void DoExecute(TestContext context)
        {
            for (var i = GetActions().Count; i > 0; i--) ExecuteAction(GetActions()[i - 1], context);
        }
    }
}