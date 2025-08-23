using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class EatsATangerine(string size) : IPerformable
{
    [Step("{0} eats a #size pear")]
    public Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        // Implementation here
        Console.WriteLine($"{actor.Name} eats a {size} tangerine");
        return Task.CompletedTask;
    }

    public static EatsATangerine OfSize(string size)
    {
        return Instrumented.InstanceOf<EatsATangerine>().WithProperties(size);
    }

    public override string ToString()
    {
        return $"EatsATangerine(size: {size})";
    }
}
