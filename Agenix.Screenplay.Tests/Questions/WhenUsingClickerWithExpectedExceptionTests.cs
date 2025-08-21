using NUnit.Framework;
using static Agenix.Screenplay.GivenWhenThen;
using static NHamcrest.Is;

namespace Agenix.Screenplay.Tests.Questions;

public class WhenUsingQuestionsWithDelays
{
    [Test]
    public void ShouldNotFailWhenAnExpectedExceptionIsThrown()
    {
        // Arrange
        var jane = Actor.Named("Jane");
        var clicker = new Clicker();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
        {
            await jane.Should(
                Eventually(SeeThat(TheClickerValueWithAnExpectedException.Of(clicker),
                        EqualTo(10)))
                    .IgnoringExceptions(typeof(InvalidOperationException))
            );
        });
    }

    private class Clicker
    {
        public int Count { get; private set; }

        public int Click()
        {
            return ++Count;
        }
    }

    private class TheClickerValueWithAnExpectedException : IQuestion<int>
    {
        private readonly Clicker _clicker;

        private TheClickerValueWithAnExpectedException(Clicker clicker)
        {
            _clicker = clicker;
        }

        public Task<int> AnsweredBy(Actor actor)
        {
            var click = _clicker.Click();
            return _clicker.Count < 10
                ? throw new InvalidOperationException("Ignore this: " + click)
                : Task.FromResult(_clicker.Count);
        }

        public static TheClickerValueWithAnExpectedException Of(Clicker clicker)
        {
            return new TheClickerValueWithAnExpectedException(clicker);
        }
    }
}
