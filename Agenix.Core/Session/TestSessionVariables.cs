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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Agenix.Core.Session;

internal class TestSessionVariables<TK, T> : ConcurrentDictionary<TK, T>, ISessionMap<TK, T>
{
    private readonly IDictionary<string, string> _metadata = new ConcurrentDictionary<string, string>();

    public Dictionary<string, string> GetMetaData()
    {
        return new Dictionary<string, string>(_metadata);
    }

    public void Add(TK key, T value)
    {
        if (value == null)
        {
            TryRemove(key, out value);
        }
        else
        {
            TryAdd(key, value);
        }
    }

    public void AddMetaData(string key, string value)
    {
        _metadata.Add(key, value);
    }

    public void ClearMetaData()
    {
        _metadata.Clear();
    }

    public void ShouldContainKey(TK key)
    {
        if (!ContainsKey(key))
        {
            throw new Exception("Session variable " + key + " expected but not found.");
        }
    }

    public new void Clear()
    {
        ClearMetaData();
        base.Clear();
    }
}
