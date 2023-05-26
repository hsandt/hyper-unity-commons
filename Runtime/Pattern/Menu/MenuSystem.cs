using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

#if UNITY_EXTENSIONS_INSPECT_IN_LINE
using UnityExtensions;
#endif

namespace HyperUnityCommons
{
    /// Menu System
    /// Manages menu navigation across various sub-menus (each having a MenuBehaviour component)
    /// Not a singleton for more flexibility
    /// SEO: before all MenuBehaviour scripts
    public class MenuSystem : MonoBehaviour
    {
        [Header("Parameter assets")]

        [Tooltip("Menu System Parameters asset")]
        #if UNITY_EXTENSIONS_INSPECT_IN_LINE
        [InspectInline(canEditRemoteTarget = true)]
        #endif
        public MenuSystemParameters menuSystemParameters;


        [Header("Scene references (required)")]

        [Tooltip("Parent of controlled menu objects. By convention, the first child with a Menu component " +
            "must be the top menu")]
        public Transform menusParent;


        [Header("Scene references (optional)")]

        [Tooltip("Selector rect transform (you need to put at least one instance of selectable " +
            "and put the Command Selector instance under it with the appropriate margins")]
        [FormerlySerializedAs("selectorTransform")]
        public RectTransform selectorRectTransform;


        /* Cached references */

        /// List of menus
        /// By convention, the first one must be the top menu
        private MenuBehaviour[] m_Menus;


        /* State */

        private readonly Stack<MenuBehaviour> m_MenuStack = new();

        public int MenuCount => m_MenuStack.Count;

        /// Getter: return current menu
        /// UB unless menu stack is not empty
        public MenuBehaviour CurrentMenu => m_MenuStack.Peek();

        private bool m_HasNotDoneInitialSelectionOnCurrentMenu = false;


        private void Awake()
        {
            DebugUtil.AssertFormat(menuSystemParameters != null, this, "[MenuSystem] Awake: Menu System Parameters asset is not set on {0}", this);
            DebugUtil.AssertFormat(menusParent != null, this, "[MenuSystem] Awake: Menu Parent transform is not set on {0}", this);

            DebugUtil.AssertFormat(menuSystemParameters.OLD_bgm == null, menuSystemParameters,
                "[MenuSystem] Awake: Menu System Parameters still uses OLD_bgm, move it to new Bgm Wrapper on {0}", menuSystemParameters);
        }

        private void Start()
        {
            // Note that we prefer hiding things in Start than in Awake,
            // because SEO guarantees this class is initialized before any menu, so Awake would be too early,
            // while Start allows menus to call their own Awake (or OnEnable) to setup things and assert on bad things early.
            // ! This means menus should *not* rely on their Start to initialize things required before their Show is called !

            // Store, init and hide all menus
            if (menusParent != null)
            {
                // Make sure to find inactive objects too since we sometimes deactivate menus to see things better
                // in the Scene editor (although normally we should only toggle visibility to avoid diff in VCS)
                m_Menus = menusParent.GetComponentsInChildren<MenuBehaviour>(true);

                DebugUtil.AssertFormat(m_Menus.Length > 0, menusParent,
                    "No Menu component found under {0}, so no top menu",
                    menusParent);

                foreach (MenuBehaviour menu in m_Menus)
                {
                    // Inject back-reference
                    menu.Init(this);

                    // Do not call Hide() which may contain some animation, immediately deactivate instead
                    menu.gameObject.SetActive(false);

                    // Initialize all MenuSelectableFeedback components with reference to this
                    var menuSelectableFeedbacks = menu.GetComponentsInChildren<MenuSelectableFeedback>();
                    foreach (MenuSelectableFeedback menuSelectableFeedback in menuSelectableFeedbacks)
                    {
                        menuSelectableFeedback.Init(this);
                    }
                }
            }
        }

        private void InstantExitAllMenus()
        {
            foreach (MenuBehaviour menu in m_Menus)
            {
                // Do not call Hide() which may contain some animation, immediately deactivate instead
                menu.gameObject.SetActive(false);
            }

            // Clear stack accordingly
            m_MenuStack.Clear();
        }

        /// Play menu BGM
        public void PlayMenuBGM()
        {
            if (menuSystemParameters != null)
            {
                // Play main menu BGM if any
                MusicManager.Instance.PlayBgmWrapperIfAny(menuSystemParameters.bgmWrapper);
            }
        }

        /// Show top menu
        public MenuBehaviour ShowTopMenu()
        {
            if (m_Menus.Length > 0)
            {
                InstantExitAllMenus();

                // Enter top menu. This will show it and set the initial selection
                MenuBehaviour topMenu = m_Menus[0];
                EnterMenu(topMenu);

                return topMenu;
            }

            return null;
        }

