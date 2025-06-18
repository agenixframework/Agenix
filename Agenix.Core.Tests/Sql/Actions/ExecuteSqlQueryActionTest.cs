using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Sql.Actions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Agenix.Core.Tests.Sql.Actions;

/// <summary>
///     Contains unit tests for the ExecuteSqlQueryAction class, verifying correct SQL execution and result extraction.
/// </summary>
/// <remarks>
///     This test class uses NUnit framework to test various scenarios of SQL query execution and validation.
///     Tests are organized to cover multiple aspects including transaction handling, variable extraction,
///     statement handling, result set validation, and more.
/// </remarks>
public class ExecuteSqlQueryActionTest : AbstractNUnitSetUp
{
    private static readonly string DbStmt1 = "select ORDERTYPE, STATUS from orders where ID = 5";
    private static readonly string DbStmt2 = "select NAME, HEIGHT from customers where ID = 1";

    private static readonly string DbStmt3 =
        "WITH RECURSIVE relations AS (SELECT id, framework_name FROM framework f INNER JOIN relations r ON f.relation_id = r.id) SELECT * FROM relations";

    private readonly Mock<AdoTemplate> _adoTemplate = new();
    private readonly Mock<IPlatformTransactionManager> _transactionManager = new();

    private ExecuteSqlQueryAction.Builder _executeSqlQueryAction;

    [SetUp]
    public void SetUpMethod()
    {
        _executeSqlQueryAction = new ExecuteSqlQueryAction.Builder().AdoTemplate(_adoTemplate.Object);
    }

