using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Container;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Report;
using Agenix.Api.Variable;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.NUnitTestProject.Context;

public class TestContextTest : AbstractNUnitSetUp
{
    private GlobalVariables _globalVariables;

    [SetUp]
    public void SetUpTest()
    {
        _globalVariables = new GlobalVariables();
    }

    [Test]
    public void TestDefaultVariables()
    {
        // Set up global variables
        _globalVariables.GetVariables().Add("defaultVar", "123");

        // First test case
        var testCase = new DefaultTestCase();
        testCase.SetName("MyTestCase");

        var variableDefinitions = new Dictionary<string, object> { { "test1Var", "456" } };
        testCase.SetVariableDefinitions(variableDefinitions);

        var testContext = TestContextFactory.GetObject();
        testContext.SetGlobalVariables(_globalVariables);
        testCase.Execute(testContext);

        // Verify variables in the first test context
        Assert.That(testContext.GetVariables()[AgenixSettings.TestNameVariable()], Is.EqualTo("MyTestCase"));
        Assert.That(testContext.GetVariables()[AgenixSettings.TestNameSpaceVariable()],
            Is.EqualTo(typeof(DefaultTestCase).Namespace));
        Assert.That(testContext.GetVariables().ContainsKey("defaultVar"), Is.True);
        Assert.That(testContext.GetVariables()["defaultVar"], Is.EqualTo("123"));
        Assert.That(testContext.GetVariables().ContainsKey("test1Var"), Is.True);
        Assert.That(testContext.GetVariables()["test1Var"], Is.EqualTo("456"));

        // Second test case 
        var testCase2 = new DefaultTestCase();
        testCase2.SetName("MyTestCase2");
        testCase2.SetNamespaceName("YourNamespace.Framework");

        var variableDefinitions2 = new Dictionary<string, object> { { "test2Var", "456" } };
        testCase2.SetVariableDefinitions(variableDefinitions2);

        // Create a new test context for the second test case
        testContext = TestContextFactory.GetObject();
        testContext.SetGlobalVariables(_globalVariables);
        testCase2.Execute(testContext);

        // Verify variables in the second test context
        Assert.That(testContext.GetVariables()[AgenixSettings.TestNameVariable()], Is.EqualTo("MyTestCase2"));
        Assert.That(testContext.GetVariables()[AgenixSettings.TestNameSpaceVariable()],
            Is.EqualTo("YourNamespace.Framework"));
        Assert.That(testContext.GetVariables().ContainsKey("defaultVar"), Is.True);
        Assert.That(testContext.GetVariables()["defaultVar"], Is.EqualTo("123"));
        Assert.That(testContext.GetVariables().ContainsKey("test2Var"), Is.True);
        Assert.That(testContext.GetVariables()["test2Var"], Is.EqualTo("456"));
        Assert.That(testContext.GetVariables().ContainsKey("test1Var"), Is.False);
    }

    [Test]
    public void TestDefaultVariablesChange()
    {
        // Initialize global variables
        _globalVariables.GetVariables().Add("defaultVar", "123");

        // First test case
        var testCase = new DefaultTestCase();
        testCase.SetName("MyTestCase");

        // Create an action to change the variable
        var varSetting = new CreateVariablesAction.Builder()
            .Variable("defaultVar", "ABC")
            .Build();
        testCase.AddTestAction(varSetting);

        // Get test context and set global variables
        var testContext = TestContextFactory.GetObject();
        testContext.SetGlobalVariables(_globalVariables);

        // Execute the test case
        testCase.Execute(testContext);

        // Verify that the variable was changed in the first test context
        Assert.That(testContext.GetVariables().ContainsKey("defaultVar"), Is.True);
        Assert.That(testContext.GetVariables()["defaultVar"], Is.EqualTo("ABC"));

        // Second test case
        var testCase2 = new DefaultTestCase();
        testCase2.SetName("MyTestCase2");

        // Create a new test context for the second test
        testContext = TestContextFactory.GetObject();
        testContext.SetGlobalVariables(_globalVariables);
        testCase2.Execute(testContext);

        // Verify that the global variable is restored to original value in second test context
        Assert.That(testContext.GetVariables().ContainsKey("defaultVar"), Is.True);
        Assert.That(testContext.GetVariables()["defaultVar"], Is.EqualTo("123"));
    }

