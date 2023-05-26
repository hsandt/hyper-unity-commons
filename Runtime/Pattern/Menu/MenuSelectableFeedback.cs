using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

using HyperUnityCommons;

public class MenuSelectableFeedback : MonoBehaviour, IPointerMoveHandler, ISelectHandler, ISubmitHandler, IPointerClickHandler
{
    [Header("Audio assets")]

    [Tooltip("(Optional) SFX played when submitting (confirm button with any input).\n" +
        "- For Back/Exit button, you can pick a Cancel SFX" +
        "- For other buttons, you can pick a Confirm SFX\n" +
        "- For non-button selectables, you can leave this empty")]
    [FormerlySerializedAs("sfxUISubmit")]
    public AudioClip sfxUIConfirm;


    /* Injected references */

    /// Menu system this selectable belongs to
    private MenuSystem m_MenuSystem;


    public void Init(MenuSystem menuSystem)
    {
        m_MenuSystem = menuSystem;
    }

    /// Select game object. This is just a utility method to be assigned as Unity Event callback, when an Event Trigger
    /// component is aware of this component, but not of an actual Selectable on the game object (for instance because
    /// we are assigning callbacks on some base prefab which doesn't have specific Selectable components, unlike variants)
    /// Implementation is same as Selectable.Select
    public void Select()
    {
        if (EventSystem.current == null || EventSystem.current.alreadySelecting)
            return;

        EventSystem.current.SetSelectedGameObject(gameObject);
    }


    /* IPointerMoveHandler */

    public void OnPointerMove(PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            return;
        }

        // Immediately select command on hovering with move, PC-game style
        // This allows to move Selector immediately and not move here quickly on click, only to be seen at new
        // position for a very short time if the click leads to sub-menu change.

        // ! Here, we are only hovering on gameObject, so eventData.selectedObject is not gameObject, if we are not
        // hovering on the game object already selected, but it should be the currently selected object !
        DebugUtil.AssertFormat(eventData.selectedObject == EventSystem.current.currentSelectedGameObject,
            "[MenuSelectableFeedback] OnPointerMove: Event Data selectedObject {0} is not EventSystem.current.currentSelectedGameObject {1} having this script " +
            "(this should not happen, did you call this method manually?)",
            eventData.selectedObject, EventSystem.current.currentSelectedGameObject);

        // Since Move is sent frequently while moving cursor over item, we check that we are not
        // already selecting the object (note that SetSelectedGameObject has its own check anyway)
        if (eventData.selectedObject != gameObject)
        {
            // This will call OnSelect
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }


    /* ISelectHandler */

    public void OnSelect(BaseEventData eventData)
    {
        DebugUtil.AssertFormat(eventData.selectedObject == gameObject,
    "[MenuSelectableFeedback] OnSelect: Event Data selectedObject {0} is not game object {1} having this script " +
            "(this should not happen, did you call this method manually?)",
    eventData.selectedObject, gameObject);

        m_MenuSystem.OnSelect(gameObject);
    }


    // Keyboard/Gamepad submit input and pointer click input are handled separately,
    // so we need to implement both ISubmitHandler.OnSubmit and IPointerClickHandler.OnPointerClick
    // to do the same thing

    private void OnConfirm()
    {
        if (sfxUIConfirm != null)
        {
            UISfxPoolManager.Instance.PlaySfx(sfxUIConfirm, context: this, debugClipName: "sfxUISubmit");
        }
    }


    /* ISubmitHandler*/

    public void OnSubmit(BaseEventData eventData)
    {
        OnConfirm();
    }


    /* IPointerClickHandler */

    public void OnPointerClick(PointerEventData eventData)
    {
        OnConfirm();
    }
}
