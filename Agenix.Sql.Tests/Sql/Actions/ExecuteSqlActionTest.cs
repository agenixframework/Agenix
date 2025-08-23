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

using System.Data;
using System.Data.Common;
using System.Reflection;
using Agenix.Api.Exceptions;
using Agenix.Sql.Actions;
using Agenix.Sql.Ado;
using Moq;
using NUnit.Framework.Legacy;

namespace Agenix.Sql.Tests.Sql.Actions;

public class ExecuteSqlActionTest : AbstractNUnitSetUp
{
    private static readonly string DbStmt1 = "DELETE * FROM ERRORS WHERE STATUS='resolved'";
    private static readonly string DbStmt2 = "DELETE * FROM CONFIGURATION WHERE VERSION=1";

    private readonly Mock<AdoTemplate> _adoTemplate = new();

    private ExecuteSqlAction.Builder _executeSqlAction;

    [SetUp]
    public void SetUpMethod()
    {
        _executeSqlAction = new ExecuteSqlAction.Builder().AdoTemplate(_adoTemplate.Object);
        BridgeAsyncToSync();
    }

    private void BridgeAsyncToSync()
    {
        // Bridge async AdoTemplate calls by returning a completed task to avoid recursive proxy invocations
        _adoTemplate.Setup(t =>
                t.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));

        _adoTemplate.Setup(t => t.ExecuteNonQueryAsync(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<DbConnection>(), It.IsAny<DbTransaction>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(0));
    }

    [Test]
    public async Task TestSqlExecutionWithInlineStatements()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        // Define the SQL statements to test
        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, It.IsAny<CancellationToken>()),
            Times.Once);
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TestSqlExecutionWithTransactions()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        // Define the SQL statements to test
        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.UseTransaction(true);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, It.IsAny<CancellationToken>()),
            Times.Once);
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TestSqlExecutionWithFileResource()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file://" + testDirectory + @"/ResourcesTest/Sql/Actions/test-sql-statements.sql";
        _executeSqlAction.SqlResource(filePath);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, It.IsAny<CancellationToken>()),
            Times.Once);
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TestSqlExecutionWithInlineScriptVariableSupport()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        Context.SetVariable("resolvedStatus", "resolved");
        Context.SetVariable("version", "1");

        var statements = new List<string>
        {
            "DELETE * FROM ERRORS WHERE STATUS='${resolvedStatus}'",
            "DELETE * FROM CONFIGURATION WHERE VERSION=${version}"
        };

        _executeSqlAction.Statements(statements);

        // Execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Verify that the SQL statements were executed with substituted variables
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, CancellationToken.None), Times.Once);
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task TestSqlExecutionWithFileResourceVariableSupport()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        Context.SetVariable("resolvedStatus", "resolved");
        Context.SetVariable("version", "1");

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file://" + testDirectory + @"/ResourcesTest/Sql/Actions/test-sql-with-variables.sql";
        _executeSqlAction.SqlResource(filePath);

        // Execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Verify that the SQL statements were executed with substituted variables
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, CancellationToken.None), Times.Once);
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task TestSqlExecutionIgnoreErrors()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.IgnoreErrors(true);

        // Setup the mock to throw an exception for DB_STMT_2
        _adoTemplate.Setup(template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, CancellationToken.None))
            .Throws(new Exception("Something went wrong!"));

        // Execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        await sqlAction.ExecuteAsync(Context);

        // Verify DB_STMT_1 is executed despite the error
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, CancellationToken.None), Times.Once);
    }

    [Test]
    public void TestSqlExecutionErrorForwarding()
    {
        _adoTemplate.Reset();
        BridgeAsyncToSync();

        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.IgnoreErrors(false);

        // Setup the mock to throw an exception for DB_STMT_2
        _adoTemplate.Setup(template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt2, CancellationToken.None))
            .Throws(new Exception("Something went wrong!"));

        // Assert that an exception is thrown when executing
        Assert.ThrowsAsync<AgenixSystemException>(async () =>
        {
            var sqlAction = _executeSqlAction.Build();
            await sqlAction.ExecuteAsync(Context);
        });

        // Verify that DB_STMT_1 is executed before the exception occurs
        _adoTemplate.Verify(
            template => template.ExecuteNonQueryAsync(CommandType.Text, DbStmt1, CancellationToken.None), Times.Once);
    }

    [Test]
    public void TestNoJdbcTemplateConfigured()
    {
        _adoTemplate.Reset();

        // Initialize the action builder without a SqlTemplate
        _executeSqlAction = new ExecuteSqlAction.Builder().AdoTemplate(null);
        _executeSqlAction.Statements(["statement"]);

        // Expect an exception when trying to execute with a null template
        var exception = Assert.ThrowsAsync<AgenixSystemException>(() =>
        {
            var sqlAction = _executeSqlAction.Build();
            return sqlAction.ExecuteAsync(Context);
        });

        // Check that the exception message is as expected
        ClassicAssert.AreEqual("No AdoTemplate configured for sql execution!", exception.Message);
    }
}