    [Test]
    public void TestGetVariable()
    {
        // Setup test variable
        Context.GetVariables().Add("test", "123");

        // Test retrieval with and without variable syntax
        Assert.That(Context.GetVariable("${test}"), Is.EqualTo("123"));
        Assert.That(Context.GetVariable("test"), Is.EqualTo("123"));
    }

    [Test]
    public void TestUnknownVariable()
    {
        // Setup test variable
        Context.GetVariables().Add("test", "123");

        // Test that the correct exception is thrown
        Assert.Throws<AgenixSystemException>(() => Context.GetVariable("${test_wrong}"));
    }

    [Test]
    public void TestGetVariableFromJsonPathExpression()
    {
        // Setup test data
        var json = "{\"name\": \"Peter\"}";
        Context.SetVariable("jsonVar", json);

        const string variableExpression = "jsonVar.jsonPath($.name)";

        // Create a mock for the segment variable extractor
        var jsonExtractorMock = new Mock<ISegmentVariableExtractor>();
        Context.SegmentVariableExtractorRegistry.SegmentValueExtractors.Add(jsonExtractorMock.Object);

        // Configure the mock behavior
        jsonExtractorMock
            .Setup(x => x.CanExtract(
                It.Is<TestContext>(c => c == Context),
                It.Is<object>(o => o.Equals(json)),
                It.IsAny<VariableExpressionSegmentMatcher>()))
            .Returns(true);

        jsonExtractorMock
            .Setup(x => x.ExtractValue(
                It.Is<TestContext>(c => c == Context),
                It.Is<object>(o => o.Equals(json)),
                It.IsAny<VariableExpressionSegmentMatcher>()))
            .Returns("Peter");

        // Assert that the variable extraction works correctly
        Assert.That(Context.GetVariable($"${{{variableExpression}}}"), Is.EqualTo("Peter"));
    }

    [Test]
    public void TestGetVariableFromJsonPathExpressionNoMatch()
    {
        // Setup test data
        const string json = "{\"name\": \"Peter\"}";
        Context.SetVariable("jsonVar", json);

        const string variableExpression = "jsonVar.jsonPath($.othername)";

        // Create a mock for the segment variable extractor
        var jsonExtractorMock = new Mock<ISegmentVariableExtractor>();
        Context.SegmentVariableExtractorRegistry.SegmentValueExtractors.Add(jsonExtractorMock.Object);

        // Configure the mock behavior
        jsonExtractorMock
            .Setup(x => x.CanExtract(
                It.Is<TestContext>(c => c == Context),
                It.Is<object>(o => o.Equals(json)),
                It.IsAny<VariableExpressionSegmentMatcher>()))
            .Returns(true);

        jsonExtractorMock
            .Setup(x => x.ExtractValue(
                It.Is<TestContext>(c => c == Context),
                It.Is<object>(o => o.Equals(json)),
                It.IsAny<VariableExpressionSegmentMatcher>()))
            .Throws<AgenixSystemException>();

        // Assert that the variable extraction throws the expected exception
        Assert.Throws<AgenixSystemException>(() =>
            Context.GetVariable($"${{{variableExpression}}}"));
    }

    [Test]
    public void TestUnknownFromPathExpression()
    {
        // Setup test data with nested DataContainer objects
        Context.SetVariable("helloData", new DataContainer("hello"));
        Context.SetVariable("container", new DataContainer(new DataContainer("nested")));

        // Test various path expressions that should all throw exceptions

        // Unknown field on a data container
        Assert.Throws<AgenixSystemException>(() =>
            Context.GetVariable("${helloData.unknown}"));

        // Unknown field on a nested data container
        Assert.Throws<AgenixSystemException>(() =>
            Context.GetVariable("${container.data.unknown}"));

        // Completely unknown variable name
        Assert.Throws<AgenixSystemException>(() =>
            Context.GetVariable("${something.else}"));

        // Invalid array index access on a non-array field
        Assert.Throws<AgenixSystemException>(() =>
            Context.GetVariable("${helloData[1]}"));
    }

