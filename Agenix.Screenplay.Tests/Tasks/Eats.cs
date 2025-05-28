namespace Agenix.Screenplay.Tests.Tasks;

public class Eats : IPerformable
{
    private readonly IPerformable nestedTask;

    public Eats()
    {
    }

    public Eats(IPerformable nestedTask)
    {
        this.nestedTask = nestedTask;
    }
    
    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.AttemptsTo(nestedTask);

    }
}