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
using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// Represents a sequence of actions that will execute a set of operations
/// and ensures that a final operation is performed at the end.
public class FinallySequence : Sequence
{
    /// Represents a sequence of actions that will execute a set of operations
    /// and ensures that a final operation is performed at the end.
    /// /
    public FinallySequence(Builder builder) : base(
        new Sequence.Builder()
            .Description(builder.GetDescription())
            .Name(builder.GetName())
            .Actions(builder.GetActions().ToArray())
    )
    {
    }

    /// Builder class for constructing FinallySequence instances.
    /// /
    public new class Builder : AbstractTestContainerBuilder<FinallySequence, dynamic>, ITestAction
    {
        public void Execute(TestContext context)
        {
            DoBuild().Execute(context);
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @return A new instance of the Builder class for constructing FinallySequence instances.
        /// /
        public static Builder DoFinally()
        {
            return new Builder();
        }

        protected override FinallySequence DoBuild()
        {
            return new FinallySequence(this);
        }
    }
}
