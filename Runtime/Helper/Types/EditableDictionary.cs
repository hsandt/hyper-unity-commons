using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HyperUnityCommons
{
    /// Class that implements IReadOnlyDictionary, but that can be serialized and edited in inspector
    /// as a list of key-value pairs.
    /// It works by caching a dictionary at runtime from the key-value pairs.
    /// However, user needs to call InitCache before any usage. There is no lazy initialization done automatically.
    /// Caution: it doesn't detect runtime changes, so you will need to call InitCache again if you change some entries
    /// at runtime (via the inspector for debugging, or via code).
    [Serializable]
    public class EditableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        [SerializeField, Tooltip("List of key value pairs used to generate the dictionary. InitCache must be called " +
             "once at runtime before the first usage, and after any runtime change of this list.")]
        private List<KeyValuePair<TKey, TValue>> m_KeyValuePairs = new();

        /// Generated cached dictionary
        private Dictionary<TKey, TValue> m_CachedDictionary = new();


        public void InitCache(Object context = null)
        {
            // Clearing cached dictionary is required in Editor, as Scriptable object non-serialized fields are still
            // preserved in memory between Play in Editor sessions, causing dictionary to keep existing entries.
            // In build, it's not required as this method is only called once, but it's not a bad idea to make it
            // idempotent (assuming m_KeyValuePairs don't change) either, so no need to strip this code outside
            // UNITY_EDITOR.
            m_CachedDictionary.Clear();

            for (int i = 0; i < m_KeyValuePairs.Count; i++)
            {
                (TKey key, TValue value) = m_KeyValuePairs[i];

                if (key != null)
                {
                    bool success = m_CachedDictionary.TryAdd(key, value);

                    if (!success)
                    {
                        DebugUtil.LogErrorFormat(context,
                            "[EditableDictionary] InitCache: could not add key " +
                            "m_KeyValuePairs[{0}] = {1} to cached dictionary. It seems that another entry in " +
                            "m_KeyValuePairs list already added the same key.",
                            i, key);
                    }
                }
                else
                {
                    DebugUtil.LogErrorFormat(context,
                        "[EditableDictionary] InitCache: m_KeyValuePairs[{0}] is null, " +
                        "cannot add as key to cached dictionary",
                        i);
                }
            }
        }


        /* IReadOnlyDictionary */

        public IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_CachedDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => m_CachedDictionary.Count;

        public bool ContainsKey(TKey key)
        {
            return m_CachedDictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_CachedDictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => m_CachedDictionary[key];
            set => m_CachedDictionary[key] = value;
        }

        public IEnumerable<TKey> Keys => m_CachedDictionary.Keys;
        public IEnumerable<TValue> Values => m_CachedDictionary.Values;
    }
}
