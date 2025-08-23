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
using Agenix.Sql.Ado.Exceptions;

namespace Agenix.Sql.Ado;

/// <summary>
///     Provides a template for performing database operations using ADO.NET.
///     This class simplifies the execution of common database commands, such as
///     executing SQL statements or mapping query results to objects.
/// </summary>
public class AdoTemplate
{
    private const string MissingDbProviderMessage =
        $"{nameof(AdoTemplate)}.{nameof(DbProvider)} is not configured. Configure {nameof(AdoTemplate)}.{nameof(DbProvider)} or pass an external {nameof(DbConnection)}.";

    /// <summary>
    ///     Gets or sets the database provider used for creating database connections.
    ///     The database provider is responsible for establishing connections to the
    ///     database and is used internally by the AdoTemplate class for executing
    ///     database operations, such as queries and commands.
    /// </summary>
    public IDbProvider? DbProvider { get; set; }

    // Async transaction-aware overload
    /// <summary>
    ///     Asynchronously executes a non-query SQL command (such as INSERT, UPDATE, DELETE) using the specified command type
    ///     and command text.
    ///     Returns the number of rows affected by the command.
    /// </summary>
    /// <param name="commandType">The type of the SQL command (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to execute.</param>
    /// <param name="externalConnection">
    ///     An optional external database connection to use. If null, a connection will be created
    ///     using the DbProvider.
    /// </param>
    /// <param name="transaction">An optional database transaction to associate with the command.</param>
    /// <param name="commandTimeoutSeconds">
    ///     An optional timeout value in seconds for the command execution. If null or less
    ///     than 0, the default value will be used.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of rows affected by the
    ///     command.
    /// </returns>
    /// <exception cref="DataAccessException">
    ///     Thrown when an error occurs during the asynchronous command execution, such as a database connection error or
    ///     command execution failure.
    /// </exception>
    public virtual async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText,
        DbConnection? externalConnection = null,
        DbTransaction? transaction = null, int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        var shouldDisposeConnection = externalConnection == null;
        var connection = externalConnection ?? DbProvider?.CreateConnection();

        if (connection == null)
        {
            throw new DataAccessException(MissingDbProviderMessage);
        }

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var cmd = connection.CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = commandText;
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            if (commandTimeoutSeconds is >= 0)
            {
                cmd.CommandTimeout = commandTimeoutSeconds.Value;
            }

