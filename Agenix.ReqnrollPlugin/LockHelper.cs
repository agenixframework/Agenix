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

using System.Collections.Concurrent;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     A helper class that provides thread-safe methods for managing lock objects.
/// </summary>
internal static class LockHelper
{
    private static readonly ConcurrentDictionary<int, object> Repository = new();

    private static readonly object GetLockLock = new();

    public static object GetLock(int hashCode)
    {
        lock (GetLockLock)
        {
            if (Repository.ContainsKey(hashCode))
            {
                return Repository[hashCode];
            }

            var lockObj = new object();
            Repository.AddOrUpdate(hashCode, lockObj, (key, oldValue) => oldValue);

            return lockObj;
        }
    }
}
