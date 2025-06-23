using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.TestCase;

public class TestCaseRunnerFactoryTest
{
    [Test]
    public void TestDefaultRunnerWithGivenContext()
    {
        var testContext = new TestContext();
        var runner = TestCaseRunnerFactory.CreateRunner(testContext);
        ClassicAssert.AreEqual(typeof(DefaultTestCaseRunner), runner.GetType());

        var defaultTestCaseRunner = (DefaultTestCaseRunner)runner;
        ClassicAssert.AreEqual(testContext, defaultTestCaseRunner.Context);
        ClassicAssert.IsInstanceOf<DefaultTestCase>(defaultTestCaseRunner.GetTestCase());
    }

    [Test]
    public void TestDefaultRunnerWithGivenTestCaseAndContext()
    {
        var testContext = new TestContext();
        var testCase = new DefaultTestCase();

        var runner = TestCaseRunnerFactory.CreateRunner(testCase, testContext);
        ClassicAssert.AreEqual(typeof(DefaultTestCaseRunner), runner.GetType());

        var defaultTestCaseRunner = (DefaultTestCaseRunner)runner;
        ClassicAssert.AreEqual(testContext, defaultTestCaseRunner.Context);
        ClassicAssert.AreEqual(testCase, defaultTestCaseRunner.GetTestCase());
    }
}
