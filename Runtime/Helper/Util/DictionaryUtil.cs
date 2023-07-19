using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    public static class DictionaryUtil
    {
        /// Return a shallow clone of passed dictionary
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            // Construct new dictionary, setting capacity to count to avoid re-allocations during loop
            Dictionary<TKey, TValue> cloneDictionary = new(dictionary.Count);

            // Copy entries one by one
            foreach ((TKey key, TValue value) in dictionary)
            {
                cloneDictionary.Add(key, value);
            }

            return cloneDictionary;
        }
    }
}
