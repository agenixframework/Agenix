#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
