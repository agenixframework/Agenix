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

using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Agenix.Core;
using Agenix.Core.Util;
using Agenix.Sql.Actions;
using Agenix.Sql.Ado;
using Moq;
using NUnit.Framework.Legacy;
using static Agenix.Sql.Actions.ExecuteSqlQueryAction.Builder;

namespace Agenix.Sql.Tests.Sql.Actions;

/// <summary>
///     Unit tests for the ExecuteSqlQuery action builder component within the SQL actions DSL.
/// </summary>
public class ExecuteSqlQueryTestActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<AdoTemplate> _adoTemplate;

    [SetUp]
    public void SetUp()
    {
        _adoTemplate = new Mock<AdoTemplate>();

        // Bridge async AdoTemplate query APIs to existing sync setups to keep tests unchanged
        _adoTemplate.Setup(t => t.QueryWithRowMapperAsync(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>(), It.IsAny<CancellationToken>()))
            .Returns((CommandType ct, string sql, ExecuteSqlQueryAction.DictionaryRowMapper rm,
                    CancellationToken token) =>
                (IList)_adoTemplate.Object.QueryWithRowMapperAsync(ct, sql, rm));
        _adoTemplate.Setup(t => t.QueryWithRowMapperAsync(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>(), It.IsAny<DbConnection>(),
                It.IsAny<DbTransaction>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns((CommandType ct, string sql, ExecuteSqlQueryAction.DictionaryRowMapper rm, DbConnection c,
                    DbTransaction tx, int? timeout, CancellationToken token) =>
                Task.FromResult<IList>(_adoTemplate.Object.QueryWithRowMapperAsync(ct, sql, rm, c, tx, timeout)));
    }

    [Test]
    public async Task TestExecuteSqlQueryWithResource()
    {
        var results = new List<Dictionary<string, object>> { new() { { "NAME", "Leonard" } } };

        _adoTemplate.Setup(j => j.QueryWithRowMapperAsync(CommandType.Text, It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(results)
            .Callback(() => _adoTemplate.Setup(j => j.QueryWithRowMapperAsync(It.IsAny<CommandType>(),
                    It.IsAny<string>(),
                    It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
                .Returns(new List<Dictionary<string, object>> { new() { { "CNT_EPISODES", "100000" } } }));

        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("episodeId", "agenix:RandomNumber(5)");

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file://" + testDirectory + @"/ResourcesTest/Sql/Actions/Dsl/query-script.sql";

        await builder.Run(Query().AdoTemplate(_adoTemplate.Object)
            .SqlResource(await FileUtils.GetFileResource(filePath, Context))
            .Validate("NAME", "Leonard")
            .Validate("CNT_EPISODES", "100000")
            .Extract("NAME", "actorName")
            .Extract("CNT_EPISODES", "episodesCount"));

        // Assertions
        ClassicAssert.IsNotNull(Context.GetVariable("actorName"));
        ClassicAssert.IsNotNull(Context.GetVariable("episodesCount"));
        ClassicAssert.AreEqual("Leonard", Context.GetVariable("actorName"));
        ClassicAssert.AreEqual("100000", Context.GetVariable("episodesCount"));

        // Assuming TestCase and ExecuteSQLQueryAction classes/objects are defined similarly
        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(ExecuteSqlQueryAction), test.GetActions().First().GetType());

        var action = (ExecuteSqlQueryAction)test.GetActions().First();

        IEnumerable<KeyValuePair<string, List<string>>> rows = action.ControlResultSet;
        ClassicAssert.AreEqual("sql-query", action.Name);
        ClassicAssert.AreEqual(2, action.ControlResultSet.Count);
        ClassicAssert.AreEqual("NAME=[Leonard]", ConvertKeyValuePairToString(GetRow("NAME", rows)));
        ClassicAssert.AreEqual("CNT_EPISODES=[100000]", ConvertKeyValuePairToString(GetRow("CNT_EPISODES", rows)));

        ClassicAssert.AreEqual(2, action.ExtractVariables.Count);
        ClassicAssert.AreEqual("actorName", action.ExtractVariables["NAME"]);
        ClassicAssert.AreEqual("episodesCount", action.ExtractVariables["CNT_EPISODES"]);
        ClassicAssert.AreEqual(2, action.Statements.Count);
        ClassicAssert.IsNull(action.SqlResourcePath);
    }

    [Test]
    public async Task TestExecuteSqlQueryWithStatements()
    {
        var results = new List<Dictionary<string, object>>
        {
            new() { { "NAME", "Penny" } }, new() { { "NAME", "Sheldon" } }
        };

        _adoTemplate.Reset();

        _adoTemplate.Setup(j => j.QueryWithRowMapperAsync(It.IsAny<CommandType>(), "SELECT NAME FROM ACTORS",
            It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(results);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapperAsync(It.IsAny<CommandType>(),
                "SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(new List<Dictionary<string, object>>
            {
                new() { { "CNT_EPISODES", "9999" } }
            });

        var builder = new DefaultTestCaseRunner(Context);
        await builder.Run(Query().AdoTemplate(_adoTemplate.Object)
            .Statement("SELECT NAME FROM ACTORS")
            .Statement("SELECT COUNT(*) as CNT_EPISODES FROM EPISODES")
            .Validate("NAME", "Penny", "Sheldon")
            .Validate("CNT_EPISODES", "9999")
            .Extract("NAME", "actorName")
            .Extract("CNT_EPISODES", "episodesCount"));

        ClassicAssert.IsNotNull(Context.GetVariable("actorName"));
        ClassicAssert.IsNotNull(Context.GetVariable("episodesCount"));
        ClassicAssert.AreEqual("Penny;Sheldon", Context.GetVariable("actorName"));
        ClassicAssert.AreEqual("9999", Context.GetVariable("episodesCount"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf(typeof(ExecuteSqlQueryAction), test.GetActions().First());

        var action = (ExecuteSqlQueryAction)test.GetActions().First();
        ClassicAssert.AreEqual("sql-query", action.Name);
        ClassicAssert.AreEqual(2, action.ControlResultSet.Count);

        var rows = action.ControlResultSet;
        ClassicAssert.AreEqual("NAME=[Penny, Sheldon]", ConvertKeyValuePairToString(GetRow("NAME", rows)));
        ClassicAssert.AreEqual("CNT_EPISODES=[9999]", ConvertKeyValuePairToString(GetRow("CNT_EPISODES", rows)));

        ClassicAssert.AreEqual(2, action.ExtractVariables.Count);
        ClassicAssert.AreEqual("actorName", action.ExtractVariables["NAME"]);
        ClassicAssert.AreEqual("episodesCount", action.ExtractVariables["CNT_EPISODES"]);
        ClassicAssert.AreEqual(2, action.Statements.Count);
        ClassicAssert.AreEqual("SELECT NAME FROM ACTORS, SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
            string.Join(", ", action.Statements));
    }

    [Test]
    public async Task TestExecuteSqlQueryWithTransaction()
    {
        var results = new List<Dictionary<string, object>>
        {
            new() { { "NAME", "Penny" } }, new() { { "NAME", "Sheldon" } }
        };

        Mock.Get(_adoTemplate.Object).Reset();

        _adoTemplate.Setup(j => j.QueryWithRowMapperAsync(It.IsAny<CommandType>(), "SELECT NAME FROM ACTORS",
            It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(results);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapperAsync(It.IsAny<CommandType>(),
                "SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(new List<Dictionary<string, object>>
            {
                new() { { "CNT_EPISODES", "9999" } }
            });

        var builder = new DefaultTestCaseRunner(Context);

        await builder.Run(Query().AdoTemplate(_adoTemplate.Object)
            .UseTransaction(true)
            .TransactionTimeout(5000)
            .TransactionIsolationLevel("ReadCommitted")
            .Statement("SELECT NAME FROM ACTORS")
            .Statement("SELECT COUNT(*) as CNT_EPISODES FROM EPISODES")
            .Validate("NAME", "Penny", "Sheldon")
            .Validate("CNT_EPISODES", "9999")
            .Extract("NAME", "actorName")
            .Extract("CNT_EPISODES", "episodesCount"));

        ClassicAssert.IsNotNull(Context.GetVariable("actorName"));
        ClassicAssert.IsNotNull(Context.GetVariable("episodesCount"));
        ClassicAssert.AreEqual("Penny;Sheldon", Context.GetVariable("actorName"));
        ClassicAssert.AreEqual("9999", Context.GetVariable("episodesCount"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ExecuteSqlQueryAction>(test.GetActions().First());

        var action = (ExecuteSqlQueryAction)test.GetActions().First();
        ClassicAssert.AreEqual("sql-query", action.Name);
        ClassicAssert.AreEqual(2, action.ControlResultSet.Count);

        var rows = action.ControlResultSet;
        ClassicAssert.AreEqual("NAME=[Penny, Sheldon]", ConvertKeyValuePairToString(GetRow("NAME", rows)));
        ClassicAssert.AreEqual("CNT_EPISODES=[9999]", ConvertKeyValuePairToString(GetRow("CNT_EPISODES", rows)));

        ClassicAssert.AreEqual(2, action.ExtractVariables.Count);
        ClassicAssert.AreEqual("actorName", action.ExtractVariables["NAME"]);
        ClassicAssert.AreEqual("episodesCount", action.ExtractVariables["CNT_EPISODES"]);
        ClassicAssert.AreEqual(2, action.Statements.Count);
        ClassicAssert.AreEqual("SELECT NAME FROM ACTORS, SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
            string.Join(", ", action.Statements));

        ClassicAssert.IsTrue(action.TransactionEnabled);
        ClassicAssert.AreEqual("5000", action.TransactionTimeout);
        ClassicAssert.AreEqual("ReadCommitted", action.TransactionIsolationLevel);
    }

    private static string ConvertKeyValuePairToString(KeyValuePair<string, List<string>> kvp)
    {
        // Join the list of values into a single string
        var valuesString = string.Join(", ", kvp.Value);

        // Format the result as "Key: Value1, Value2, Value3"
        return $"{kvp.Key}=[{valuesString}]";
    }

    private KeyValuePair<string, List<string>> GetRow(string columnName,
        IEnumerable<KeyValuePair<string, List<string>>> rows)
    {
        foreach (var row in rows)
        {
            if (row.Key.Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        throw new InvalidOperationException($"Missing column in result set for name '{columnName}'");
    }
}
