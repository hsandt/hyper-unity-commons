using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace HyperUnityCommons
{
    /// Class that implements IReadOnlyDictionary, but that can be serialized and edited in inspector
    /// as a list of key-value pairs.
    /// It works by caching a dictionary at runtime from the key-value pairs.
    /// However, user needs to call some variant of InitCache at least once before the first usage, as there is no lazy
    /// initialization done automatically.
    /// We recommend centralizing access and initialization of the EditableDictionary in one place, so you only need to
    /// call InitCache once. However, if this is not possible (e.g. when many SO can access the EditableDictionary and
    /// it is difficult to retrieve), then you should call TryInitCache before every usage.
    /// Caution: it doesn't detect Inspector changes at runtime, so you will need to call ForceInitCache again if you
    /// change some entries via the inspector while playing. It doesn't have an API to modify key value pairs at runtime,
    /// so there is no risk modifying them by code.
    [Serializable]
    public class EditableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        [SerializeField, Tooltip("List of key value pairs used to generate the dictionary. InitCache must be called " +
             "once at runtime before the first usage, and after any runtime change of this list.")]
        private List<KeyValuePair<TKey, TValue>> m_KeyValuePairs = new();


        /* Cache */

        /// Generated cached dictionary
        private Dictionary<TKey, TValue> m_CachedDictionary = new();

        /// True when the cached dictionary has been initialized at least once
        private bool m_Initialized = false;

        /// A proxy for the initialized flag that takes into account the case when playing in editor with
        /// Enter Play Mode Options with Reload Domain disabled.
        /// Indeed, EditableDictionary may be used inside a ScriptableObject, which is known to save private fields
        /// across Play in Editor sessions, and, in the case where Reload Domain is disabled, SO even save
        /// [NonSerialized] fields, causing cached flags to already be set on next Play.
        /// Therefore, m_Initialized is only reliable in one of those situations:
        /// a. this class is not embedded inside a ScriptableObject
        /// b. we are playing a build
        /// c. we are playing in the editor, not using Enter Play Mode Options, or at least with Reload Domain still
        /// enabled
        private bool IsGuaranteedInitialized
        {
            get
            {
                // We cannot check if we are embedded in a SO or not, so we assume the worst, that we are in an SO,
                // and only check cases b. and c.
                #if UNITY_EDITOR
                if (EditorSettings.enterPlayModeOptionsEnabled &&
                    EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload))
                {
                    // Here, m_Initialized is not reliable, the situation is Unknown, so return false (no guarantee)
                    return false;
                }
                #endif

                // We are either in build or reloading domain, so we can trust the cached flag
                return m_Initialized;
            }
        }

        /// Similar to IsGuaranteedInitialized, but used by assertions
        /// It is not necessarily equal to !IsGuaranteedInitialized, because of the uncertainty brought by not Reloading
        /// Domain, so the initialization is a kind of ternary state: Guaranteed Initialized, Guaranteed Uninitialized,
        /// and Unknown, in which case both properties return false, so we must always re-initialize when asked to, but
        /// we don't error on assertions either.
        private bool IsGuaranteedUninitialized
        {
            get
            {
                #if UNITY_EDITOR
                if (EditorSettings.enterPlayModeOptionsEnabled &&
                    EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload))
                {
                    // Here, m_Initialized is not reliable, the situation is Unknown, so return false (no guarantee)
                    return false;
                }
                #endif

                // We are either in build or reloading domain, so we can trust the cached flag
                return !m_Initialized;
            }
        }

        /// Initialize cache
        /// This should only be called once before usage
        public void InitCache(Object context = null, bool errorOnNullValue = false)
        {
            if (IsGuaranteedInitialized)
            {
                DebugUtil.LogErrorFormat("[EditableDictionary] InitCache: already initialized. " +
                    "If you need to force initialize after some changes in the inspector, call ForceInitCache instead. " +
                    "If you need to lazily initialize without error if already initialized, call TryInitCache.");
                return;
            }

            InitCache_Internal(context, errorOnNullValue);
        }

        /// Initialize cache if not already initialized, else do nothing
        public void TryInitCache(Object context = null, bool errorOnNullValue = false)
        {
            if (IsGuaranteedInitialized)
            {
                return;
            }

            InitCache_Internal(context, errorOnNullValue);
        }

        /// Initialize cache from scratch, whatever it was before
        /// Useful to call after changing key value paris in the inspector
        public void ForceInitCache(Object context = null, bool errorOnNullValue = false)
        {
            InitCache_Internal(context, errorOnNullValue);
        }

        private void InitCache_Internal(Object context = null, bool errorOnNullValue = false)
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

                // Remember that C# doesn't allow null keys, and it would likely be an entry the designer forgot
                // to fill anyway
                if (key != null)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (errorOnNullValue)
                    {
                        if (value == null)
                        {
                            // True null: this can happen with dictionary storing pure object values, but it should not
                            // really happen in our case since this component is made for edit in Inspector, which means
                            // we must be dealing with UnityEngine.Object (see case below)
                            Debug.LogErrorFormat("[EditableDictionary] InitCache: value for key " +
                                "m_KeyValuePairs[{0}].key = {1} is null. It will still be added to cached dictionary, but caller " +
                                "passed errorOnNullValue = true, so this must be invalid data.",
                            i, key);
                        }
                        // Just like AssertDictionaryElementsNotNull, we need to check for dictionary entries
                        // (with Object type) that are not truly null, but some dummy Object with instance ID = 0,
                        // to allow showing an UnassignedReferenceException with details on which field is undefined
                        // to the developer.
                        else if (value is Object objectValue && objectValue.GetInstanceID() == 0)
                        {
                            Debug.LogErrorFormat("[EditableDictionary] InitCache: value for key " +
                                "m_KeyValuePairs[{0}].key = {1} is undefined/missing. It will still be added to cached dictionary, but caller " +
                                "passed errorOnNullValue = true, so this must be invalid data.",
                                i, key);
                        }
                    }
                    #endif

                    bool success = m_CachedDictionary.TryAdd(key, value);

                    if (!success)
                    {
                        DebugUtil.LogErrorFormat(context,
                            "[EditableDictionary] InitCache: could not add key " +
                            "m_KeyValuePairs[{0}].key = {1} to cached dictionary. It seems that another entry in " +
                            "m_KeyValuePairs list already added the same key.",
                            i, key);
                    }
                }
                else
                {
                    DebugUtil.LogErrorFormat(context,
                        "[EditableDictionary] InitCache: m_KeyValuePairs[{0}].key is null, " +
                        "cannot add as key to cached dictionary",
                        i);
                }
            }

            m_Initialized = true;
        }


        /* IReadOnlyDictionary */

        public IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Count;
            }
        }

        public bool ContainsKey(TKey key)
        {
            DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary[key];
            }
            set
            {
                DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
                m_CachedDictionary[key] = value;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                DebugUtil.AssertFormat(!IsGuaranteedUninitialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Values;
            }
        }
    }
}
