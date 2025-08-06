using System.Collections.Concurrent;
using Agenix.Screenplay.Parallel;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Parallel;

[TestFixture]
public class WhenRunningTasksInParallel
{
    [SetUp]
    public void SetUp()
    {
        // Clear completed tasks before each test
        while (_completedTasks.TryTake(out _))
        {
            // no code
        }
    }

    private readonly ConcurrentBag<string> _completedTasks = new();

    private IPerformable DoSomething()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            _completedTasks.Add("Do something");
        });
    }

    private IPerformable DoAnotherThing()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            _completedTasks.Add("Do another thing");
        });
    }

    private IPerformable DoAThing()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            _completedTasks.Add("Do a thing");
        });
    }

    private IPerformable DoYetAnotherThing()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            _completedTasks.Add("Do yet another thing");
        });
    }

    private IPerformable DoSomethingElse()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            _completedTasks.Add("Do something else");
        });
    }

    private static IPerformable DoSomethingThatFails()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            Assert.Fail("Fail by purpose");
        });
    }

    private IPerformable DoSomethingWithSubTasks()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            actor.AttemptsTo(DoAThing(), DoYetAnotherThing());
        });
    }

    private IPerformable DoSomethingSlowly()
    {
        return new AnonymousPerformableFunction(actor =>
        {
            try
            {
                Thread.Sleep(500);
                _completedTasks.Add("Do something slowly");
            }
            catch (ThreadInterruptedException)
            {
                // Handle interruption if needed
            }
        });
    }

    [Test]
    public void ParallelTasksShouldRunWithASingleAction()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");

        // Act
        InParallel.TheActors(actorA).Perform("Actor A performs",
            () => actorA.AttemptsTo(DoSomething())
        );

        // Assert
        Assert.That(_completedTasks, Does.Contain("Do something"));
        Assert.That(_completedTasks.Count, Is.EqualTo(1));
    }

    [Test]
    public void ParallelTasksShouldRunWithASequenceOfActions()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");

        // Act
        InParallel.TheActors(actorA).Perform(() => actorA.AttemptsTo(DoSomething(), DoSomethingElse())
        );

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo(["Do something", "Do something else"]));
    }

    [Test]
    public void AGroupOfActorsCanAllPerformTheSameTaskInParallel()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");
        var actorC = Actor.Named("Actor C");

        // Act
        InParallel.TheActors(actorA, actorB, actorC).EachAttemptTo(DoSomething(), DoSomethingElse());

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo([
            "Do something", "Do something else",
            "Do something", "Do something else",
            "Do something", "Do something else"
        ]));
    }

    [Test]
    public void FailingTasksShouldBeReported()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");
        var actorC = Actor.Named("Actor C");

        // Act & Assert
        Assert.Throws<AssertionException>(() =>
        {
            InParallel.TheActors(actorA, actorB, actorC).EachAttemptTo(DoSomething(), DoSomethingThatFails());
            actorA.AttemptsTo(DoSomethingElse());
        });
    }

    [Test]
    public void ActorsCanBeDefinedInACollection()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");
        var actorC = Actor.Named("Actor C");

        var cast = new List<Actor> { actorA, actorB, actorC };

        // Act
        InParallel.TheActors(cast).EachAttemptTo(DoSomething(), DoSomethingElse());

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo([
            "Do something", "Do something else",
            "Do something", "Do something else",
            "Do something", "Do something else"
        ]));
    }

    [Test]
    public void ParallelStreamsOfSingleActionsShouldResultInSeparateSteps()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");

        // Act
        InParallel.TheActors(actorA, actorB).Perform(
            () => actorA.AttemptsTo(DoSomething()),
            () => actorB.AttemptsTo(DoAnotherThing())
        );

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo(["Do something", "Do another thing"]));
    }

    [Test]
    public void ParallelStreamsOfSingleActionsShouldResultInSeparateStepsInTheRightOrder()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");

        // Act
        InParallel.TheActors(actorA, actorB).Perform(
            () => actorA.AttemptsTo(DoSomething()),
            () => actorB.AttemptsTo(DoSomethingElse(), DoAnotherThing())
        );

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo(["Do something", "Do something else", "Do another thing"]));
    }

    [Test]
    public void NestedParallelTasksShouldBeRecordedInTheRightOrder()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");

        // Act
        InParallel.TheActors(actorA, actorB).Perform(
            () => actorA.AttemptsTo(DoSomething(), DoSomethingWithSubTasks()),
            () => actorB.AttemptsTo(DoSomethingElse(), DoSomethingWithSubTasks(), DoAnotherThing())
        );

        // Assert
        Assert.That(_completedTasks, Is.EquivalentTo([
            "Do something",
            "Do something else",
            "Do a thing",
            "Do a thing",
            "Do yet another thing",
            "Do yet another thing",
            "Do another thing"
        ]));
    }

    [Test]
    public void NestedParallelTasksShouldBeRecordedEvenIfTheyTakeSomeTime()
    {
        // Arrange
        var actorA = Actor.Named("Actor A");
        var actorB = Actor.Named("Actor B");

        // Act
        InParallel.TheActors(actorA, actorB).Perform(
            () => actorA.AttemptsTo(DoSomething(), DoSomethingSlowly()),
            () => actorB.AttemptsTo(DoSomethingElse(), DoSomethingWithSubTasks(), DoAnotherThing())
        );

        // Assert
        Assert.That(_completedTasks, Does.Contain("Do something else"));
        Assert.That(_completedTasks, Does.Contain("Do something"));
        Assert.That(_completedTasks, Does.Contain("Do a thing"));
        Assert.That(_completedTasks, Does.Contain("Do yet another thing"));
        Assert.That(_completedTasks, Does.Contain("Do another thing"));
        Assert.That(_completedTasks, Does.Contain("Do something slowly"));
    }
}
