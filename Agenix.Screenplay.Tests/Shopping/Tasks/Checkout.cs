namespace Agenix.Screenplay.Tests.Shopping.Tasks;

public class Checkout(int waitTimeInSeconds)
{
    private volatile string _nextCustomer = "Bob";
    private Timer? _timer;


    public static Checkout FastCheckout()
    {
        var checkout = new Checkout(1);
        checkout.StartTimer();
        return checkout;
    }

    public static Checkout SlowCheckout()
    {
        var checkout = new Checkout(7);
        checkout.StartTimer();
        return checkout;
    }

    public string NextCustomer()
    {
        return _nextCustomer;
    }

    private void StartTimer()
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(waitTimeInSeconds * 1000).ConfigureAwait(false);
                Run();
            }
            catch (TaskCanceledException)
            {
                // Ignored
            }
        });
    }

    public void Run()
    {
        _nextCustomer = "Dana";
    }
}
