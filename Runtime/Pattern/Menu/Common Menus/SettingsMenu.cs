using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HyperUnityCommons
{
    public class SettingsMenu : MenuBehaviour
    {
        [Header("Child references")]

        [Tooltip("Parent to place setting entries under")]
        public Transform settingsParent;

        [Tooltip("(Optional) Reset to defaults button")]
        [FormerlySerializedAs("buttonDefault")]
        public Button buttonResetToDefaults;

        [Tooltip("Back button")]
        public Button buttonBack;


        /* Cached child references */

        /// Array of base setting labels found under this game object
        private List<BaseSettingLabel> m_SettingLabels = new();


        private void Awake()
        {
            DebugUtil.AssertFormat(settingsParent != null, this, "[OptionsMenu] Awake: Settings Parent not set on {0}", this);
            DebugUtil.AssertFormat(buttonBack != null, this, "[OptionsMenu] Awake: Button Back not set on {0}", this);

            if (buttonResetToDefaults != null)
            {
                buttonResetToDefaults.onClick.AddListener(ResetToDefaults);
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

            // Fix navigation (we assume setting labels are placed vertically, with optionally reset to defaults button
            // below, and back at the bottom)
            if (m_SettingLabels.Count > 0)
            {
                Navigation lastSettingLabelNavigation = m_SettingLabels[^1].navigation;
                lastSettingLabelNavigation.mode = Navigation.Mode.Explicit;
                lastSettingLabelNavigation.selectOnDown = buttonResetToDefaults != null ? buttonResetToDefaults : buttonBack;

                if (m_SettingLabels.Count > 1)
                {
                    lastSettingLabelNavigation.selectOnUp = m_SettingLabels[^2];
                }

                m_SettingLabels[^1].navigation = lastSettingLabelNavigation;

                if (buttonResetToDefaults != null)
                {
                    DebugUtil.AssertFormat(buttonResetToDefaults.navigation.mode == Navigation.Mode.Explicit,
                        buttonResetToDefaults,
                        "[SettingsMenu] CreateAllSettingLabels: buttonResetToDefaults navigation mode is not Explicit, " +
                        "navigation fix will be ignored");
                    DebugUtil.AssertFormat(buttonResetToDefaults.navigation.selectOnDown == buttonBack,
                        "[SettingsMenu] CreateAllSettingLabels: buttonResetToDefaults navigation selectOnDown is not " +
                        "buttonBack");

                    Navigation buttonResetToDefaultsNavigation = buttonResetToDefaults.navigation;
                    buttonResetToDefaultsNavigation.selectOnUp = m_SettingLabels[^1];
                    buttonResetToDefaults.navigation = buttonResetToDefaultsNavigation;

                    DebugUtil.AssertFormat(buttonBack.navigation.mode == Navigation.Mode.Explicit,
                        buttonBack,
                        "[SettingsMenu] CreateAllSettingLabels: buttonBack navigation mode is not Explicit, " +
                        "navigation fix will be ignored");

                    Navigation buttonBackNavigation = buttonBack.navigation;
                    buttonBackNavigation.selectOnUp = buttonResetToDefaults;
                    buttonBackNavigation.selectOnDown = m_SettingLabels[0];
                    buttonBack.navigation = buttonBackNavigation;
                }
                else
                {
                    DebugUtil.AssertFormat(buttonBack.navigation.mode == Navigation.Mode.Explicit,
                        buttonBack,
                        "[SettingsMenu] CreateAllSettingLabels: buttonBack navigation mode is not Explicit, " +
                        "navigation fix will be ignored");

                    Navigation buttonBackNavigation = buttonBack.navigation;
                    buttonBackNavigation.selectOnUp = m_SettingLabels[^1];
                    buttonBackNavigation.selectOnDown = m_SettingLabels[0];
                    buttonBack.navigation = buttonBackNavigation;
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
            if (buttonResetToDefaults != null)
            {
                buttonResetToDefaults.onClick.RemoveAllListeners();
            }
        }

        public override Selectable GetInitialSelection() => m_SettingLabels.Count > 0 ? m_SettingLabels[0] : buttonBack;

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
