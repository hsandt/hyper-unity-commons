using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HyperUnityCommons
{
    /// Base class for component added to each menu
    /// Managed by a master MenuSystem
    public abstract class MenuBehaviour : MonoBehaviour
    {
        /* Injected references */

        /// Menu system this selectable belongs to
        protected MenuSystem m_MenuSystem;


        /* State */

        /// Last selection, stored when entering sub-menu, and restored when going back to this menu
        /// This is basically the button leading to the sub-menu we are exiting
        private GameObject m_LastSelection;


        /// Inject menu system reference and call OnInit
        public void Init(MenuSystem menuSystem)
        {
            m_MenuSystem = menuSystem;
            OnInit();
        }

        /// Override for custom initialization guaranteed on MenuSystem Start,
        /// even if the menu object was deactivated in the scene
        /// Start also happens after SingletonManager Instance registrations, so it is safe to use singleton managers
        protected virtual void OnInit() {}

        /// Override to customize the initial selection when this menu is shown
        /// If null, there is no initial selection, so if this is a brand new menu,
        /// player may lose kayboard/gamepad control
        public virtual Selectable GetInitialSelection() => null;

        /// Show this menu. This calls OnShow, but you need to call either TryInitialSelection (when entering sub-menu)
        /// or TryRestoreLastSelection (when going back to upper menu) after that to actually select something.
        public void Show()
        {
            OnShow();
        }

        /// Select the initial selection
        public void ApplyInitialSelection()
        {
            Selectable initialSelectable = GetInitialSelection();
            if (initialSelectable != null)
            {
                initialSelectable.Select();
            }
            else
            {
                DebugUtil.LogErrorFormat("[MenuBehaviour] ApplyInitialSelection: GetInitialSelection returned null");
            }
        }

        /// Store reference to last selection, so we can restore it later
        /// Call it when entering a sub-menu
        public void StoreLastSelection()
        {
            m_LastSelection = EventSystem.current.currentSelectedGameObject;
        }

        /// Restore last selection before we entered a sub-menu
        /// Call it when going back to this menu
        public void RestoreLastSelection()
        {
            if (m_LastSelection != null)
            {
                EventSystem.current.SetSelectedGameObject(m_LastSelection);
            }
            else
            {
                DebugUtil.LogErrorFormat("[MenuBehaviour] RestoreLastSelection: m_LastSelection is null");
            }
        }

        /// What to do when showing this menu:
        /// activate game object or play transition animation?
        protected abstract void OnShow();

        /// What to do when hiding this menu:
        /// 1. Clear selection? (most often, yes, to avoid selecting hidden object during transition if any)
        /// 2. Deactivate game object or play transition animation?
        // TODO: like Show, split in Hide and OnHide, as common behavior is very stable, disabling
        // current selection
        public abstract void Hide();

        /// Return true if player can go back to previous menu from this one
        /// by using Cancel input (if using buttons, the presence of a Back button decides it)
        public abstract bool CanGoBack();
    }
}