            return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error executing non-query: {ex.Message}", ex);
        }
        finally
        {
            if (shouldDisposeConnection)
            {
                connection.Dispose();
            }
        }
    }

    /// <summary>
    ///     Asynchronously executes a non-query SQL command (such as INSERT, UPDATE, DELETE) using the specified command type
    ///     and command text.
    ///     Returns the number of rows affected by the command.
    /// </summary>
    /// <param name="commandType">The type of the SQL command (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of rows affected by the
    ///     command.
    /// </returns>
    /// <exception cref="DataAccessException">
    ///     Thrown when an error occurs during the asynchronous command execution.
    /// </exception>
    public virtual Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText,
        CancellationToken cancellationToken = default)
    {
        return ExecuteNonQueryAsync(commandType, commandText, null, null, null, cancellationToken);
    }

    // Async transaction-aware overload
    /// <summary>
    ///     Executes an asynchronous query with the specified command type, command text, and a row mapper,
    ///     optionally using an external database connection, transaction, and command timeout.
    ///     Returns a list of mapped results.
    /// </summary>
    /// <param name="commandType">Specifies how the command text is interpreted (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to be executed.</param>
    /// <param name="rowMapper">
    ///     An implementation of <see cref="IRowMapper" /> to map rows from the data reader to result
    ///     objects.
    /// </param>
    /// <param name="externalConnection">
    ///     An optional external <see cref="DbConnection" /> to use. If not specified, a new
    ///     connection is created.
    /// </param>
    /// <param name="transaction">An optional database transaction to associate with the query execution.</param>
    /// <param name="commandTimeoutSeconds">The command timeout in seconds. If null, the default timeout is used.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to observe if the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of mapped results.</returns>
    /// <exception cref="DataAccessException">
    ///     Thrown when the <see cref="DbProvider" /> is not configured or an error occurs during query execution.
    /// </exception>
    public virtual async Task<IList> QueryWithRowMapperAsync(CommandType commandType, string commandText,
        IRowMapper rowMapper,
        DbConnection? externalConnection, DbTransaction? transaction, int? commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var results = new ArrayList();
        var shouldDisposeConnection = externalConnection == null;
        var connection = externalConnection ?? DbProvider?.CreateConnection();

        if (connection == null)
        {
            throw new DataAccessException(MissingDbProviderMessage);
        }

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var cmd = connection.CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = commandText;
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            if (commandTimeoutSeconds is >= 0)
            {
                cmd.CommandTimeout = commandTimeoutSeconds.Value;
            }

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var rowNum = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(rowMapper.MapRow(reader, rowNum++));
            }
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Error executing query: {ex.Message}", ex);
        }
        finally
        {
            if (shouldDisposeConnection)
            {
                connection.Dispose();
            }
        }

        return results;
    }

    /// <summary>
    ///     Executes a SQL query asynchronously using the specified command type and command text. The result set is
    ///     mapped to a collection of objects using the provided row mapper implementation.
    /// </summary>
    /// <param name="commandType">Specifies how the command text is interpreted (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to be executed.</param>
    /// <param name="rowMapper">An implementation of <see cref="IRowMapper" /> used to map query result rows to objects.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is a collection of objects mapped by the
    ///     row mapper.
    /// </returns>
    /// <exception cref="DataAccessException">
    ///     Thrown if an error occurs during query execution or mapping of result rows.
    /// </exception>
    // Synchronous wrappers to support legacy tests that mock synchronous behavior
    public virtual IList QueryWithRowMapperAsync(CommandType commandType, string commandText, IRowMapper rowMapper)
    {
        return QueryWithRowMapperAsync(commandType, commandText, rowMapper, null, null, null, CancellationToken.None)
            .GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Asynchronously executes a query and maps the resulting data rows to a list of objects using the specified row
    ///     mapper.
    /// </summary>
    /// <param name="commandType">The type of the SQL command (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to execute.</param>
    /// <param name="rowMapper">The row mapper used to map each data row to a specific object.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a</returns>
    public virtual IList QueryWithRowMapperAsync(CommandType commandType, string commandText, IRowMapper rowMapper,
        CancellationToken cancellationToken)
    {
        return QueryWithRowMapperAsync(commandType, commandText, rowMapper, null, null, null, cancellationToken)
            .GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Asynchronously executes a SQL query using the specified command type, command text, and row mapper, and maps the
    ///     resulting rows into a list.
    /// </summary>
    /// <param name="commandType">The type of the SQL command (e.g., Text, StoredProcedure).</param>
    /// <param name="commandText">The SQL command or stored procedure to execute.</param>
    /// <param name="rowMapper">
    ///     An implementation of the IRowMapper interface used to map each row of the result set to an
    ///     object.
    /// </param>
    /// <param name="externalConnection">
    ///     An optional external database connection to use. If null, a connection will be created
    ///     using the DbProvider.
    /// </param>
    /// <param name="transaction">An optional database transaction to associate with the query.</param>
    /// <param name="commandTimeoutSeconds">
    ///     An optional timeout value in seconds for the query execution. If null or less than
    ///     0, the default value will be used.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of objects mapped from the
    ///     query results.
    /// </returns>
    /// <exception cref="DataAccessException">
    ///     Thrown when an error occurs during the asynchronous query execution, such as a database connection error or query
    ///     execution failure.
    /// </exception>
    public virtual IList QueryWithRowMapperAsync(CommandType commandType, string commandText, IRowMapper rowMapper,
        DbConnection? externalConnection, DbTransaction? transaction, int? commandTimeoutSeconds)
    {
        return QueryWithRowMapperAsync(commandType, commandText, rowMapper, externalConnection, transaction,
                commandTimeoutSeconds, CancellationToken.None)
            .GetAwaiter().GetResult();
    }
}
