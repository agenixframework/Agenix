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

using Agenix.Api.Exceptions;

namespace Agenix.Api;

/// Represents a test class, inheriting from TestSource.
/// This class allows the creation of a test instance with
/// an optional method.
/// /
public class TestClass(Type type, string method) : TestSource(type)
{
    public TestClass(Type type) : this(type, null)
    {
    }

    // Gets the method
    public string Method => method;

    // Read String representation and construct a proper test class
    // instance. Read optional method name information and class
    // name using format "fully.qualified.class.Name#optionalMethodName()"
    public static TestClass FromString(string testClass)
    {
        try
        {
            string className;
            string methodName = null;

            if (testClass.Contains('#'))
            {
                className = testClass[..testClass.IndexOf('#', StringComparison.Ordinal)];
                methodName = testClass[(testClass.IndexOf('#') + 1)..];
            }
            else
            {
                className = testClass;
            }

            return !string.IsNullOrEmpty(methodName)
                ? new TestClass(System.Type.GetType(className), methodName)
                : new TestClass(System.Type.GetType(className));
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test class", e);
        }
    }
}
