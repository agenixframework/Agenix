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

#region Imports

using System.Data;
using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.IO;
using Agenix.Core;
using Agenix.Core.Util;
using Agenix.Sql.Ado;
using Agenix.Sql.Util;

#endregion

namespace Agenix.Sql.Actions;

/// <summary>
///     An abstract class for executing database-related test actions with transactional support.
/// </summary>
/// <remarks>
///     This class provides the necessary setup for connecting to a database and executing
///     SQL statements within a specified transaction context. It requires a subclass to
///     implement the execution logic through the DoExecute method.
/// </remarks>
public abstract class AbstractDatabaseConnectingTestAction : AdoDaoSupport, IAsyncTestAction, INamed, IAsyncDescribed
{
    /// <summary>
    /// Represents the path to the SQL resource file used for executing database scripts.
    /// </summary>
    protected readonly string? sqlResourcePath;

    /**
     * List of SQL statements
     */
    protected readonly List<string> statements;

    protected readonly bool transactionEnabled;

    protected readonly string? transactionIsolationLevel;
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
        bool transactionEnabled,
        string transactionTimeout,
        List<string> statements)
    {
        this.name = name;
        this.description = description;
        if (dbProvider != null)
        {
            DbProvider = dbProvider;
        }

        if (adoTemplate != null)
        {
            AdoTemplate = adoTemplate;
        }

        this.sqlResourcePath = sqlResourcePath;
        this.transactionIsolationLevel = transactionIsolationLevel;
        this.transactionEnabled = transactionEnabled;
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

    /// Indicates whether this action should use an ADO.NET transaction during execution.
    public bool TransactionEnabled => transactionEnabled;

    /// <summary>
    /// Represents the transaction isolation level for database operations.
    /// </summary>
    public string? TransactionIsolationLevel => transactionIsolationLevel;

    /// <summary>
    /// Specifies the timeout duration for a database transaction.
    /// </summary>
    public string? TransactionTimeout => transactionTimeout;

    /// <summary>
    ///     Sets the description for the current action.
    /// </summary>
    /// <param name="newDescription">The new description to set for the action.</param>
    /// <returns>The updated instance of the action with the new description.</returns>
    public IAsyncTestAction SetDescription(string newDescription)
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
    ///     Retrieves the name of the current test action instance.
    /// </summary>
    /// <returns>The name of the current instance.</returns>
    public string Name => name;

    /// <summary>
    /// Executes the action asynchronously within the provided test context.
    /// </summary>
    /// <param name="context">The test context in which the action will be executed.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous execution of the action.</returns>
    public async Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default)
    {
        await DoExecute(context, cancellationToken);
    }

    /// <summary>
    ///     Sets the name for the current instance.
    /// </summary>
    /// <param name="newName">The new name to be assigned.</param>
    public void SetName(string newName)
    {
        name = newName;
    }


    /// Reads SQL statements from an external file resource. The file can contain multiple
    /// multi-line statements and comments.
    /// @param context The current test context used for resource path processing.
    /// @return A list of SQL statements extracted from the file resource.
    /// /
    protected async Task<List<string>> CreateStatementsFromFileResource(TestContext context)
    {
        return await SqlUtils.CreateStatementsFromFileResource(await FileUtils.GetFileResourceAsync(SqlResourcePath, context));
    }

    /// Reads SQL statements from an external file resource. The file can contain multiple
    /// multi-line statements and comments.
    /// <param name="context">The current test context used for resource path processing.</param>
    /// <param name="lineDecorator">The decorator used for processing the last line of the SQL script.</param>
    /// <return>A list of SQL statements extracted from the file resource.</return>
    protected async Task<List<string>> CreateStatementsFromFileResource(TestContext context,
        SqlUtils.ILastScriptLineDecorator? lineDecorator)
    {
        return await SqlUtils.CreateStatementsFromFileResource(
            await FileUtils.GetFileResourceAsync(SqlResourcePath, context), lineDecorator);
    }

    /// <summary>
    ///     Executes the database-related test action. Subclasses must override this method
    ///     to implement specific execution logic.
    /// </summary>
    /// <param name="context">The test context that provides state and additional execution information.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A Task representing the asynchronous execution of the test action.</returns>
    public abstract Task DoExecute(TestContext context, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Action Builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public abstract class AbstractDatabaseBuilder<T, S> : AbstractAsyncTestActionBuilder<T, S>
        where T : AbstractDatabaseConnectingTestAction
        where S : AbstractDatabaseBuilder<T, S>
    {
        internal readonly List<string> statements = [];
        internal AdoTemplate? adoTemplate;
        internal IDbProvider? dbProvider;
        internal string? sqlResourcePath;
        internal bool transactionEnabled;
        internal string transactionIsolationLevel = nameof(IsolationLevel.ReadCommitted);
        internal string transactionTimeout = (-1).ToString();

        /// Sets the database provider for the test action builder.
        /// <param name="newDbProvider">The database provider instance to be set.</param>
        /// <return>The builder instance with the database provider updated.</return>
        public S DbProvider(IDbProvider newDbProvider)
        {
            dbProvider = newDbProvider;
            return Self;
        }

        /// <summary>
        ///     Sets the AdoTemplate used for database operations in the test action builder.
        /// </summary>
        /// <param name="newAdoTemplate">The AdoTemplate instance to be set.</param>
        /// <returns>The builder instance with the AdoTemplate updated.</returns>
        public S AdoTemplate(AdoTemplate newAdoTemplate)
        {
            adoTemplate = newAdoTemplate;
            return Self;
        }

        /// <summary>
        ///     Enables or disables wrapping statements in a single ADO.NET transaction.
        /// </summary>
        /// <param name="enabled">True to enable a transaction; false to execute without a transaction.</param>
        /// <returns>The builder instance.</returns>
        public S UseTransaction(bool enabled)
        {
            transactionEnabled = enabled;
            return Self;
        }

        /// Sets the transaction timeout value for the test action builder.
        /// <param name="timeout">The timeout duration in seconds for the transaction.</param>
        /// <return>The builder instance with the transaction timeout value updated.</return>
        public S TransactionTimeout(int timeout)
        {
            transactionTimeout = timeout.ToString();
            return Self;
        }

        /// Sets the transaction isolation level for the test action builder.
        /// <param name="newIsolationLevel">The IsolationLevel to be set for transactions.</param>
        /// <returns>The builder instance with the transaction isolation level updated.</returns>
        public S TransactionIsolationLevel(string newIsolationLevel)
        {
            transactionIsolationLevel = newIsolationLevel;
            return Self;
        }

        /// Adds an SQL statement to the list of statements to be executed as part of the database test action.
        /// <param name="sql">The SQL statement to be added to the list.</param>
        /// <return>Returns an instance of the builder to allow method chaining.</return>
        public S Statement(string sql)
        {
            statements.Add(sql);
            return Self;
        }

        /// <summary>
        ///     Adds a list of SQL statements to be executed by the test action builder.
        /// </summary>
        /// <param name="newStatements">The list of SQL statements to be added.</param>
        /// <returns>The builder instance with the statements updated.</returns>
        public S Statements(List<string> newStatements)
        {
            statements.AddRange(newStatements);
            return Self;
        }

        /// <summary>
        ///     Sets the SQL resource to be used, and adds the SQL statements derived from it to the test action builder.
        /// </summary>
        /// <param name="newSqlResource">The SQL resource containing the statements to be executed.</param>
        /// <returns>The builder instance with the SQL statements updated.</returns>
        public S SqlResource(IResource newSqlResource)
        {
            Statements(SqlUtils.CreateStatementsFromFileResource(newSqlResource).ConfigureAwait(false).GetAwaiter().GetResult());
            return Self;
        }

        /// <summary>
        ///     Sets the path for the SQL resource file in the test action builder.
        /// </summary>
        /// <param name="resourceName">The file path to the SQL resource to be set.</param>
        /// <returns>The builder instance with the SQL resource path updated.</returns>
        public S SqlResource(string resourceName)
        {
            sqlResourcePath = resourceName;
            return Self;
        }
    }
}
