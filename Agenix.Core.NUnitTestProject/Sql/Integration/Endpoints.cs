using System;
using System.IO;
using System.Reflection;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Spi;
using Agenix.Sql.Actions;
using Spring.Data.Common;
using Spring.Data.Core;

namespace Agenix.Core.NUnitTestProject.Sql.Integration;

/// <summary>
///     Represents the SQL integration endpoints for testing within the Agenix NUnit framework.
/// </summary>
/// <remarks>
///     This class is used in integration tests to facilitate the execution of SQL actions.
///     It is intended to be used with Agenix's testing framework to manage and validate SQL operations
///     such as executing scripts, running queries, and validating results against expected conditions.
/// </remarks>
public class Endpoints
{
    # region Bind To Registry

    [BindToRegistry(Name = "DBProvider")] private static readonly IDbProvider _dbProvider = CreateDbProvider();

    [BindToRegistry(Name = "AdoTemplate")] private AdoTemplate _adoTemplate = new()
    {
        DbProvider = _dbProvider
    };

    private static IDbProvider CreateDbProvider()
    {
        var dbProvider = DbProviderFactory.GetDbProvider("System.Data.SQLite");
        const string connectionString = "Data Source=InMemory;Mode=Memory;Cache=Shared";

        dbProvider.ConnectionString = connectionString;
        return dbProvider;
    }

    private static readonly string assemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string testDirectory = Path.GetDirectoryName(assemblyLocation);

    [BindToRegistry(Name = "queryScriptSqlFile")]
    private readonly string queryScriptSqlFile =
        "file:" + testDirectory + @"/ResourcesTest/Sql/Integration/Actions/query-script.sql";

    [BindToRegistry(Name = "scriptSqlFile")]
    private readonly string scriptSqlFile =
        "file:" + testDirectory + @"/ResourcesTest/Sql/Integration/Actions/script.sql";

    private readonly string createTablesScriptSqlFile =
        "file:" + testDirectory + @"/ResourcesTest/Sql/Integration/Actions/create-tables.sql";

    [BindToRegistry]
    private IBeforeTest BeforeTest()
    {
        return SequenceBeforeTest.Builder.BeforeTest()
            .Actions(ExecuteSqlAction.Builder.Sql(_dbProvider)
                .SqlResource(createTablesScriptSqlFile))
            .Build();
    }

    [BindToRegistry]
    private IAfterTest AfterTest()
    {
        return SequenceAfterTest.Builder.AfterTest()
            .Actions(EchoAction.Builder.Echo("After test executed"))
            .Build();
    }

    #endregion
}