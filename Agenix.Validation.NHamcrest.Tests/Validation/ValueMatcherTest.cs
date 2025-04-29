using Agenix.Core.Validation.Matcher;
using Agenix.Validation.NHamcrest.Validation;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class ValueMatcherTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IValueMatcher.Lookup();

        Assert.That(validators, Is.Not.Null);
        Assert.That(validators, Is.Not.Empty);
        Assert.That(validators.Count, Is.EqualTo(1));
        Assert.That(validators.ContainsKey("nhamcrest"), Is.True);
        Assert.That(validators["nhamcrest"].GetType(), Is.EqualTo(typeof(NHamcrestValueMatcher)));
    }
}