    [Test]
    public void TestGetVariableFromPathExpression()
    {
        // Setup basic test data
        Context.SetVariable("helloData", new DataContainer("hello"));
        Context.SetVariable("container", new DataContainer(new DataContainer("nested")));

        // Setup array of nested data containers
        var subContainerArray = new DataContainer[]
        {
            new("A"),
            new("B"),
            new("C"),
            new("D")
        };

        var containerArray = new DataContainer[]
        {
            new("0"),
            new("1"),
            new("2"),
            new(subContainerArray)
        };

        Context.SetVariable("containerArray", containerArray);

        // Test direct object access
        Assert.That(Context.GetVariable("${helloData}"), Is.EqualTo(typeof(DataContainer).FullName));

        // Test property access
        Assert.That(Context.GetVariable("${helloData.data}"), Is.EqualTo("hello"));
        Assert.That(Context.GetVariable("${helloData.number}"), Is.EqualTo("99"));

        // Test constant access
        Assert.That(Context.GetVariable("${helloData.Constant}"), Is.EqualTo("FOO"));

        // Test nested object access
        Assert.That(Context.GetVariable("${container.data}"), Is.EqualTo(typeof(DataContainer).FullName));
        Assert.That(Context.GetVariable("${container.data.data}"), Is.EqualTo("nested"));
        Assert.That(Context.GetVariable("${container.data.number}"), Is.EqualTo("99"));
        Assert.That(Context.GetVariable("${container.data.Constant}"), Is.EqualTo("FOO"));

        // Test array element access
        Assert.That(Context.GetVariable("${container.intVals[1]}"), Is.EqualTo("1"));

        // Test deep nested array and property access
        Assert.That(Context.GetVariable("${containerArray[3].data[1].data}"), Is.EqualTo("B"));
    }

