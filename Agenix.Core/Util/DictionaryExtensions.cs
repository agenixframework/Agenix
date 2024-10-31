using System.Collections.Generic;

namespace Agenix.Core.Util;

public static class DictionaryExtensions
{
    public static void SafeDictionaryAdd(Dictionary<string, object> dict, string key, object view)
    {
        dict[key] = view;
    }

    public static Dictionary<TKey, TVal> Merge<TKey, TVal>(this Dictionary<TKey, TVal> dictA,
        Dictionary<TKey, TVal> dictB)
    {
        var output = new Dictionary<TKey, TVal>(dictA);

        foreach (var (key, value) in dictB) output[key] = value;

        return output;
    }
}