        public void EnterMenu(MenuBehaviour menu)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            // Check for any previous menu at the top of the stack
            if (m_MenuStack.Count > 0)
            {
                // Store last selection and hide previous menu
                MenuBehaviour previousMenuBehaviour = m_MenuStack.Peek();
                previousMenuBehaviour.StoreLastSelection();
                previousMenuBehaviour.Hide();
            }

            // Clear tracking flag for current menu as we're entering a new one
            // Do this before Show which may indirectly call OnSelect
            m_HasNotDoneInitialSelectionOnCurrentMenu = false;

            // Open = push and show next menu with initial selection
            m_MenuStack.Push(menu);
            menu.Show();
            menu.ApplyInitialSelection();
        }

        public void GoBackToPreviousMenu()
        {
            // Make sure that there are at least 2 menus:
            // the one to close, and the one to go back to
            if (m_MenuStack.Count > 1)
            {
                // Close = pop and hide current menu
                MenuBehaviour menu = m_MenuStack.Pop();
                menu.Hide();

                // Clear tracking flag for current menu as we're exiting it
                // Do this before Show which may indirectly call OnSelect
                m_HasNotDoneInitialSelectionOnCurrentMenu = false;

                // Show previous menu and restore last selection
                MenuBehaviour previousMenu = m_MenuStack.Peek();
                previousMenu.Show();
                previousMenu.RestoreLastSelection();
            }
            else
            {
                DebugUtil.LogErrorFormat("[MenuSystem] GoBackToPreviousMenu: m_MenuStack.Count is {0}, " +
                    "expected at least 2",
                    m_MenuStack.Count);
            }
        }

        /// Callback on cancel input
        /// This must be called from another script listening to PlayerInput component (or via Unity events)
        public void OnCancel()
        {
            if (m_MenuStack.Count > 1)
            {
                MenuBehaviour menu = m_MenuStack.Peek();
                if (menu.CanGoBack())
                {
                    // For now, works in all cases, but when we add special behaviour on Back
                    // like prompting for changes or auto-applying changes in Options,
                    // we may want to call some overridden OnBack method on the current menu
                    GoBackToPreviousMenu();

                    // SFX
                    PlaySfxUICancel();
                }
            }
        }

        /// Process selecting any selectable (they should all have some MenuSelectableFeedback component)
        public void OnSelect(GameObject selectedGameObject)
        {
            MoveAnySelectorToSelectable(selectedGameObject.transform);

            // Only play Select SFX if changing selection, not on the initial selection when entering a new menu
            // Note that the flag is only reliable if all selectables have some component that makes them call this method
            // on selection
            if (m_HasNotDoneInitialSelectionOnCurrentMenu)
            {
                PlaySfxUISelect();
            }
            else
            {
                // This was the (silent) initial selection, update flag so we know it's done now
                m_HasNotDoneInitialSelectionOnCurrentMenu = true;
            }
        }

        private void MoveAnySelectorToSelectable(Transform menuSelectableTransform)
        {
            if (menuSelectableTransform is RectTransform menuSelectableRectTransform)
            {
                // We don't do anything special with the fact it's a RectTransform for now, but may be useful
                // if we switch from parenting to setting canvas global position of selector directly with coordinate
                // conversion
                selectorRectTransform.SetParent(menuSelectableRectTransform, false);
            }
            else
            {
                Debug.LogErrorFormat(menuSelectableTransform,
                    "[MenuSystem] MoveAnySelectorToSelectable: menuSelectableTransform ({0}) is not " +
                    "a RectTransform",
                    menuSelectableTransform);
            }
        }


        #region Audio

        public void PlaySfxUISelect()
        {
            UISfxPoolManager.Instance.PlaySfx(menuSystemParameters.sfxUISelect, useThrottle: true,
                context: menuSystemParameters, debugClipName: "sfxUISelect");
        }

        // Not called in this script, as MenuSelectableFeedback uses custom sfxUIConfirm for each button,
        // but still defined for consistency
        public void PlaySfxUIConfirm()
        {
            UISfxPoolManager.Instance.PlaySfx(menuSystemParameters.sfxUIConfirm, context: menuSystemParameters, debugClipName: "sfxUIConfirm");
        }

        public void PlaySfxUICancel()
        {
            UISfxPoolManager.Instance.PlaySfx(menuSystemParameters.sfxUICancel, context: menuSystemParameters, debugClipName: "sfxUICancel");
        }

        #endregion
    }
}
