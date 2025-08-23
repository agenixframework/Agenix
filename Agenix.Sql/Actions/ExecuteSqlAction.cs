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
using System.Diagnostics.CodeAnalysis;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Sql.Ado;
using Microsoft.Extensions.Logging;

namespace Agenix.Sql.Actions;

/// Represents an action for executing SQL statements during testing processes.
/// The ExecuteSqlAction class provides mechanisms for running SQL queries or statements
/// in a test context, leveraging the database provider and connections configured within the environment.
/// It supports features like asynchronous execution and integration with transaction and command timeout options.
public class ExecuteSqlAction : AbstractDatabaseConnectingTestAction
{
    /// Logger for ExecuteSQLQueryAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ExecuteSqlAction));

    private readonly CommandType _commandType;

    ///// <summary>
    ///// a Boolean flag marking that possible SQL errors will be ignored.
    ///// </summary>
    ///// <returns>Returns true if errors are ignored, false otherwise.</returns>
    private readonly bool _ignoreErrors;

    private ExecuteSqlAction(Builder builder) : base(builder.GetName() ?? "sql", builder.GetDescription(),
        builder.dbProvider,
        builder.adoTemplate,
        builder.sqlResourcePath,
        builder.transactionIsolationLevel,
        builder.transactionEnabled,
        builder.transactionTimeout,
        builder.statements)
    {
        _ignoreErrors = builder.IgnoreErrorsFlag;
        _commandType = builder.SqlCommandType;
    }

    /// Executes a list of SQL statements asynchronously within the provided test context.
    /// <param name="newStatements">A list of SQL statements to be executed.</param>
    /// <param name="context">
    ///     The test context in which the SQL statements will be executed. This context may include dynamic content and
    ///     configuration data required for execution.
    /// </param>
    /// <param name="connection">
    ///     An optional database connection to be used for executing the statements. If null, a new connection
    ///     will be established.
    /// </param>
    /// <param name="transaction">
    ///     An optional database transaction to be used for executing the statements. If null, the statements
    ///     will be executed without an explicit transaction.
    /// </param>
    /// <param name="commandTimeoutSeconds">
    ///     An optional timeout in seconds for the execution of SQL commands. If null, the default timeout
    ///     will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token that allows the operation to be cancelled.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if no AdoTemplate is configured or if an error occurs during statement execution
    ///     when errors are not set to be ignored.
    /// </exception>
    /// <returns>A task representing the asynchronous operation of executing the statements.</returns>
    [SuppressMessage(
        "SonarAnalyzer.CSharp",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification =
            "Complexity is acceptable here due to transactional orchestration; refactor would harm readability.")]
    protected async Task ExecuteStatementsAsync(List<string> newStatements, TestContext context,
        DbConnection? connection = null, DbTransaction? transaction = null, int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        if (AdoTemplate == null)
        {
            throw new AgenixSystemException("No AdoTemplate configured for sql execution!");
        }

        foreach (var statement in newStatements)
        {
            try
            {
                var toExecute = context.ReplaceDynamicContentInString(statement.Trim().EndsWith(';')
                    ? statement.Trim()[..(statement.Trim().Length - 1)]
                    : statement.Trim());

                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug("Executing SQL statement: {toExecute}", toExecute);
                }

                if (connection == null && transaction == null && commandTimeoutSeconds == null)
                {
                    await AdoTemplate.ExecuteNonQueryAsync(_commandType, toExecute, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await AdoTemplate.ExecuteNonQueryAsync(_commandType, toExecute, connection, transaction,
                        commandTimeoutSeconds, cancellationToken).ConfigureAwait(false);
                }

                Log.LogInformation("SQL statement execution successful");
            }
            catch (Exception e)
            {
                if (_ignoreErrors)
                {
                    Log.LogError(e, "Ignoring error while executing SQL statement: {Message}", e.Message);
                }
                else
                {
                    throw new AgenixSystemException(e.Message, e);
                }
            }
        }
    }

    /// Executes a series of SQL statements within the provided test context.
    /// <param name="context">
    ///     The test context that contains dynamic content and configuration settings required for executing
    ///     the SQL statements.
    /// </param>
    /// <param name="cancellationToken">
    ///     A CancellationToken to monitor for cancellation requests and to allow the operation
    ///     to be canceled if needed.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if a transaction manager is not configured and an error occurs during execution
    ///     without being set to be ignored, or if an issue arises during the processing of SQL statements.
    /// </exception>
    /// <returns>
    ///     A task representing the asynchronous execution of SQL statements within the test context.
    /// </returns>
    [SuppressMessage(
        "SonarAnalyzer.CSharp",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification =
            "Complexity is acceptable here due to transactional orchestration; refactor would harm readability.")]
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        var statementsToUse = statements.Count == 0 ? CreateStatementsFromFileResource(context) : statements;

        if (TransactionEnabled)
        {
            var provider = AdoTemplate?.DbProvider ?? DbProvider;
            if (provider != null)
            {
                await using var connection = provider.CreateConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }

                var iso = (IsolationLevel)Enum.Parse(typeof(IsolationLevel),
                    context.ReplaceDynamicContentInString(TransactionIsolationLevel));
                await using var transaction = await connection.BeginTransactionAsync(iso, cancellationToken);
                int? cmdTimeout =
                    int.TryParse(context.ReplaceDynamicContentInString(TransactionTimeout), out var timeout)
                        ? timeout
                        : null;
                try
                {
                    await ExecuteStatementsAsync(statementsToUse, context, connection, transaction, cmdTimeout,
                        cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    try { await transaction.RollbackAsync(cancellationToken); }
                    catch
                    {
                        /* ignore rollback exceptions */
                    }

                    throw;
                }
            }
            else
            {
                await ExecuteStatementsAsync(statementsToUse, context, null, null, null, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        else
        {
            await ExecuteStatementsAsync(statementsToUse, context, null, null, null, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// Provides a mechanism for building and configuring instances of the ExecuteSqlAction class.
    /// The Builder class allows the customization of SQL execution actions by setting various parameters,
    /// such as the database provider and error-handling options.
    /// /
    public class Builder : AbstractDatabaseBuilder<ExecuteSqlAction, Builder>
    {
        internal bool IgnoreErrorsFlag;
        internal CommandType SqlCommandType = System.Data.CommandType.Text;

        /// Creates a new builder instance configured with default settings.
        /// <return>Returns a new builder instance with the default configuration.</return>
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
            IgnoreErrorsFlag = ignoreErrors;
            return this;
        }

        /// Sets the command type for executing SQL commands.
        /// <param name="commandType">The CommandType to be used, which determines how the command is interpreted.</param>
        /// <return>Returns the builder instance with the specified command type applied.</return>
        public Builder CommandType(CommandType commandType)
        {
            SqlCommandType = commandType;
            return this;
        }

        /// Builds and returns an instance of the ExecuteSqlAction class.
        /// <returns>An instance of ExecuteSqlAction initialized with the current configuration of the builder.</returns>
        public override ExecuteSqlAction Build()
        {
            return new ExecuteSqlAction(this);
        }
    }
}
