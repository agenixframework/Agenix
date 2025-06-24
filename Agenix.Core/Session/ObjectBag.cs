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

using System.Threading;

namespace Agenix.Core.Session;

public class ObjectBag
{
    private static readonly ThreadLocal<TestSessionVariables<object, object>> TestSessionThreadLocal = new();

    public static ISessionMap<object, object> GetCurrentSession()
    {
        return TestSessionThreadLocal.Value ??
               (TestSessionThreadLocal.Value = new TestSessionVariables<object, object>());
    }

    public static T SessionVariableCalled<T>(object key)
    {
        GetCurrentSession().TryGetValue(key, out var o);
        return (T)o;
    }

    public static SessionVariableSetter SetSessionVariable(object key)
    {
        return new SessionVariableSetter(key);
    }

    public static void ClearCurrentSession()
    {
        GetCurrentSession().Clear();
    }

    public static bool HasASessionVariableCalled(object key)
    {
        return GetCurrentSession().ContainsKey(key);
    }

    public class SessionVariableSetter
    {
        private readonly object _key;

        public SessionVariableSetter(object key)
        {
            _key = key;
        }

        public void To<T>(T value)
        {
            if (value != null)
            {
                GetCurrentSession().Add(_key, value);
            }
            else
            {
                GetCurrentSession().Remove(_key);
            }
        }
    }
}
