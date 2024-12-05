using System.Data;
using Agenix.Core;
using Agenix.Core.Common;
using Agenix.Core.Spi;
using Agenix.Sql.Util;
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Agenix.Sql.Actions;

/// <summary>
///     An abstract class for executing database-related test actions with transactional support.
/// </summary>
/// <remarks>
///     This class provides the necessary setup for connecting to a database and executing
///     SQL statements within a specified transaction context. It requires a subclass to
///     implement the execution logic through the DoExecute method.
/// </remarks>
public abstract class AbstractDatabaseConnectingTestAction : AdoDaoSupport, ITestAction, INamed, IDescribed
{
    /**
     * SQL file resource path
     */
    protected readonly string? sqlResourcePath;

    /**
     * List of SQL statements
     */
    protected readonly List<string> statements;

    protected readonly string? transactionIsolationLevel;

    protected readonly IPlatformTransactionManager? transactionManager;
    protected readonly string? transactionTimeout;

    /**
     * Text describing the test action
     */
    private string description;

    /**
     * TestAction name injected as spring bean name
     */
    private string name;

    protected AbstractDatabaseConnectingTestAction(string name,
        string description,
        IDbProvider? dbProvider,
        AdoTemplate? adoTemplate,
        string? sqlResourcePath,
        string transactionIsolationLevel,
        IPlatformTransactionManager? transactionManager,
        string transactionTimeout,
        List<string> statements)
    {
        this.name = name;
        this.description = description;
        if (dbProvider != null) DbProvider = dbProvider;

        if (adoTemplate != null) AdoTemplate = adoTemplate;
        this.sqlResourcePath = sqlResourcePath;
        this.transactionIsolationLevel = transactionIsolationLevel;
        this.transactionManager = transactionManager;
        this.transactionTimeout = transactionTimeout;
        this.statements = statements;
    }

    /**
     * SQL file resource path
     */
    public string? SqlResourcePath => sqlResourcePath;

    /// Provides access to a collection of SQL statements to be executed as part of the test action.
    /// /
    public List<string> Statements => statements;

    /// Provides access to the transaction manager used for managing database transactions
    /// within the context of executing database-related test actions.
    /// /
    public IPlatformTransactionManager? TransactionManager => transactionManager;

    public string? TransactionIsolationLevel => transactionIsolationLevel;

    public string? TransactionTimeout => transactionTimeout;

    /// <summary>
    ///     Sets the description for the current action.
    /// </summary>
    /// <param name="newDescription">The new description to set for the action.</param>
    /// <returns>The updated instance of the action with the new description.</returns>
    public ITestAction SetDescription(string newDescription)
    {
        description = newDescription;
        return this;
    }

    /// <summary>
    ///     Retrieves the description associated with the test action.
    /// </summary>
    /// <returns>The description of the test action.</returns>
    public string GetDescription()
    {
        return description;
    }

    /// <summary>
    ///     Sets the name for the current instance.
    /// </summary>
    /// <param name="newName">The new name to be assigned.</param>
    public void SetName(string newName)
    {
        name = newName;
    }

    /// <summary>
    ///     Do basic logging and delegate execution to subclass.
    /// </summary>
    /// <param name="context">The test context</param>
    public void Execute(TestContext context)
    {
        DoExecute(context);
    }

    /// <summary>
    ///     Retrieves the name of the current test action instance.
    /// </summary>
    /// <returns>The name of the current instance.</returns>
    public string Name => name;


    /// Reads SQL statements from an external file resource. The file can contain multiple
    /// multi-line statements and comments.
    /// @param context The current test context used for resource path processing.
    /// @return A list of SQL statements extracted from the file resource.
    /// /
    protected List<string> CreateStatementsFromFileResource(TestContext context)
    {
        return SqlUtils.CreateStatementsFromFileResource(
            Resources.FromFileSystem(context.ReplaceDynamicContentInString(SqlResourcePath)));
    }

    /// Reads SQL statements from an external file resource. The file can contain multiple
    /// multi-line statements and comments.
    /// <param name="context">The current test context used for resource path processing.</param>
    /// <param name="lineDecorator">The decorator used for processing the last line of the SQL script.</param>
    /// <return>A list of SQL statements extracted from the file resource.</return>
    protected List<string> CreateStatementsFromFileResource(TestContext context,
        SqlUtils.ILastScriptLineDecorator? lineDecorator)
    {
        return SqlUtils.CreateStatementsFromFileResource(
            Resources.FromFileSystem(context.ReplaceDynamicContentInString(SqlResourcePath)), lineDecorator);
    }

