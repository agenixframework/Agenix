using Agenix.Core.Actions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Actions;

/// <summary>
///     Unit tests for the CreateVariablesAction class.
/// </summary>
/// <remarks>
///     This class contains several test cases to validate the behavior of the CreateVariablesAction class,
///     specifically focusing on the creation, retrieval, and overwriting of variables within a TestContext.
/// </remarks>
public class CreateVariablesActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestCreateSingleVariable()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "value")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("value", Context.GetVariable("${myVariable}"));
    }

    [Test]
    public void TestCreateVariables()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "value1")
            .Variable("anotherVariable", "value2")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("value1", Context.GetVariable("${myVariable}"));
        ClassicAssert.NotNull(Context.GetVariable("${anotherVariable}"));
        ClassicAssert.AreEqual("value2", Context.GetVariable("${anotherVariable}"));
    }

    [Test]
    public void TestOverwriteVariables()
    {
        Context.SetVariable("myVariable", "initialValue");

        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "newValue")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("newValue", Context.GetVariable("${myVariable}"));
    }

    [Test]
    public void TestCreateSingleVariableWithFunctionValue()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "core:Concat('Hello ', 'Agenix')")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("Hello Agenix", Context.GetVariable("${myVariable}"));
    }
}