    [Test]
    public void TestReplaceDynamicContentInString()
    {
        // Setup test variable
        Context.GetVariables()["test"] = "456";

        // Test variable replacement (without quoting)
        Assert.That(Context.ReplaceDynamicContentInString("Variable test is: ${test}"),
            Is.EqualTo("Variable test is: 456"));
        Assert.That(Context.ReplaceDynamicContentInString("${test} is the value of variable test"),
            Is.EqualTo("456 is the value of variable test"));
        Assert.That(Context.ReplaceDynamicContentInString("123${test}789"), Is.EqualTo("123456789"));

        // Test static strings (without quoting)
        Assert.That(Context.ReplaceDynamicContentInString("Hello TestFramework!"), Is.EqualTo("Hello TestFramework!"));

        // Test function calls (without quoting)
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('Hello', ' TestFramework!')"),
            Is.EqualTo("Hello TestFramework!"));
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('agenix', ':agenix')"),
            Is.EqualTo("agenix:agenix"));
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('agenix:agenix')"), Is.EqualTo("agenix:agenix"));

        // Test variable replacement with quoting enabled
        Assert.That(Context.ReplaceDynamicContentInString("Variable test is: ${test}", true),
            Is.EqualTo("Variable test is: '456'"));
        Assert.That(Context.ReplaceDynamicContentInString("${test} is the value of variable test", true),
            Is.EqualTo("'456' is the value of variable test"));
        Assert.That(Context.ReplaceDynamicContentInString("123${test}789", true), Is.EqualTo("123'456'789"));

        // Test static strings with quoting enabled
        Assert.That(Context.ReplaceDynamicContentInString("Hello TestFramework!", true),
            Is.EqualTo("Hello TestFramework!"));

        // Test function calls with quoting enabled
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('Hello', ' TestFramework!')", true),
            Is.EqualTo("'Hello TestFramework!'"));

        // Repeated tests for basic cases (without quoting)
        Assert.That(Context.ReplaceDynamicContentInString("Hello TestFramework!"), Is.EqualTo("Hello TestFramework!"));
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('Hello', ' TestFramework!')"),
            Is.EqualTo("Hello TestFramework!"));

        // Repeated tests for basic cases (with quoting)
        Assert.That(Context.ReplaceDynamicContentInString("Hello TestFramework!", true),
            Is.EqualTo("Hello TestFramework!"));
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('Hello', ' TestFramework!')", true),
            Is.EqualTo("'Hello TestFramework!'"));

        // Tests with spacing
        Assert.That(Context.ReplaceDynamicContentInString("123 ${test}789"), Is.EqualTo("123 456789"));
        Assert.That(Context.ReplaceDynamicContentInString("123 ${test}789", true), Is.EqualTo("123 '456'789"));
    }

    [Test]
    public void TestVariableExpressionEscaped()
    {
        // Test basic variable escaping
        Assert.That(Context.ReplaceDynamicContentInString("${//escaped//}"), Is.EqualTo("${escaped}"));

        // Test function with escaped variable
        Assert.That(Context.ReplaceDynamicContentInString("core:Concat('${////escaped////}', ' That is ok!')"),
            Is.EqualTo("${escaped} That is ok!"));

        // Set up variables to test path-like variables and escaping
        Context.SetVariable("/value/", "123");
        Context.SetVariable("value", "456");

        // Test accessing variable with slashes in name
        Assert.That(Context.ReplaceDynamicContentInString("${/value/}"), Is.EqualTo("123"));

        // Test escaped variable to prevent resolution
        Assert.That(Context.ReplaceDynamicContentInString("${//value//}"), Is.EqualTo("${value}"));
    }

    [Test]
    public void TestSetVariable()
    {
        // Arrange
        Context.SetVariable("${test1}", "123");
        Context.SetVariable("${test2}", "");

        // Assert
        Assert.That(Context.GetVariable("test1"), Is.EqualTo("123"));
        Assert.That(Context.GetVariable("test2"), Is.EqualTo(""));
    }

    [Test]
    public void TestFailSetVariableNoName()
    {
        // Assert
        Assert.That(() => Context.SetVariable("", "123"),
            Throws.TypeOf<AgenixSystemException>());
    }

    [Test]
    public void TestFailSetVariableNoValue()
    {
        // Assert
        Assert.That(() => Context.SetVariable("${test}", null),
            Throws.TypeOf<VariableNullValueException>());
    }

    [Test]
    public void TestAddVariables()
    {
        // Arrange
        var vars = new Dictionary<string, object>
        {
            { "${test1}", "123" },
            { "${test2}", "" }
        };

        // Act
        Context.AddVariables(vars);

        // Assert
        Assert.That(Context.GetVariable("test1"), Is.EqualTo("123"));
        Assert.That(Context.GetVariable("test2"), Is.EqualTo(""));
    }

    [Test]
    public void TestAddVariablesFromArrays()
    {
        //GIVEN
        string[] variableNames = { "variable1", "${variable2}" };
        object[] variableValues = { "value1", "" };

        //WHEN
        Context.AddVariables(variableNames, variableValues);

        //THEN
        Assert.That(Context.GetVariable("variable1"), Is.EqualTo("value1"));
        Assert.That(Context.GetVariable("variable2"), Is.EqualTo(""));
    }

    [Test]
    public void TestAddVariablesThrowsExceptionIfArraysHaveDifferentSize()
    {
        //GIVEN
        string[] variableNames = { "variable1", "variable2" };
        object[] variableValues = { "value1" };

        //WHEN + THEN
        Assert.That(() => Context.AddVariables(variableNames, variableValues),
            Throws.TypeOf<AgenixSystemException>());
    }

    [Test]
    public void TestReplaceVariablesInMap()
    {
        //GIVEN
        Context.GetVariables().Add("test", "123");

        var testMap = new Dictionary<string, object>();
        testMap.Add("plainText", "Hello TestFramework!");
        testMap.Add("value", "${test}");

        //WHEN
        testMap = Context.ResolveDynamicValuesInMap(testMap);

        //THEN
        Assert.That(testMap["value"], Is.EqualTo("123"));

        //WHEN
        testMap.Clear();
        testMap.Add("value", "test");
        testMap = Context.ResolveDynamicValuesInMap(testMap);

        //THEN
        Assert.That(testMap["value"], Is.EqualTo("test"));

        //WHEN
        testMap.Clear();
        testMap.Add("${test}", "value");
        testMap = Context.ResolveDynamicValuesInMap(testMap);

        //THEN
        // Should be null due to variable substitution
        Assert.That(testMap.ContainsKey("${test}"), Is.False);
        // Should return "test" after variable substitution
        Assert.That(testMap["123"], Is.EqualTo("value"));
    }

    [Test]
    public void TestReplaceVariablesInList()
    {
        //GIVEN
        Context.GetVariables().Add("test", "123");

        var testList = new List<string>();
        testList.Add("Hello TestFramework!");
        testList.Add("${test}");
        testList.Add("test");

        //WHEN
        var replaceValues = Context.ResolveDynamicValuesInList(testList);

        //THEN
        Assert.That(replaceValues[0], Is.EqualTo("Hello TestFramework!"));
        Assert.That(replaceValues[1], Is.EqualTo("123"));
        Assert.That(replaceValues[2], Is.EqualTo("test"));
    }

    [Test]
    public void TestReplaceVariablesInArray()
    {
        //GIVEN
        Context.GetVariables().Add("test", "123");

        string[] testArray = { "Hello TestFramework!", "${test}", "test" };

        //WHEN
        var replaceValues = Context.ResolveDynamicValuesInArray(testArray);

        //THEN
        Assert.That(replaceValues[0], Is.EqualTo("Hello TestFramework!"));
        Assert.That(replaceValues[1], Is.EqualTo("123"));
        Assert.That(replaceValues[2], Is.EqualTo("test"));
    }

    [Test]
    public void TestResolveDynamicValue()
    {
        //GIVEN
        Context.GetVariables().Add("test", "testtesttest");

        //WHEN/THEN
        Assert.That(Context.ResolveDynamicValue("${test}"), Is.EqualTo("testtesttest"));
        Assert.That(Context.ResolveDynamicValue("core:Concat('Hello', ' TestFramework!')"),
            Is.EqualTo("Hello TestFramework!"));
        Assert.That(Context.ResolveDynamicValue("nonDynamicValue"), Is.EqualTo("nonDynamicValue"));
    }

    [Test]
    public void ShouldCallMessageListeners()
    {
        //GIVEN
        var listeners = new Mock<MessageListeners>();
        Context.MessageListeners = listeners.Object;

        var inbound = new DefaultMessage("INBOUND");
        var outbound = new DefaultMessage("OUTBOUND");

        //WHEN
        Context.OnInboundMessage(inbound);
        Context.OnOutboundMessage(outbound);

        //THEN
        listeners.Verify(l => l.OnInboundMessage(It.Is<IMessage>(m => m == inbound), Context));
        listeners.Verify(l => l.OnOutboundMessage(It.Is<IMessage>(m => m == outbound), Context));
    }

    [Test]
    public void TestRegisterAndStopTimers()
    {
        //GIVEN
        const string timerId = "t1";
        var timer = new Mock<IStopTimer>();

        //WHEN/THEN
        Context.RegisterTimer(timerId, timer.Object);

        //Verify duplicate registration throws an exception
        Assert.That(() => Context.RegisterTimer(timerId, timer.Object),
            Throws.Exception
                .With.Message.Contains("Timer already registered with this id"));

        //Verify timer operations
        Assert.That(Context.StopTimer(timerId), Is.True);
        Assert.That(Context.StopTimer("?????"), Is.False);

        Context.StopTimers();

        //Verify timer was stopped twice - once by StopTimer() and once by StopTimers()
        timer.Verify(t => t.StopTimer(), Times.Exactly(2));
    }


    /// <summary>
    ///     Data container for test variable object access.
    /// </summary>
    private class DataContainer
    {
        private readonly string Constant = "FOO";
        private readonly object data;

        private readonly int[] intVals = [0, 1, 2, 3, 4];
        private readonly int number = 99;


        /// <summary>
        ///     Constructor with data.
        /// </summary>
        /// <param name="data">The data to store in the container</param>
        public DataContainer(object data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            return typeof(DataContainer).FullName;
        }
    }
}