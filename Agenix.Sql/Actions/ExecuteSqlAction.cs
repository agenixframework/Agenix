using System.Data;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using log4net;
using Spring.Data.Common;
using Spring.Transaction.Support;

namespace Agenix.Sql.Actions;

public class ExecuteSqlAction : AbstractDatabaseConnectingTestAction
{
    /// Logger for ExecuteSQLQueryAction.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(ExecuteSqlAction));

    private readonly CommandType _commandType;

    ///// <summary>
    /////     Boolean flag marking that possible SQL errors will be ignored.
    ///// </summary>
    ///// <returns>Returns true if errors will be ignored, false otherwise.</returns>
    private readonly bool _ignoreErrors;

    private ExecuteSqlAction(Builder builder) : base(builder.GetName() ?? "sql", builder.GetDescription(),
        builder.dbProvider,
        builder.adoTemplate,
        builder.sqlResourcePath,
        builder.transactionIsolationLevel,
        builder.transactionManager,
        builder.transactionTimeout,
        builder.statements)
    {
        _ignoreErrors = builder._ignoreErrors;
        _commandType = builder._commandType;
    }

    /// Executes a list of SQL statements within the provided test context.
    /// <param name="newStatements">A list of SQL statements to be executed.</param>
    /// <param name="context">
    ///     The test context in which the statements will be executed. This context may contain dynamic
    ///     content and configuration data required for execution.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when there is no AdoTemplate configured, or when an error occurs during
    ///     statement execution and errors are not set to be ignored.
    /// </exception>
    protected void ExecuteStatements(List<string> newStatements, TestContext context)
    {
        if (AdoTemplate == null) throw new AgenixSystemException("No AdoTemplate configured for sql execution!");

        foreach (var statement in newStatements)
            try
            {
                var toExecute = context.ReplaceDynamicContentInString(statement.Trim().EndsWith(';')
                    ? statement.Trim()[..(statement.Trim().Length - 1)]
                    : statement.Trim());

                if (Log.IsDebugEnabled) Log.Debug("Executing SQL statement: " + toExecute);

                AdoTemplate.ExecuteNonQuery(_commandType, toExecute);

                Log.Info("SQL statement execution successful");
            }
            catch (Exception e)
            {
                if (_ignoreErrors)
                    Log.Error("Ignoring error while executing SQL statement: " + e.Message, e);
                else
                    throw new AgenixSystemException(e.Message, e);
            }
    }

    /// Executes a series of SQL statements within the given test context.
    /// <param name="context">
    ///     The test context that provides dynamic content and configuration settings necessary for executing
    ///     SQL statements.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if no transaction manager is configured and an error occurs during
    ///     execution without being set to be ignored, or if there is an issue related to SQL statement processing.
    /// </exception>
    public override void DoExecute(TestContext context)
    {
        var statementsToUse = statements.Count == 0 ? CreateStatementsFromFileResource(context) : statements;


        if (TransactionManager != null)
        {
            Log.Debug($"Using transaction manager: {TransactionManager.GetType().Name}");

            var transactionTemplate = new TransactionTemplate(TransactionManager)
            {
                TransactionTimeout = int.Parse(context.ReplaceDynamicContentInString(TransactionTimeout)),
                TransactionIsolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel),
                    context.ReplaceDynamicContentInString(TransactionIsolationLevel))
            };

            transactionTemplate.Execute(status =>
            {
                ExecuteStatements(statementsToUse, context);
                return null;
            });
        }
        else
        {
            ExecuteStatements(statementsToUse, context);
        }
    }

    /// Provides a mechanism for building and configuring instances of the ExecuteSqlAction class.
    /// The Builder class allows the customization of SQL execution actions by setting various parameters,
    /// such as the database provider and error-handling options.
    /// /
    public class Builder : AbstractDatabaseBuilder<ExecuteSqlAction, Builder>
    {
        internal CommandType _commandType = System.Data.CommandType.Text;
        internal bool _ignoreErrors;

        /// Creates a new builder instance configured with default settings.
        /// <return>Returns a new builder instance with default configuration.</return>
        public static Builder Sql()
        {
            return new Builder();
        }

        /// Creates a new builder instance configured with the specified database provider.
        /// <param name="iDbProvider">The database provider instance to be set for the builder.</param>
        /// <return>Returns a new builder instance configured with the provided database provider.</return>
        public static Builder Sql(IDbProvider iDbProvider)
        {
            var builder = new Builder();
            builder.DbProvider(iDbProvider);
            return builder;
        }

        /// Creates a new builder instance configured with the current database provider.
        /// <return>Returns the current builder instance with the database provider set.</return>
        public ExecuteSqlQueryAction.Builder Query()
        {
            return new ExecuteSqlQueryAction.Builder().DbProvider(dbProvider ??
                                                                  throw new InvalidOperationException(
                                                                      "IDbProvider is not configured."));
        }

        /// Sets the flag to ignore errors during SQL execution.
        /// <param name="ignoreErrors">Boolean flag to determine if errors should be ignored.</param>
        /// <return>Returns the current builder instance with the updated flag setting.</return>
        /// /
        public Builder IgnoreErrors(bool ignoreErrors)
        {
            _ignoreErrors = ignoreErrors;
            return this;
        }

        /// Sets the command type for executing SQL commands.
        /// <param name="commandType">The CommandType to be used, which determines how the command is interpreted.</param>
        /// <return>Returns the builder instance with the specified command type applied.</return>
        public Builder CommandType(CommandType commandType)
        {
            _commandType = commandType;
            return this;
        }

        public override ExecuteSqlAction Build()
        {
            return new ExecuteSqlAction(this);
        }
    }
}