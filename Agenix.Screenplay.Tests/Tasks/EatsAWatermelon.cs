namespace Agenix.Screenplay.Tests.Tasks;

public class EatsAWatermelon : IPerformable
{
    private readonly string fruit;

    public EatsAWatermelon(string fruit)
    {
        this.fruit = fruit;
    }

    public void PerformAs<T>(T actor) where T : Actor
    {
    }

    public static EatsAWatermelon Quietly()
    {
        return Screenplay.Tasks.Instrumented<EatsAWatermelon>("watermelon quietly");
    }

    public static EatsAWatermelon Noisily()
    {
        return Screenplay.Tasks.Instrumented<EatsAWatermelon>("watermelon loudly");
    }
}
