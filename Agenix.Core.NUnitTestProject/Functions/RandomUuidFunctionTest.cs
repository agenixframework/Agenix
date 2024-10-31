using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

public class RandomUuidFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomUuidFunction _function = new();

    [Test]
    public void TestRandomUuidFunction()
    {
        ClassicAssert.IsNotNull(_function.Execute([], Context));
    }
}