using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Agenix.Core.Exceptions;
using Agenix.Sql.Actions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Data.Core;
using Spring.Transaction;

namespace Agenix.Core.NUnitTestProject.Sql.Actions;

public class ExecuteSqlActionTest : AbstractNUnitSetUp
{
    private static readonly string DbStmt1 = "DELETE * FROM ERRORS WHERE STATUS='resolved'";
    private static readonly string DbStmt2 = "DELETE * FROM CONFIGURATION WHERE VERSION=1";

    private readonly Mock<AdoTemplate> _adoTemplate = new();
    private readonly Mock<IPlatformTransactionManager> _transactionManager = new();

    private ExecuteSqlAction.Builder _executeSqlAction;

    [SetUp]
    public void SetUpMethod()
    {
        _executeSqlAction = new ExecuteSqlAction.Builder().AdoTemplate(_adoTemplate.Object);
    }

    [Test]
    public void TestSqlExecutionWithInlineStatements()
    {
        _adoTemplate.Reset();

        // Define the SQL statements to test
        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        sqlAction.Execute(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2), Times.Once);
    }

    [Test]
    public void TestSqlExecutionWithTransactions()
    {
        _adoTemplate.Reset();
        _transactionManager.Reset();

        // Define the SQL statements to test
        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.TransactionManager(_transactionManager.Object);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        sqlAction.Execute(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2), Times.Once);
    }

    [Test]
    public void TestSqlExecutionWithFileResource()
    {
        _adoTemplate.Reset();

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file:" + testDirectory + @"/ResourcesTest/Sql/Actions/test-sql-statements.sql";
        _executeSqlAction.SqlResource(filePath);

        // Build and execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        sqlAction.Execute(Context);

        // Assert that the expected SQL statements were executed
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2), Times.Once);
    }

    [Test]
    public void TestSqlExecutionWithInlineScriptVariableSupport()
    {
        _adoTemplate.Reset();

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
        sqlAction.Execute(Context);

        // Verify that the SQL statements were executed with substituted variables
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2), Times.Once);
    }

    [Test]
    public void TestSqlExecutionWithFileResourceVariableSupport()
    {
        _adoTemplate.Reset();

        Context.SetVariable("resolvedStatus", "resolved");
        Context.SetVariable("version", "1");

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file:" + testDirectory + @"/ResourcesTest/Sql/Actions/test-sql-with-variables.sql";
        _executeSqlAction.SqlResource(filePath);

        // Execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        sqlAction.Execute(Context);

        // Verify that the SQL statements were executed with substituted variables
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2), Times.Once);
    }

    [Test]
    public void TestSqlExecutionIgnoreErrors()
    {
        _adoTemplate.Reset();

        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.IgnoreErrors(true);

        // Setup the mock to throw an exception for DB_STMT_2
        _adoTemplate.Setup(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2))
            .Throws(new Exception("Something went wrong!"));

        // Execute the SQL action
        var sqlAction = _executeSqlAction.Build();
        sqlAction.Execute(Context);

        // Verify DB_STMT_1 is executed despite the error
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
    }

    [Test]
    public void TestSqlExecutionErrorForwarding()
    {
        _adoTemplate.Reset();

        var statements = new List<string> { DbStmt1, DbStmt2 };
        _executeSqlAction.Statements(statements);
        _executeSqlAction.IgnoreErrors(false);

        // Setup the mock to throw an exception for DB_STMT_2
        _adoTemplate.Setup(template => template.ExecuteNonQuery(CommandType.Text, DbStmt2))
            .Throws(new Exception("Something went wrong!"));

        // Assert that an exception is thrown when executing
        Assert.Throws<CoreSystemException>(() =>
        {
            var sqlAction = _executeSqlAction.Build();
            sqlAction.Execute(Context);
        });

        // Verify that DB_STMT_1 is executed before the exception occurs
        _adoTemplate.Verify(template => template.ExecuteNonQuery(CommandType.Text, DbStmt1), Times.Once);
    }

    [Test]
    public void TestNoJdbcTemplateConfigured()
    {
        _adoTemplate.Reset();

        // Initialize the action builder without a SqlTemplate
        _executeSqlAction = new ExecuteSqlAction.Builder().AdoTemplate(null);
        _executeSqlAction.Statements(["statement"]);

        // Expect an exception when trying to execute with a null template
        var exception = Assert.Throws<CoreSystemException>(() =>
        {
            var sqlAction = _executeSqlAction.Build();
            sqlAction.Execute(Context);
        });

        // Check that the exception message is as expected
        ClassicAssert.AreEqual("No AdoTemplate configured for sql execution!", exception.Message);
    }
}