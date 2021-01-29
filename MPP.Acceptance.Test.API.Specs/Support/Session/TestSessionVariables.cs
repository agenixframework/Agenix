using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MPP.Acceptance.Test.API.Specs.Support.Session
{
    internal class TestSessionVariables<TK, T> : ConcurrentDictionary<TK, T>, ISessionMap<TK, T>
    {
        private readonly IDictionary<string, string> _metadata = new ConcurrentDictionary<string, string>();

        public Dictionary<string, string> GetMetaData()
        {
            return new(_metadata);
        }

        public void Add(TK key, T value)
        {
            if (value == null)
                TryRemove(key, out value);
            else
                TryAdd(key, value);
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
            if (!ContainsKey(key)) throw new Exception("Session variable " + key + " expected but not found.");
        }

        public new void Clear()
        {
            ClearMetaData();
            base.Clear();
        }
    }
}