    /// <summary>
    ///     Subclasses may add custom execution logic here.
    /// </summary>
    /// <param name="context"></param>
    public abstract void DoExecute(TestContext context);

    /// <summary>
    ///     Action Builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public abstract class AbstractDatabaseBuilder<T, S> : AbstractTestActionBuilder<T, S>
        where T : AbstractDatabaseConnectingTestAction
        where S : AbstractDatabaseBuilder<T, S>
    {
        internal readonly List<string> statements = [];
        internal AdoTemplate? adoTemplate;
        internal IDbProvider? dbProvider;
        internal string? sqlResourcePath;
        internal string transactionIsolationLevel = IsolationLevel.ReadCommitted.ToString();
        internal IPlatformTransactionManager? transactionManager;
        internal string transactionTimeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT.ToString();

        /// Sets the database provider for the test action builder.
        /// <param name="newDbProvider">The database provider instance to be set.</param>
        /// <return>The builder instance with the database provider updated.</return>
        public S DbProvider(IDbProvider newDbProvider)
        {
            dbProvider = newDbProvider;
            return self;
        }

        /// <summary>
        ///     Sets the AdoTemplate used for database operations in the test action builder.
        /// </summary>
        /// <param name="newAdoTemplate">The AdoTemplate instance to be set.</param>
        /// <returns>The builder instance with the AdoTemplate updated.</returns>
        public S AdoTemplate(AdoTemplate newAdoTemplate)
        {
            adoTemplate = newAdoTemplate;
            return self;
        }

        /// <summary>
        ///     Sets the platform transaction manager for managing transactions in the test action builder.
        /// </summary>
        /// <param name="newTransactionManager">The IPlatformTransactionManager instance to be set.</param>
        /// <returns>The builder instance with the transaction manager updated.</returns>
        public S TransactionManager(IPlatformTransactionManager newTransactionManager)
        {
            transactionManager = newTransactionManager;
            return self;
        }

        /// Sets the transaction timeout value for the test action builder.
        /// <param name="timeout">The timeout duration in seconds for the transaction.</param>
        /// <return>The builder instance with the transaction timeout value updated.</return>
        public S TransactionTimeout(int timeout)
        {
            transactionTimeout = timeout.ToString();
            return self;
        }

        /// Sets the transaction isolation level for the test action builder.
        /// <param name="newIsolationLevel">The IsolationLevel to be set for transactions.</param>
        /// <returns>The builder instance with the transaction isolation level updated.</returns>
        public S TransactionIsolationLevel(string newIsolationLevel)
        {
            transactionIsolationLevel = newIsolationLevel;
            return self;
        }

        /// Adds an SQL statement to the list of statements to be executed as part of the database test action.
        /// <param name="sql">The SQL statement to be added to the list.</param>
        /// <return>Returns an instance of the builder to allow method chaining.</return>
        public S Statement(string sql)
        {
            statements.Add(sql);
            return self;
        }

        /// <summary>
        ///     Adds a list of SQL statements to be executed by the test action builder.
        /// </summary>
        /// <param name="newStatements">The list of SQL statements to be added.</param>
        /// <returns>The builder instance with the statements updated.</returns>
        public S Statements(List<string> newStatements)
        {
            statements.AddRange(newStatements);
            return self;
        }

        /// <summary>
        ///     Sets the SQL resource to be used, and adds the SQL statements derived from it to the test action builder.
        /// </summary>
        /// <param name="newSqlResource">The SQL resource containing the statements to be executed.</param>
        /// <returns>The builder instance with the SQL statements updated.</returns>
        public S SqlResource(IResource newSqlResource)
        {
            Statements(SqlUtils.CreateStatementsFromFileResource(newSqlResource));
            return self;
        }

        /// <summary>
        ///     Sets the path for the SQL resource file in the test action builder.
        /// </summary>
        /// <param name="filePath">The file path to the SQL resource to be set.</param>
        /// <returns>The builder instance with the SQL resource path updated.</returns>
        public S SqlResource(string filePath)
        {
            sqlResourcePath = filePath;
            return self;
        }
    }
}