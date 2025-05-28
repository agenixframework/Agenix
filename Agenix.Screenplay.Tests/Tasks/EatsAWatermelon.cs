namespace Agenix.Screenplay.Tests.Tasks;

public class EatsAWatermelon : IPerformable
{
    private readonly string fruit;

    public EatsAWatermelon(string fruit)
    {
        this.fruit = fruit;
    }

    public static EatsAWatermelon Quietly()
    {
        return Agenix.Screenplay.Tasks.Instrumented<EatsAWatermelon>("watermelon quietly");
    }

    public static EatsAWatermelon Noisily()
    {
        return Agenix.Screenplay.Tasks.Instrumented<EatsAWatermelon>("watermelon loudly");
    }
    
    public void PerformAs<T>(T actor) where T : Actor
    {
    }
}
