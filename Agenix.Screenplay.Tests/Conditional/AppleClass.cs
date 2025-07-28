namespace Agenix.Screenplay.Tests.Conditional;

public class Apple
{
    private bool _eaten;

    public void Eat()
    {
        _eaten = true;
    }

    public bool IsEaten()
    {
        return _eaten;
    }
}
