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

namespace Agenix.Sql.Ado.Exceptions;

/// <summary>
///     Represents an exception that occurs during data access operations in the ADO.NET template layer.
/// </summary>
/// <remarks>
///     This exception is typically thrown when there are issues related to database interactions,
///     such as connection failures, command execution errors, or misconfigurations in the database provider.
///     It provides additional context for errors encountered during the execution of database operations.
/// </remarks>
/// <example>
///     This exception is often used in conjunction with the <see cref="AdoTemplate" /> class
///     to handle errors during execution of methods like ExecuteNonQuery and QueryWithRowMapper.
/// </example>
public class DataAccessException : Exception
{
    /// <summary>
    ///     Represents an exception that occurs during data access operations in the ADO.NET template layer.
    /// </summary>
    /// <remarks>
    ///     This exception is typically thrown when there are issues related to database interactions,
    ///     such as connection failures, command execution errors, or misconfigurations in the database provider.
    ///     It provides additional context for errors encountered during the execution of database operations.
    /// </remarks>
    public DataAccessException()
    {
    }

    /// <summary>
    ///     Represents an exception that is thrown when issues arise during data access operations within the ADO.NET template
    ///     layer.
    /// </summary>
    /// <remarks>
    ///     This exception class is used to encapsulate and provide additional context on errors encountered during database
    ///     interactions,
    ///     such as connection issues, command execution failures, or misconfigurations in the database provider.
    ///     It commonly surfaces in scenarios involving ADO.NET abstractions like the AdoTemplate.
    /// </remarks>
    public DataAccessException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Represents an exception that is thrown during data access operations in the ADO.NET abstraction layer.
    /// </summary>
    /// <remarks>
    ///     This exception is used to indicate issues related to database interaction, such as errors in executing commands,
    ///     transaction handling, or connection management. It provides a way to encapsulate and propagate low-level
    ///     database-related exceptions with additional context.
    /// </remarks>
    public DataAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
