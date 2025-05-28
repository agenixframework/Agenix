namespace Agenix.Core.Tests.Spi.mocks;

public class SingletonFoo
{

    public static readonly SingletonFoo INSTANCE = new SingletonFoo();

    public SingletonFoo() {
    }
}
