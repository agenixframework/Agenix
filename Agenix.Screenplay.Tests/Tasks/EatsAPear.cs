namespace Agenix.Screenplay.Tests.Tasks;

public class EatsAPear(string size) : IPerformable
{
    private readonly string size = size;

    public void PerformAs<T>(T actor) where T : Actor
    {
    }

    public static EatsAPear OfSize(string size)
    {
        return Screenplay.Tasks.Instrumented<EatsAPear>(size);
    }
}
