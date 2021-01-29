using System.Collections.Generic;

namespace MPP.Acceptance.Test.API.Specs.Support.Session
{
    public interface ISessionMap<TK, TV> : IDictionary<TK, TV>
    {
        Dictionary<string, string> GetMetaData();
        void AddMetaData(string key, string value);
        void ClearMetaData();
        void ShouldContainKey(TK key);
    }
}