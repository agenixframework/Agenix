namespace Agenix.Screenplay.Tests.Tasks;

public class EatsFruit : IPerformable
{
    private readonly string fruit;

    public EatsFruit()
    {
    }

    private EatsFruit(string fruit)
    {
        this.fruit = fruit;
    }
    
    public void PerformAs<T>(T actor) where T : Actor
    {
    }

    public static EatsFruit Loudly()
    {
        return new EatsFruit("peach");

    }
}
