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

using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Sql.Ado;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Sql.Actions.ExecuteSqlQueryAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Sql.Tests.Sql.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(Endpoints)])]
[Platform(Exclude = "MacOSX", Reason = "Unable to load shared library 'SQLite.Interop.dll' or one of its dependencies")]
public class ExecuteSqlIT
{
    [Test]
    public async Task ExecuteSqlAction()

    {
        var adoTemplate = _context.ReferenceResolver.Resolve<AdoTemplate>("AdoTemplate");
        var scriptSqlFile = _context.ReferenceResolver.Resolve<string>("scriptSqlFile");
        var queryScriptSqlFile = _context.ReferenceResolver.Resolve<string>("queryScriptSqlFile");

        await _runner.Given(CreateVariables().Variable("customerId", "1").Variable("rowsCount", "0"));

        await _runner.Given(Agenix.Sql.Actions.ExecuteSqlAction.Builder.Sql()
            .AdoTemplate(adoTemplate)
            .SqlResource(scriptSqlFile));

        await _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .Statement("select NAME from CUSTOMERS where CUSTOMER_ID='${customerId}'")
                .Statement("select COUNT(1) as overall_cnt from ERRORS")
                .Statement("select ORDER_ID from ORDERS where DESCRIPTION LIKE 'Migrate%'")
                .Statement("select DESCRIPTION from ORDERS where ORDER_ID = 2")
                .Validate("ORDER_ID", "1")
                .Validate("NAME", "Christoph")
                .Validate("OVERALL_CNT", "${rowsCount}")
                .Validate("DESCRIPTION", "")
        );
        await _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .SqlResource(queryScriptSqlFile)
                .Validate("ORDER_ID", "1")
                .Validate("NAME", "Christoph")
                .Validate("OVERALL_CNT", "${rowsCount}")
                .Validate("DESCRIPTION", "")
        );

        await _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .Statement("select REQUEST_TAG as RTAG, DESCRIPTION as DESC from ORDERS")
                .Validate("RTAG", "requestTag", "@Ignore@")
                .Validate("DESC", "Migrate")
                .Validate("DESC", "Migrate", "")
                .Extract("RTAG", "tags")
                .Extract("DESC", "description")
        );

        await _runner.When(Agenix.Sql.Actions.ExecuteSqlAction.Builder.Sql()
            .AdoTemplate(adoTemplate)
            .Statement("DELETE FROM CUSTOMERS"));

        await _runner.Then(Query()
            .AdoTemplate(adoTemplate)
            .Statement("select DESCRIPTION as desc from ORDERS where ORDER_ID = 2")
            .Validate("DESC", "")
        );
    }
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinAsyncTestActionRunner _runner;

    [AgenixResource] private TestContext _context;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
