using System;
using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher;

public class DefaultControlExpressionParserTest
{
    [Test]
    [TestCaseSource(nameof(ValidControlExpressions))]
    public void ShouldExtractControlParametersSuccessfully(string controlExpression, List<string> expectedParameters)
    {
        IControlExpressionParser expressionParser = new DefaultControlExpressionParser();
        var extractedParameters = expressionParser.ExtractControlValues(controlExpression, '\0');

        Console.WriteLine("Expected Parameters: " + string.Join(", ", expectedParameters));
        Console.WriteLine("Extracted Parameters: " + string.Join(", ", extractedParameters));

        ClassicAssert.AreEqual(extractedParameters.Count, expectedParameters.Count);

        for (var i = 0; i < expectedParameters.Count; i++)
        {
            ClassicAssert.True(extractedParameters.Count > i);
            ClassicAssert.AreEqual(extractedParameters[i], expectedParameters[i]);
        }
    }

    [Test]
    [TestCaseSource(nameof(InvalidControlExpressions))]
    public void ShouldNotExtractControlParametersSuccessfully(string controlExpression)
    {
        IControlExpressionParser expressionParser = new DefaultControlExpressionParser();

        Assert.Throws<AgenixSystemException>(() => expressionParser.ExtractControlValues(controlExpression, '\0'));
    }

    public static IEnumerable<TestCaseData> ValidControlExpressions()
    {
        yield return new TestCaseData("'a'", new List<string> { "a" });
        yield return new TestCaseData("'a',", new List<string> { "a" });
        yield return new TestCaseData("'a','b'", new List<string> { "a", "b" });
        yield return new TestCaseData("'a','b',", new List<string> { "a", "b" });
        yield return new TestCaseData("'a,s','b',", new List<string> { "a,s", "b" });
        yield return new TestCaseData("'a)s','b',", new List<string> { "a)s", "b" });
        yield return new TestCaseData("'a's','b',", new List<string> { "a's", "b" });
        yield return new TestCaseData("''", new List<string> { "" });
        yield return new TestCaseData("'',", new List<string> { "" });
        yield return new TestCaseData("", new List<string>());
        yield return new TestCaseData(null, new List<string>());
    }

    public static IEnumerable<TestCaseData> InvalidControlExpressions()
    {
        yield return new TestCaseData("'");
        yield return new TestCaseData("',");
        yield return new TestCaseData("'a");
        yield return new TestCaseData("'a,");
        yield return new TestCaseData("'a's,");
        yield return new TestCaseData("'a',s'");
        yield return new TestCaseData("'a','b");
        yield return new TestCaseData("'a','b,");
    }
}
