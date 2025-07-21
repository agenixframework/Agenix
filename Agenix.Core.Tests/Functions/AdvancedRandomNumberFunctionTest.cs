using System;
using System.Collections.Generic;
using System.Globalization;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;

namespace Agenix.Core.Tests.Functions;

public class AdvancedRandomNumberFunctionTest : AbstractNUnitSetUp
{
    private AdvancedRandomNumberFunction _function;
    private TestAdvancedRandomNumberFunction _functionWithRandomOne;
    private TestAdvancedRandomNumberFunction _functionWithRandomZero;

    [SetUp]
    public void SetUp()
    {
        _function = new AdvancedRandomNumberFunction();
        _functionWithRandomOne = new TestAdvancedRandomNumberFunction(1.0);
        _functionWithRandomZero = new TestAdvancedRandomNumberFunction(0.0);
    }

    [Test]
    [Repeat(100)]
    public void TestRandomNumberWithNullParameter()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(null, Context));
    }

    [Test]
    [Repeat(100)]
    public void TestRandomNumberWithDefaultValues()
    {
        var result = _function.Execute([], Context);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Match(@"-?\d*\.\d{2}"));
    }

    [Test]
    [Repeat(100)]
    public void TestRandomNumberWithDecimalPlaces()
    {
        var result = _function.Execute(["2"], Context);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Match(@"-?\d*\.\d{2}"));
    }

    [Test]
    [Repeat(100)]
    public void TestRandomNumberWithinRange()
    {
        var result = _function.Execute(["2", "10.5", "20.5"], Context);
        Assert.That(result, Is.Not.Null);
        var value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.InRange(10.5m, 20.5m));
    }

    [Test]
    public void TestRandomNumberIncludesMin()
    {
        var result = _functionWithRandomZero.Execute(["1", "10.5", "20.5"], Context);
        Assert.That(result, Is.EqualTo("10.5"));
    }

    [Test]
    public void TestRandomNumberIncludesMax()
    {
        var result = _functionWithRandomOne.Execute(["1", "10.5", "20.5"], Context);
        Assert.That(result, Is.EqualTo("20.5"));
    }

    [Test]
    public void TestRandomNumberExcludeMin()
    {
        var result = _functionWithRandomZero.Execute(["1", "10.5", "20.5", "True", "False"], Context);
        Assert.That(result, Is.Not.Null);
        var value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.GreaterThan(10.5m).And.LessThanOrEqualTo(20.5m));

        result = _functionWithRandomOne.Execute(["1", "10.5", "20.5", "True", "False"], Context);
        Assert.That(result, Is.Not.Null);
        value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.GreaterThan(10.5m).And.LessThanOrEqualTo(20.5m));
    }

    [Test]
    public void TestRandomNumberExcludeMax()
    {
        var result = _functionWithRandomZero.Execute(["2", "10.5", "20.5", "false", "true"], Context);
        Assert.That(result, Is.Not.Null);
        var value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.GreaterThanOrEqualTo(10.5m).And.LessThan(20.5m));

        result = _functionWithRandomOne.Execute(["2", "10.5", "20.5", "false", "true"], Context);
        Assert.That(result, Is.Not.Null);
        value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.GreaterThanOrEqualTo(10.5m).And.LessThan(20.5m));
    }

    [Test]
    [Repeat(100)]
    public void TestRandomInteger32EdgeCase()
    {
        var result = _functionWithRandomZero.Execute(["0", "-2147483648", "2147483647", "false", "false"], Context);
        Assert.That(result, Is.Not.Null);
        var value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.InRange((decimal)int.MinValue, (decimal)int.MaxValue));

        result = _functionWithRandomOne.Execute(["0", "-2147483648", "2147483647", "false", "false"], Context);
        Assert.That(result, Is.Not.Null);
        value = decimal.Parse(result, CultureInfo.InvariantCulture);
        Assert.That(value, Is.InRange((decimal)int.MinValue, (decimal)int.MaxValue));
    }

    [Test]
    [Repeat(100)]
    public void TestInvalidDecimalPlaces()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string> { "-1" }, Context));
    }

    [Test]
    [Repeat(100)]
    public void TestInvalidRange()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            _function.Execute(["2", "20.5", "10.5"], Context));
    }

    [TestCase(0, 12, null, null, false, false)]
    [TestCase(0, null, 0, 2, true, true)]
    [TestCase(0, null, null, null, false, false)]
    [Repeat(100)]
    public void TestRandomNumber(int decimalPlaces, object multipleOf, object minimum, object maximum,
        bool exclusiveMinimum, bool exclusiveMaximum)
    {
        var parameters = new List<string>
        {
            decimalPlaces.ToString(),
            minimum?.ToString() ?? "null",
            maximum?.ToString() ?? "null",
            exclusiveMinimum.ToString().ToLower(),
            exclusiveMaximum.ToString().ToLower(),
            multipleOf?.ToString() ?? "null"
        };

        var result = _function.Execute(parameters, Context);
        var value = decimal.Parse(result, CultureInfo.InvariantCulture);

        if (multipleOf != null)
        {
            var multipleOfDecimal = Convert.ToDecimal(multipleOf);
            var remainder = value % multipleOfDecimal;
            Assert.That(remainder, Is.EqualTo(0m));
        }

        if (maximum != null)
        {
            var maxDecimal = Convert.ToDecimal(maximum);
            if (exclusiveMaximum)
            {
                Assert.That(value, Is.LessThan(maxDecimal));
            }
            else
            {
                Assert.That(value, Is.LessThanOrEqualTo(maxDecimal));
            }
        }

        if (minimum != null)
        {
            var minDecimal = Convert.ToDecimal(minimum);
            if (exclusiveMinimum)
            {
                Assert.That(value, Is.GreaterThan(minDecimal));
            }
            else
            {
                Assert.That(value, Is.GreaterThanOrEqualTo(minDecimal));
            }
        }
    }

    private class TestAdvancedRandomNumberFunction(double fixedRandomValue) : AdvancedRandomNumberFunction
    {
        protected override decimal CreateRandomValue(decimal minValue, decimal range, double random)
        {
            return base.CreateRandomValue(minValue, range, fixedRandomValue);
        }
    }
}
