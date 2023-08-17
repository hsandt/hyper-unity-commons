using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HyperUnityCommons
{
    /// Setting Data List
    /// References on Setting Data in order
    [CreateAssetMenu(fileName = "SettingDataList", menuName = "Settings/Setting Data List")]
    public class SettingDataList : ScriptableObject
    {
        // Note that we need BaseSettingData to add any setting data in the inspector
        // SettingData<object> would not work, as it would show all SettingData<T> in the assignment popup, but only accept
        // SettingData specifically bound to type `object`, which we never use.
        [Tooltip("List of references to Setting Data, in order of display in Settings Menu")]
        public List<BaseSettingData> entries;


        public void AssertIsValid()
        {
            // In theory we should verify that there is no redundancy, etc.
            // but the most common issue is null entry due to missing data or adding an entry and forgetting to fill it,
            // so just check this
            DebugUtil.AssertListElementsNotNull(entries, this, nameof(entries));
        }
    }
}