    [Test]
    public void TestSqlStatement()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "status");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${status}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${status}"));
    }

    [Test]
    public void TestSqlStatementWithTransaction()
    {
        // Reset mocks
        _adoTemplate.Reset();
        _transactionManager.Reset();

        // Setup transaction status mock
        var transactionStatusMock = new Mock<ITransactionStatus>();
        _transactionManager.Setup(tm => tm.GetTransaction(It.IsAny<TransactionTemplate>()))
            .Returns(transactionStatusMock.Object);

        // Prepare result map
        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        // Setup JDBC template mock
        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        // Execute the SQL query action
        _executeSqlQueryAction.Statements([DbStmt1]);
        _executeSqlQueryAction.TransactionManager(_transactionManager.Object);
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "status");
        _executeSqlQueryAction.Build().Execute(Context);

        // Verify transaction commit
        _transactionManager.Verify(tm => tm.Commit(transactionStatusMock.Object));

        // Assert results
        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${status}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${status}"));
    }

    [Test]
    public void TestSqlStatementLowerCaseColumnNames()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ordertype", "small" }, { "status", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "status");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${status}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${status}"));
    }

    [Test]
    public void TestSQLMultipleStatements()
    {
        var sql1 = DbStmt1;
        var sql2 = DbStmt2;
        var sql3 = DbStmt3;
        _adoTemplate.Reset();

        var resultMap1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };
        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql1,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap1 });

        var resultMap2 = new Dictionary<string, object> { { "NAME", "Mickey Mouse" }, { "HEIGHT", "0,3" } };
        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql2,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap2 });

        var resultMap3 = new Dictionary<string, object> { { "ID", "1234" }, { "FRAMEWORK_NAME", "agenix" } };
        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql3,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap3 });

        var statements = new List<string> { sql1, sql2, sql3 };

        _executeSqlQueryAction.Statements(statements);
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "status");
        _executeSqlQueryAction.Extract("NAME", "name");
        _executeSqlQueryAction.Extract("HEIGHT", "height");
        _executeSqlQueryAction.Extract("ID", "id");
        _executeSqlQueryAction.Extract("FRAMEWORK_NAME", "frameworkName");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${status}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${status}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${name}"));
        ClassicAssert.AreEqual("Mickey Mouse", Context.GetVariable("${name}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${height}"));
        ClassicAssert.AreEqual("0,3", Context.GetVariable("${height}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${id}"));
        ClassicAssert.AreEqual("1234", Context.GetVariable("${id}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${frameworkName}"));
        ClassicAssert.AreEqual("agenix", Context.GetVariable("${frameworkName}"));
    }

    [Test]
    public void TestNullValue()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", null } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestVariableSupport()
    {
        Context.SetVariable("orderId", "5");

        var sql = "select ORDERTYPE, STATUS from orders where ID = ${orderId}";
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), DbStmt1,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestExtractToVariablesLowerCaseColumnNames()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ordertype", "small" }, { "status", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("ordertype", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "orderStatus");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${orderStatus}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${orderStatus}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
    }

    [Test]
    public void TestExtractToVariablesUnknownColumnMapping()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("UNKNOWN_COLUMN", "orderStatus");

        Assert.Throws<AgenixSystemException>(() => { _executeSqlQueryAction.Build().Execute(Context); });
    }

    [Test]
    public void TestResultSetValidation()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Validate("ORDERTYPE", "small");
        _executeSqlQueryAction.Validate("STATUS", "in_progress");
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestResultSetValidationLowerCase()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ordertype", "small" }, { "status", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Validate("ORDERTYPE", "small");
        _executeSqlQueryAction.Validate("STATUS", "in_progress");
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestResultSetValidationWithAliasNames()
    {
        var sql = "select ORDERTYPE AS TYPE, STATUS AS STATE from orders where ID=5";
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "TYPE", "small" }, { "STATE", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements(new List<string> { sql });
        _executeSqlQueryAction.Validate("TYPE", "small");
        _executeSqlQueryAction.Validate("STATE", "in_progress");
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestResultSetValidationError()
    {
        var sql = DbStmt1;
        _adoTemplate.Reset();

        var resultMap = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Validate("ORDERTYPE", "xxl"); // This is supposed to cause an error
        _executeSqlQueryAction.Validate("STATUS", "in_progress");

        try
        {
            _executeSqlQueryAction.Build().Execute(Context);
        }
        catch (ValidationException)
        {
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${ORDERTYPE}"));
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${STATUS}"));

            return;
        }

        Assert.Fail($"Expected test to fail with {nameof(ValidationException)} but was successful");
    }

    [Test]
    public void TestResultSetMultipleRowsValidation()
    {
        var sql = "select ORDERTYPE, STATUS from orders where ID < 5";
        _adoTemplate.Reset();

        var resultList = new List<Dictionary<string, object>>();

        var resultRow1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "started" } };
        var resultRow2 = new Dictionary<string, object> { { "ORDERTYPE", "medium" }, { "STATUS", "in_progress" } };
        var resultRow3 = new Dictionary<string, object> { { "ORDERTYPE", "big" }, { "STATUS", "finished" } };

        resultList.Add(resultRow1);
        resultList.Add(resultRow2);
        resultList.Add(resultRow3);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(resultList);

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Validate("ORDERTYPE", "small", "medium", "big");
        _executeSqlQueryAction.Validate("STATUS", "started", "in_progress", "finished");
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestIgnoreInMultipleRowsValidation()
    {
        var sql = "select ORDERTYPE, STATUS from orders where ID < 5";
        _adoTemplate.Reset();

        var resultList = new List<Dictionary<string, object>>();

        var resultRow1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "started" } };
        var resultRow2 = new Dictionary<string, object> { { "ORDERTYPE", "medium" }, { "STATUS", "in_progress" } };
        var resultRow3 = new Dictionary<string, object> { { "ORDERTYPE", "big" }, { "STATUS", "finished" } };

        resultList.Add(resultRow1);
        resultList.Add(resultRow2);
        resultList.Add(resultRow3);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(resultList);

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Validate("ORDERTYPE", "small", AgenixSettings.IgnorePlaceholder, "big");
        _executeSqlQueryAction.Validate("STATUS", AgenixSettings.IgnorePlaceholder, "in_progress", "finished");
        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestExtractMultipleRowValues()
    {
        var sql = "select distinct STATUS from orders";
        _adoTemplate.Reset();

        var resultList = new List<Dictionary<string, object>>();

        var resultRow1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "started" } };
        var resultRow2 = new Dictionary<string, object> { { "ORDERTYPE", null }, { "STATUS", "in_progress" } };
        var resultRow3 = new Dictionary<string, object> { { "ORDERTYPE", "big" }, { "STATUS", "finished" } };

        resultList.Add(resultRow1);
        resultList.Add(resultRow2);
        resultList.Add(resultRow3);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(resultList);

        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("STATUS", "orderStatus");
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Validate("ORDERTYPE", "small", AgenixSettings.IgnorePlaceholder, "big");
        _executeSqlQueryAction.Validate("STATUS", "started", "in_progress", "finished");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("orderType"));
        ClassicAssert.AreEqual("small;NULL;big", Context.GetVariable("orderType"));
        ClassicAssert.IsNotNull(Context.GetVariable("orderStatus"));
        ClassicAssert.AreEqual("started;in_progress;finished", Context.GetVariable("orderStatus"));
    }

    [Test]
    public void TestMultipleStatementsValidationError()
    {
        var sql1 = DbStmt1;
        var sql2 = DbStmt2;
        _adoTemplate.Reset();

        var resultMap1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql1,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap1 });

        var resultMap2 = new Dictionary<string, object> { { "NAME", "Mickey Mouse" }, { "HEIGHT", "0,3" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql2,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap2 });

        var statements = new List<string> { sql1, sql2 };

        _executeSqlQueryAction.Statements(statements);
        _executeSqlQueryAction.Validate("ORDERTYPE", "small");
        _executeSqlQueryAction.Validate("STATUS", "in_progress");
        _executeSqlQueryAction.Validate("NAME", "Donald Duck"); // This is supposed to cause an error
        _executeSqlQueryAction.Validate("HEIGHT", "0,3");

        try
        {
            _executeSqlQueryAction.Build().Execute(Context);
        }
        catch (ValidationException)
        {
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${ORDERTYPE}"));
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${STATUS}"));
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${NAME}"));
            ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("${HEIGHT}"));

            return;
        }

        Assert.Fail($"Expected test to fail with {nameof(ValidationException)} but was successful");
    }

    [Test]
    public void TestSqlStatementsWithFileResource()
    {
        _adoTemplate.Reset();

        var resultMap1 = new Dictionary<string, object> { { "ORDERTYPE", "small" }, { "STATUS", "in_progress" } };

        var resultMap2 = new Dictionary<string, object> { { "NAME", "Mickey Mouse" }, { "HEIGHT", "0,3" } };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap1 })
            .Callback(() => _adoTemplate.Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                    It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
                .Returns(new List<Dictionary<string, object>> { resultMap2 }));

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file://" + testDirectory + @"/ResourcesTest/Sql/Actions/test-sql-query-statements.sql";
        _executeSqlQueryAction.SqlResource(filePath);
        _executeSqlQueryAction.Extract("ORDERTYPE", "orderType");
        _executeSqlQueryAction.Extract("STATUS", "status");
        _executeSqlQueryAction.Extract("NAME", "name");
        _executeSqlQueryAction.Extract("HEIGHT", "height");
        _executeSqlQueryAction.Build().Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${orderType}"));
        ClassicAssert.AreEqual("small", Context.GetVariable("${orderType}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${status}"));
        ClassicAssert.AreEqual("in_progress", Context.GetVariable("${status}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${name}"));
        ClassicAssert.AreEqual("Mickey Mouse", Context.GetVariable("${name}"));
        ClassicAssert.IsNotNull(Context.GetVariable("${height}"));
        ClassicAssert.AreEqual("0,3", Context.GetVariable("${height}"));
    }

    [Test]
    public void TestResultSetValidationWithVariableAndFunction()
    {
        var sql = DbStmt1;

        // Mock result set
        var resultMap = new Dictionary<string, object>
        {
            { "ORDERTYPE", "testVariableValue" }, { "STATUS", "in_progress" }
        };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        // Execute SQL query and validate results
        _executeSqlQueryAction.Statements([sql]);

        _executeSqlQueryAction.Validate("ORDERTYPE", "${testVariable}");
        _executeSqlQueryAction.Validate("STATUS", "agenix:Concat('in_', ${progressVar})");

        // Set up context variables
        Context.SetVariable("testVariable", "testVariableValue");
        Context.SetVariable("progressVar", "progress");

        _executeSqlQueryAction.Build().Execute(Context);
    }

    [Test]
    public void TestBinaryBlobColumnValues()
    {
        var sql = "select ORDERTYPE, BINARY_DATA from orders where ID=5";

        // Mock result set
        var resultMap = new Dictionary<string, object>
        {
            { "ORDERTYPE", "small" }, { "BINARY_DATA", "some_binary_data"u8.ToArray() }
        };

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), sql,
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(new List<Dictionary<string, object>> { resultMap });

        // Execute SQL query and extract binary data
        _executeSqlQueryAction.Statements([sql]);
        _executeSqlQueryAction.Extract("BINARY_DATA", "binaryData");
        _executeSqlQueryAction.Build().Execute(Context);

        // Use assertions to verify the expected results
        ClassicAssert.NotNull(Context.GetVariable("${binaryData}"));

        var expectedBase64 = Convert.ToBase64String("some_binary_data"u8.ToArray());
        ClassicAssert.AreEqual(Context.GetVariable("${binaryData}"), expectedBase64);

        var decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(Context.GetVariable("${binaryData}")));
        ClassicAssert.AreEqual(decodedData, "some_binary_data");
    }

    [Test]
    public void TestNoJdbcTemplateConfigured()
    {
        // Setup action without a JdbcTemplate
        _executeSqlQueryAction = new ExecuteSqlQueryAction.Builder().AdoTemplate(null);

        _executeSqlQueryAction.Statements(["statement"]);

        // Assert that executing leads to an exception
        var exception = Assert.Throws<AgenixSystemException>(() => _executeSqlQueryAction.Build().Execute(Context));

        ClassicAssert.AreEqual("No AdoTemplate configured for query execution!", exception.Message);
    }
}
