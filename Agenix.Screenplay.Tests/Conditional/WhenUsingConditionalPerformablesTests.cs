using Agenix.Screenplay.Conditions;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Conditional;

[TestFixture]
public class WhenUsingConditionalPerformables
{
    [SetUp]
    public void SetUp()
    {
        _apple = new Apple();
    }

    private Apple _apple;

    [Test]
    public async Task AConditionalWithABooleanExpression()
    {
        var eddie = Actor.Named("Eddie");
        var eatsTheApple = new EatsTheApple(_apple);

        await eddie.AttemptsTo(Check.Whether(true).AndIfSo(eatsTheApple));

        Assert.That(_apple.IsEaten(), Is.True);
    }

    [Test]
    public async Task TaskShouldNotBeExecutedIfTheConditionIsNotTrue()
    {
        var eddie = Actor.Named("Eddie");
        var eatsTheApple = new EatsTheApple(_apple);

        await eddie.AttemptsTo(Check.Whether(false).AndIfSo(eatsTheApple));

        Assert.That(_apple.IsEaten(), Is.False);
    }
}
