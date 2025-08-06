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

namespace Agenix.Sql.Ado;

/// <summary>
///     Provides a base class for Data Access Object (DAO) support, leveraging ADO.NET functionality.
///     This class offers foundational properties for working with database providers and templates
///     in a consistent manner across implementations.
/// </summary>
public abstract class AdoDaoSupport
{
    /// <summary>
    ///     Gets or sets the database provider used to manage and create database connections.
    ///     This property allows specifying the underlying implementation of <see cref="IDbProvider" />
    ///     which provides access to database connection instances and connection string configuration.
    ///     Used internally for executing database operations.
    /// </summary>
    protected IDbProvider? DbProvider { get; set; }

    /// <summary>
    ///     Gets or sets the instance of <see cref="Agenix.Sql.Ado.AdoTemplate" /> used for executing database operations.
    ///     This property provides methods for executing queries, commands, and mapping results when interacting with
    ///     the database. It simplifies the execution of standard database operations with transaction and connection
    ///     management.
    /// </summary>
    protected AdoTemplate? AdoTemplate { get; set; }
}
