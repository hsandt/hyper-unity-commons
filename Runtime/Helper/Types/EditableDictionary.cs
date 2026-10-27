using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

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
        #if ODIN_INSPECTOR
        [OnInspectorInit("@$property.State.Expanded = true")]
        #endif
        private List<KeyValuePair<TKey, TValue>> m_KeyValuePairs = new();


        /* Cache */

        /// Generated cached dictionary
        private Dictionary<TKey, TValue> m_CachedDictionary = new();

        public Dictionary<TKey, TValue> CachedDictionary => m_CachedDictionary;

        /// True when the cached dictionary has been initialized at least once
        private bool m_Initialized = false;

        /// Initialize cache
        /// This should only be called once before usage
        /// Use this when you have one clear owner for the editable dictionary
        public void InitCache(Object context = null, bool errorOnNullValue = false)
        {
            bool success = TryInitCache(context, errorOnNullValue);
            if (!success)
            {
                DebugUtil.LogErrorFormat("[EditableDictionary] InitCache: TryInitCache failed, so cache is already flagged as initialized. " +
                    "Make sure that Editor is not set to Recompile and Continue Playing. " +
                    "If you need to force initialize after some changes in the inspector, call ForceInitCache instead. " +
                    "If you need to lazily initialize without error if already initialized, call TryInitCache.");
            }
        }

        /// Initialize cache if not already initialized, else do nothing
        /// Use this when you have multiple owners for the editable dictionary and don't know which one will access
        /// it first, so each of them must be able to initialize it without error if already initialized
        public bool TryInitCache(Object context = null, bool errorOnNullValue = false)
        {
            if (m_Initialized)
            {
                return false;
            }

            InitCache_Internal(context, errorOnNullValue);
            return true;
        }

        /// Initialize cache from scratch, whatever it was before
        /// Currently unused, but useful if you write a custom editor to support live editing
        /// of key value pairs in the inspector at runtime, to immediately refresh cached dictionary content
        public void ForceInitCache(Object context = null, bool errorOnNullValue = false)
        {
            m_CachedDictionary.Clear();

            InitCache_Internal(context, errorOnNullValue);
        }

        private void InitCache_Internal(Object context = null, bool errorOnNullValue = false)
        {
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
                        // (with Object type) that are not truly null, but some dummy Object with instance ID = 0
    					// In Unity 6.4, GetInstanceID is deprecated but we kept the closest check using EntityID we could
                        else if (value is Object objectValue && !objectValue.GetEntityId().IsValid())
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

            // Remember to clear content and initialized flag on Exit Play mode, as even non-serialized fields
            // are preserved over play sessions when:
            // 1. playing in the Editor
            // 2. disabling Domain Reload
            // 3. with serialized data stored on a Scriptable Object
            // Make sure to bind the callback even when not using Disable Domain Reload, because user may activate
            // Disable Domain Reload between two play sessions, and then we still need to have cleared the initialized flag
            // See https://forum.unity.com/threads/scriptableobject-is-it-supposed-to-save-its-state-or-isnt-it.80777/
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EditorOnPlayModeStateChangedWhenInitialized;
            #endif
        }

        #if UNITY_EDITOR
        private void EditorOnPlayModeStateChangedWhenInitialized(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                m_CachedDictionary.Clear();
                m_Initialized = false;
                EditorApplication.playModeStateChanged -= EditorOnPlayModeStateChangedWhenInitialized;
            }
        }
        #endif

        public static EditableDictionary<TKey, TValue> FromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            EditableDictionary<TKey, TValue> editableDictionary = new();

            foreach ((TKey key, TValue value) in dictionary)
            {
                editableDictionary.m_KeyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
            }

            return editableDictionary;
        }


        /* IReadOnlyDictionary */

        public IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Count;
            }
        }

        public bool ContainsKey(TKey key)
        {
            DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
            return m_CachedDictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary[key];
            }
            set
            {
                DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
                m_CachedDictionary[key] = value;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                DebugUtil.AssertFormat(m_Initialized, "[EditableDictionary] GetEnumerator: not initialized");
                return m_CachedDictionary.Values;
            }
        }
    }
}
