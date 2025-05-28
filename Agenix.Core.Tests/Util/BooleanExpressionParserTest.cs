using Agenix.Api.Exceptions;
using Agenix.Core.Util;
using NUnit.Framework;

namespace Agenix.Core.Tests.Util;

public class BooleanExpressionParserTest
{
    [Test]
    public void TestExpressionParser()
    {
        Assert.That(BooleanExpressionParser.Evaluate("1 = 1"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("1 = 2"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("1 lt 2"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("2 lt 1"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("2 gt 1"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("1 gt 2"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("2 lt= 2"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("2 gt= 2"), Is.True);

        Assert.That(BooleanExpressionParser.Evaluate("2 lt= 1"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("2 gt= 3"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("(1 = 1)"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(1 = 1) and (2 = 2)"), Is.True);

        Assert.That(BooleanExpressionParser.Evaluate("(1 lt= 1) and (2 gt= 2)"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(1 gt 2) or (2 = 2)"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("((1 = 5) and (2 = 6)) or (2 gt 1)"), Is.True);

        Assert.That(BooleanExpressionParser.Evaluate("(1 = 2)"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("(1 = 1) and (2 = 3)"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("(1 lt 1) and (2 gt 2)"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("(1 gt 2) or (2 = 3)"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("((1 = 5) and (2 = 6)) or (2 lt 1)"), Is.False);
    }

    [Test]
    public void TestExpressionParserWithStringValues()
    {
        Assert.That(BooleanExpressionParser.Evaluate("true"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("true = true"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("false = false"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("false"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("true = false"), Is.False);
        Assert.That(BooleanExpressionParser.Evaluate("false = true"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("( false = false ) and ( true = true )"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("( false = false ) and ( true = false )"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("(false = false) and (true = true)"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(false = false) and (true = false)"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("(   false = false) and (true = true    )"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(false = false    ) and     (    true = false)"), Is.False);

        Assert.That(BooleanExpressionParser.Evaluate("( true = false ) or ( false = false )"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(false = false) or (true = true)"), Is.True);

        Assert.That(BooleanExpressionParser.Evaluate("(false = false) or (true = false)"), Is.True);
        Assert.That(BooleanExpressionParser.Evaluate("(false = false    ) or (    true = false)"), Is.True);
    }

    [Test]
    public void TestExpressionParserWithUnknownOperator()
    {
        var ex = Assert.Throws<AgenixSystemException>(() => BooleanExpressionParser.Evaluate("wahr"));
        Assert.That(ex.Message, Is.EqualTo("Unknown operator 'wahr'"));
    }

    [Test]
    public void TestExpressionParserWithBrokenExpression()
    {
        var ex = Assert.Throws<AgenixSystemException>(() => BooleanExpressionParser.Evaluate("1 = "));
        Assert.That(ex.Message,
            Is.EqualTo("Unable to parse boolean expression '1 = '. Maybe expression is incomplete!"),
            "Unexpected exception message");
    }
}
