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

using System.Reflection;
using Agenix.Api.Container;
using Agenix.Api.Spi;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Sql.Actions;
using Agenix.Sql.Ado;

namespace Agenix.Sql.Tests.Sql.Integration;

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

    [BindToRegistry(Name = "DBProvider")] private static readonly IDbProvider DbProvider = CreateDbProvider();

    [BindToRegistry(Name = "AdoTemplate")] private AdoTemplate _adoTemplate = new() { DbProvider = DbProvider };

    private static IDbProvider CreateDbProvider()
    {
        var dbProvider = DbProviderFactory.GetDbProvider("Microsoft.Data.Sqlite");
        const string connectionString = "Data Source=InMemory;Mode=Memory;Cache=Shared";

        dbProvider.ConnectionString = connectionString;
        return dbProvider;
    }

    private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
    private static readonly string TestDirectory = Path.GetDirectoryName(AssemblyLocation);

    [BindToRegistry(Name = "queryScriptSqlFile")]
    private readonly string queryScriptSqlFile =
        "file://" + TestDirectory + @"/ResourcesTest/Sql/Integration/Actions/query-script.sql";

    [BindToRegistry(Name = "scriptSqlFile")]
    private readonly string scriptSqlFile =
        "file://" + TestDirectory + @"/ResourcesTest/Sql/Integration/Actions/script.sql";

    private readonly string createTablesScriptSqlFile =
        "file://" + TestDirectory + @"/ResourcesTest/Sql/Integration/Actions/create-tables.sql";

    [BindToRegistry]
    private IBeforeTest BeforeTest()
    {
        return SequenceBeforeTest.Builder.BeforeTest()
            .Actions(ExecuteSqlAction.Builder.Sql(DbProvider)
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
