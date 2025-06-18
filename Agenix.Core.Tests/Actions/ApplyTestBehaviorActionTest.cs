using Agenix.Api;
using Agenix.Core.Actions;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Actions;

public class ApplyTestBehaviorActionTest : AbstractNUnitSetUp
{
    private Mock<ITestAction> _mock;
    private Mock<ITestActionRunner> _runner;

    [SetUp]
    public void SetupMock()
    {
        _runner = new Mock<ITestActionRunner>();
        _mock = new Mock<ITestAction>();
    }

    [Test]
    public void ShouldApply()
    {
        var applyBehavior = new ApplyTestBehaviorAction.Builder()
            .Behavior(runner => runner.Run(_mock.Object))
            .On(_runner.Object)
            .Build();

        applyBehavior.Execute(Context);

        _runner.Verify(r => r.Run(_mock.Object));
    }
}
