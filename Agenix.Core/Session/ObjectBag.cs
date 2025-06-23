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
