using Agenix.Screenplay.Questions;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Questions;

public class WhenWorkingWithAggregateQuestionsTests
{
    [Test]
    public void CountingTheNumberOfItemsInASet()
    {
        // Given
        var tracy = Actor.Named("Tracy");

        // When
        IQuestion<ICollection<string>> whatColours =
            new Question<ICollection<string>>(actor => new HashSet<string> { "Red", "Blue", "Green" }
            );

        // Then
        Assert.That(AggregateQuestions.TheTotalNumberOf(whatColours).AnsweredBy(tracy), Is.EqualTo(3));
    }
}