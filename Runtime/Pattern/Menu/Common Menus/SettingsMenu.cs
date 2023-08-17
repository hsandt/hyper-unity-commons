using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HyperUnityCommons
{
    public class SettingsMenu : MenuBehaviour
    {
        [Header("Child references")]

        [Tooltip("Initial selection (if not set, use Button Back)")]
        public Selectable initialSelectable;

        [Tooltip("Parent to place setting entries under")]
        public Transform settingsParent;

        [Tooltip("Default button")]
        public Button buttonDefault;

        [Tooltip("Back button")]
        public Button buttonBack;


        /* Cached child references */

        /// Array of base setting labels found under this game object
        private List<BaseSettingLabel> m_SettingLabels = new();


        private void Awake()
        {
            DebugUtil.AssertFormat(settingsParent != null, this, "[OptionsMenu] Awake: Settings Parent not set on {0}", this);
            DebugUtil.AssertFormat(buttonBack != null, this, "[OptionsMenu] Awake: Button Back not set on {0}", this);

            if (buttonDefault != null)
            {
                buttonDefault.onClick.AddListener(ResetToDefaults);
            }
            buttonBack.onClick.AddListener(GoBack);

            CreateAllSettingLabels();
        }

        private void CreateAllSettingLabels()
        {
            List<BaseSettingData> settingDataList = SettingsManager.Instance.settingDataList.entries;

            // Set capacity to avoid reallocations later
            m_SettingLabels.Capacity = settingDataList.Count;

            foreach (BaseSettingData settingData in settingDataList)
            {
                // Instantiate setting label from prefab (label may be on child, so really instantiate object then get
                // component, as instantiating component on child would only instantiate the associated child branch)
                GameObject settingDataViewPrefab = settingData.viewPrefab;
                GameObject settingDataView = Instantiate(settingDataViewPrefab, settingsParent);

                var settingLabel = settingDataView.GetComponentInChildren<BaseSettingLabel>();
                if (settingLabel != null)
                {
                    // Inject setting data and initialize content based on it
                    settingLabel.SetSettingData(settingData);
                    settingLabel.Init();

                    // Add to list
                    m_SettingLabels.Add(settingLabel);
                }
                else
                {
                    Debug.LogErrorFormat(settingDataViewPrefab,
                        "[SettingsMenu] CreateAllSettingLabels: could not find BaseSettingLabel component on " +
                        "instance of viewPrefab {0} for settingData {1}",
                        settingDataViewPrefab, settingData);
                }
            }
        }

        private void SetupAllSettingLabels()
        {
            foreach (BaseSettingLabel settingLabel in m_SettingLabels)
            {
                settingLabel.Setup();
            }
        }

        private void OnDestroy()
        {
            if (buttonBack)
            {
                buttonBack.onClick.RemoveAllListeners();
            }
            if (buttonDefault != null)
            {
                buttonDefault.onClick.RemoveAllListeners();
            }
        }

        public override Selectable GetInitialSelection() => initialSelectable != null ? initialSelectable : buttonBack;

        protected override void OnShow()
        {
            gameObject.SetActive(true);
            SetupAllSettingLabels();
        }

        public override void Hide()
        {
            EventSystem.current.SetSelectedGameObject(null);

            gameObject.SetActive(false);
        }

        public override bool CanGoBack()
        {
            return true;
        }

        private void ResetToDefaults()
        {
            // Reset all settings in model
            SettingsManager.Instance.ResetAllSettingsToDefaultValues(immediatelySavePreference: true);

            // Update view to show new values
            SetupAllSettingLabels();
        }

        private void GoBack()
        {
            m_MenuSystem.GoBackToPreviousMenu();
        }
    }
}
