using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Agenix.Core.Util;
using Agenix.Sql.Actions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Data.Core;
using Spring.Transaction;
using static Agenix.Sql.Actions.ExecuteSqlQueryAction.Builder;

namespace Agenix.Core.Tests.Sql.Actions.Dsl;

/// <summary>
///     Unit tests for the ExecuteSqlQuery action builder component within the SQL actions DSL.
/// </summary>
public class ExecuteSqlQueryTestActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<AdoTemplate> _adoTemplate;
    private Mock<IPlatformTransactionManager> _transactionManager;


    [SetUp]
    public void SetUp()
    {
        _adoTemplate = new Mock<AdoTemplate>();
        _transactionManager = new Mock<IPlatformTransactionManager>();
    }

    [Test]
    public void TestExecuteSqlQueryWithResource()
    {
        var results = new List<Dictionary<string, object>> { new() { { "NAME", "Leonard" } } };

        _adoTemplate.Setup(j => j.QueryWithRowMapper(CommandType.Text, It.IsAny<string>(),
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
            .Returns(results)
            .Callback(() => _adoTemplate.Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), It.IsAny<string>(),
                    It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>()))
                .Returns(new List<Dictionary<string, object>> { new() { { "CNT_EPISODES", "100000" } } }));

        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("episodeId", "agenix:RandomNumber(5)");

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(assemblyLocation);
        var filePath = "file://" + testDirectory + @"/ResourcesTest/Sql/Actions/Dsl/query-script.sql";

        builder.Run(Query().AdoTemplate(_adoTemplate.Object)
            .SqlResource(FileUtils.GetFileResource(filePath, Context))
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
        ClassicAssert.AreEqual(_adoTemplate.Object, action.AdoTemplate);
        ClassicAssert.AreEqual(2, action.Statements.Count);
        ClassicAssert.IsNull(action.SqlResourcePath);
    }

    [Test]
    public void TestExecuteSqlQueryWithStatements()
    {
        var results = new List<Dictionary<string, object>>
        {
            new() { { "NAME", "Penny" } }, new() { { "NAME", "Sheldon" } }
        };

        _adoTemplate.Reset();

        _adoTemplate.Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), "SELECT NAME FROM ACTORS",
            It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(results);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), "SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(new List<Dictionary<string, object>>
            {
                new() { { "CNT_EPISODES", "9999" } }
            });

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(Query().AdoTemplate(_adoTemplate.Object)
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

        ClassicAssert.AreEqual(_adoTemplate.Object, action.AdoTemplate);
    }

    [Test]
    public void TestExecuteSqlQueryWithTransaction()
    {
        var results = new List<Dictionary<string, object>>
        {
            new() { { "NAME", "Penny" } }, new() { { "NAME", "Sheldon" } }
        };

        Mock.Get(_adoTemplate.Object).Reset();
        Mock.Get(_transactionManager.Object).Reset();

        _adoTemplate.Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), "SELECT NAME FROM ACTORS",
            It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(results);

        _adoTemplate
            .Setup(j => j.QueryWithRowMapper(It.IsAny<CommandType>(), "SELECT COUNT(*) as CNT_EPISODES FROM EPISODES",
                It.IsAny<ExecuteSqlQueryAction.DictionaryRowMapper>())).Returns(new List<Dictionary<string, object>>
            {
                new() { { "CNT_EPISODES", "9999" } }
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(Query().AdoTemplate(_adoTemplate.Object)
            .TransactionManager(_transactionManager.Object)
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

        ClassicAssert.AreEqual(_adoTemplate.Object, action.AdoTemplate);
        ClassicAssert.AreEqual(_transactionManager.Object, action.TransactionManager);
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
