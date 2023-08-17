using System.Collections;
using System.Collections.Generic;
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

        [Tooltip("Default button")]
        public Button buttonDefault;

        [Tooltip("Back button")]
        public Button buttonBack;


        /* Cached child references */

        /// Array of base setting labels found under this game object
        private BaseSettingLabel[] m_SettingLabels;


        private void Awake()
        {
            DebugUtil.AssertFormat(buttonBack != null, this, "[OptionsMenu] Awake: Button Back not set on {0}", this);

            if (buttonDefault != null)
            {
                buttonDefault.onClick.AddListener(ResetToDefaults);
            }
            buttonBack.onClick.AddListener(GoBack);

            // Retrieve all setting labels (include inactive as this is done very early)
            m_SettingLabels = GetComponentsInChildren<BaseSettingLabel>(true);
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
