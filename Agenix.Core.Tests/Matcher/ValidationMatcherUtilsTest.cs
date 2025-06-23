using System.Collections.Generic;
using Agenix.Api.Validation.Matcher;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Matcher;

public class ValidationMatcherUtilsTest : AbstractNUnitSetUp
{
    private readonly Mock<IValidationMatcher> _matcher = new();
    private ValidationMatcherLibrary _validationMatcherLibrary = new();

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
        _validationMatcherLibrary = new ValidationMatcherLibrary
        {
            Name = "fooValidationMatcherLibrary",
            Prefix = "foo:"
        };
        _validationMatcherLibrary.Members.Add("CustomMatcher", _matcher.Object);
    }

    [Test]
    public void TestResolveDefaultValidationMatcher()
    {
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore()@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore('bad syntax')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@EqualsIgnoreCase('VAlUe')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${EqualsIgnoreCase('value')}@",
            Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${EqualsIgnoreCase(value)}@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "John's", "@EqualsIgnoreCase('John's')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "John's&Barabara's",
            "@EqualsIgnoreCase('John's&Barabara's')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "", "@EqualsIgnoreCase('')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "prefix:value",
            "@EqualsIgnoreCase('prefix:value')@", Context);
    }

    [Test]
    public void TestResolveCustomValidationMatcher()
    {
        _matcher.Reset();

        Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@foo:CustomMatcher('value')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@foo:CustomMatcher(value)@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${foo:CustomMatcher('value')}@",
            Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "prefix:value",
            "@foo:CustomMatcher('prefix:value')@", Context);

        _matcher.Verify(s => s.Validate("field", "value", new List<string> { "value" }, Context), Times.Exactly(3));
        _matcher.Verify(s => s.Validate("field", "value", new List<string> { "value" }, Context));
    }
}
