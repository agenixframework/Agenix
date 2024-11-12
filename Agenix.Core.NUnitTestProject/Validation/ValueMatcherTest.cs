using Agenix.Core.Validation.Matcher;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation;

public class ValueMatcherTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IValueMatcher.Lookup();

        Assert.That(validators, Is.Not.Null);
        Assert.That(validators, Is.Not.Empty);
        Assert.That(validators.Count, Is.EqualTo(1));
        Assert.That(validators.ContainsKey("hamcrest"), Is.True);
        Assert.That(validators["hamcrest"].GetType(), Is.EqualTo(typeof(HamcrestValueMatcher)));
    }
}