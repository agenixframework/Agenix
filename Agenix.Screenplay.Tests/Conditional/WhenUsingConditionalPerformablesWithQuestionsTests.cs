using System.Threading.Tasks;
using Agenix.Screenplay.Conditions;
using Agenix.Screenplay.Questions;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Conditional;

[TestFixture]
public class WhenUsingConditionalPerformablesWithQuestionsTests
{
    private Apple _apple = null!;

    [SetUp]
    public void SetUp()
    {
        _apple = new Apple();
    }

    [Test]
    public async Task Should_execute_task_when_boolean_question_evaluates_true()
    {
        var eddie = Actor.Named("Eddie");
        var eatsTheApple = new EatsTheApple(_apple);
        var question = new Question<bool>(_ => true);

        await eddie.AttemptsTo(Check.Whether(question).AndIfSo(eatsTheApple));

        Assert.That(_apple.IsEaten(), Is.True);
    }

    [Test]
    public async Task Should_execute_otherwise_when_boolean_question_evaluates_false()
    {
        var eddie = Actor.Named("Eddie");
        var eatsTheApple = new EatsTheApple(_apple);
        var question = new Question<bool>(_ => false);

        await eddie.AttemptsTo(Check.Whether(question).Otherwise(eatsTheApple));

        Assert.That(_apple.IsEaten(), Is.True);
    }
}
