using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Tasks;

public class WhenInstrumentingTasksTests
{
    [Test]
    public void ANonInstrumentedClassTaskExecution()
    {
        IPerformable basicTask = new EatsAMango();

        Actor.Named("Annie").AttemptsTo(basicTask);
    }

    [Test]
    public void ForClassesWithoutDefaultConstructorUseExplicitlyInstrumentedClass()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsAPear.OfSize("large"));
    }

    [Test]
    public void ANonInstrumentedClassWithABuilder()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsFruit.Loudly());
    }

    [Test]
    public void ANestedTask()
    {
        // When
        Actor.Named("Annie").AttemptsTo(new Eats(new EatsAMango()));
    }

    [Test]
    public void ATaskWithParameters()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsAWatermelon.Quietly(), EatsAWatermelon.Noisily());
    }
}
