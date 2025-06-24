#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections.Generic;
using Agenix.Api.Variable;
using NUnit.Framework;

namespace Agenix.Core.Tests.Variable;

[TestFixture]
public class VariableExpressionSegmentMatcherTests
{
    [Test]
    [TestCaseSource(nameof(GetTestExpressions))]
    public void TestExpression(TestData testData)
    {
        var matcher = new VariableExpressionSegmentMatcher(testData.Expression);

        foreach (var attributes in testData.SegmentAttributes)
        {
            Assert.That(matcher.NextMatch(), Is.True, $"Failed to find next match in {testData.Expression}");
            Assert.That(matcher.SegmentExpression, Is.EqualTo(attributes.Name));
            Assert.That(matcher.SegmentIndex, Is.EqualTo(attributes.Index));
        }

        Assert.That(matcher.NextMatch(), Is.False, $"Found unexpected additional match in {testData.Expression}");
    }

    private static IEnumerable<TestData> GetTestExpressions()
    {
        return new[]
        {
            new TestData("var.prop1[1].prop2[2].prop3")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("prop1", 1)
                .AddSegmentAttributes("prop2", 2)
                .AddSegmentAttributes("prop3", -1),
            new TestData("var[2].prop1.prop2[2].prop3")
                .AddSegmentAttributes("var", 2)
                .AddSegmentAttributes("prop1", -1)
                .AddSegmentAttributes("prop2", 2)
                .AddSegmentAttributes("prop3", -1),
            new TestData("var.jsonPath($.name1.name2)")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$.name1.name2", -1),
            new TestData("var.jsonPath($['store']['book'][0]['author'])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$['store']['book'][0]['author']", -1),
            new TestData("var.jsonPath($.store.book[*].author)")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$.store.book[*].author", -1),
            new TestData("var.jsonPath($..author)")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$..author", -1),
            new TestData("var.jsonPath($..book[(@.length-1)])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$..book[(@.length-1)]", -1),
            new TestData("var.jsonPath($..book[?(@.price<10)])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("$..book[?(@.price<10)]", -1),
            new TestData("var1.prop1[1].prop2[2].jsonPath($..book[?(@.price<10)])")
                .AddSegmentAttributes("var1", -1)
                .AddSegmentAttributes("prop1", 1)
                .AddSegmentAttributes("prop2", 2)
                .AddSegmentAttributes("$..book[?(@.price<10)]", -1),
            new TestData("var.xpath(//title[@lang='en'])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("//title[@lang='en']", -1),
            new TestData("var.xpath(/bookstore/book[price>35.00])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("/bookstore/book[price>35.00]", -1),
            new TestData("var.xpath(/bookstore/book[position()<3])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("/bookstore/book[position()<3]", -1),
            new TestData("var.xpath(/bookstore/book[last()-1])")
                .AddSegmentAttributes("var", -1)
                .AddSegmentAttributes("/bookstore/book[last()-1]", -1),
            new TestData("var1.prop1[1].prop2[2].xpath(//title[@lang='en']])")
                .AddSegmentAttributes("var1", -1)
                .AddSegmentAttributes("prop1", 1)
                .AddSegmentAttributes("prop2", 2)
                .AddSegmentAttributes("//title[@lang='en']]", -1)
        };
    }

    /// <summary>
    ///     Helper class to define test data for segment expression matching
    /// </summary>
    public class TestData(string expression)
    {
        public string Expression { get; } = expression;
        public List<SegmentAttributes> SegmentAttributes { get; } = [];

        public TestData AddSegmentAttributes(string name, int index)
        {
            SegmentAttributes.Add(new SegmentAttributes(name, index));
            return this;
        }

        public override string ToString()
        {
            return Expression;
        }
    }

    /// <summary>
    ///     Helper class to define expected segment attributes
    /// </summary>
    public class SegmentAttributes(string name, int index)
    {
        /// <summary>
        ///     The name of the segment
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        ///     The index. -1 if not appropriate.
        /// </summary>
        public int Index { get; } = index;
    }
}
