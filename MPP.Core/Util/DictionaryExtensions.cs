using System.Collections.Generic;

namespace FleetPay.Core.Util
{
    public static class DictionaryExtensions
    {
        public static void SafeDictionaryAdd(Dictionary<string, object> dict, string key, object view)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, view);
            else
                dict[key] = view;
        }

        public static IDictionary<TKey, TVal> Merge<TKey, TVal>(this IDictionary<TKey, TVal> dictA,
            IDictionary<TKey, TVal> dictB)
        {
            IDictionary<TKey, TVal> output = new Dictionary<TKey, TVal>(dictA);

            foreach (var (key, value) in dictB)
                if (!output.ContainsKey(key))
                    output.Add(key, value);
                else
                    output[key] = value;

            return output;
        }
    }
}