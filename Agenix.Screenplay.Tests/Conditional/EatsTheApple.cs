namespace Agenix.Screenplay.Tests.Conditional;

public class EatsTheApple : IPerformable
{
    public readonly Apple Apple;

    public EatsTheApple() { }

    public EatsTheApple(Apple apple)
    {
        Apple = apple;
    }

    public void PerformAs<T>(T actor) where T : Actor
    {
        Apple.Eat();
    }
}
