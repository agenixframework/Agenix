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
using Microsoft.Data.Sqlite;

namespace Agenix.Sql.Ado;

/// <summary>
///     Provides factory methods for creating database provider instances that implement the <see cref="IDbProvider" />
///     interface.
/// </summary>
/// <remarks>
///     This static class is used to abstract the creation of database provider instances.
///     It enables support for different database providers by providing implementations of the <see cref="IDbProvider" />
///     interface
///     based on a specified provider name.
/// </remarks>
public static class DbProviderFactory
{
    /// <summary>
    ///     Retrieves an implementation of the <see cref="IDbProvider" /> interface based on the given provider name.
    /// </summary>
    /// <param name="providerName">
    ///     The name of the database provider for which an instance of <see cref="IDbProvider" /> is to be created.
    ///     The supported value is "Microsoft.Data.Sqlite".
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IDbProvider" /> that corresponds to the specified provider name.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the provided <paramref name="providerName" /> is not supported.
    /// </exception>
    public static IDbProvider GetDbProvider(string providerName)
    {
        return string.Equals(providerName, "Microsoft.Data.Sqlite", StringComparison.OrdinalIgnoreCase)
            ? new SqliteDbProvider()
            : throw new NotSupportedException($"Provider '{providerName}' is not supported by Agenix.Sql ADO factory.");
    }

    private sealed class SqliteDbProvider : IDbProvider
    {
        public string? ConnectionString { get; set; }

        public DbConnection CreateConnection()
        {
            return new SqliteConnection(ConnectionString);
        }
    }
}
