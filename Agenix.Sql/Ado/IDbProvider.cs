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

using System.Data.Common;

namespace Agenix.Sql.Ado;

/// <summary>
///     Defines a contract for database providers, enabling creation and management
///     of database connections in an abstracted manner.
/// </summary>
public interface IDbProvider
{
    /// <summary>
    ///     Gets or sets the connection string used to establish a connection to the database.
    /// </summary>
    /// <remarks>
    ///     The value of this property represents the details required to connect to a specific database,
    ///     such as the server address, database name, authentication credentials, and other connection parameters.
    /// </remarks>
    string? ConnectionString { get; set; }

    /// <summary>
    ///     Creates and returns a new instance of a database connection.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.Data.Common.DbConnection" /> object representing the database connection.
    /// </returns>
    DbConnection CreateConnection();
}
