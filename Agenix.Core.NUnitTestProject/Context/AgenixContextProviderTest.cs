using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Context;

public class AgenixContextProviderTest
{
    [Test]
    public void TestLookup()
    {
        var provider = IAgenixContextProvider.Lookup();
        ClassicAssert.AreEqual(provider.Create().GetType(), typeof(AgenixContext));
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsFalse(IAgenixContextProvider.Lookup(DefaultAgenixContextProvider.Spring).IsPresent);
    }
}