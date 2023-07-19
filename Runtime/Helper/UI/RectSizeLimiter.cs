// Source code based on https://forum.unity.com/threads/rect-transform-size-limiter.620860/
// (Ultroman's version)
//
// Authors:
// - Democide: original code
// - giggioz: support minSize
// - Ultroman: support parentRectTransform
//
// Adapted for Hyper Unity Commons:
// - surrounded with namespace, some code formatting
//
// Usage notes by giggioz:
//
//     To make a textbox, this script is supposed to sit on the GameObject with the TextMeshPRO component on it, after you
// have added a Content Size Fitter to it (set both its settings to Preferred Size). Do not try to use Unity's normal
// Text components; they do not work properly, especially when it comes to tight fits, which make them stop showing due
// to floating point errors or something.
//
//     You also have to drag a reference to the child that the script is sitting on into the "Rect Transform" inspector
// field. It doesn't find it automatically, to make it so the script doesn't necessarily have to sit on the same
// GameObject that it controls RectTransforms for.
//
//     If you wish to have another RectTransform (usually the parent) also be sized along with the main
// "Rect Transform", you can also add a reference to the "Parent Rect Transform" inspector field. Set the parent to
// stretch both horizontally and vertically (click the Anchor Presets button, hold Alt and press the button in the
// bottom-right with the two blue arrows). The parent does not need any other layout components for this to work.
//
//     In order to get margins, don't use negative values in top/bottom/left/right on the child (text) RectTransform,
// but instead set the margins on the TextMeshPRO component itself, under Extra Settings. That way the parent can still
// be positioned properly using anchors.
//
// Additional note by hsandt:
//    To align content, move pivot in alignment direction you want.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HyperUnityCommons
{
    [ExecuteInEditMode]
    public class RectSizeLimiter : UIBehaviour, ILayoutSelfController
    {
        public RectTransform rectTransform;
        public RectTransform parentRectTransform;

        [SerializeField]
        protected Vector2 m_maxSize = Vector2.zero;

        [SerializeField]
        protected Vector2 m_minSize = Vector2.zero;

        public Vector2 maxSize
        {
            get { return m_maxSize; }
            set
            {
                if(m_maxSize != value)
                {
                    m_maxSize = value;
                    SetDirty();
                }
            }
        }

        public Vector2 minSize
        {
            get { return m_minSize; }
            set
            {
                if(m_minSize != value)
                {
                    m_minSize = value;
                    SetDirty();
                }
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected void SetDirty()
        {
            if(!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        public void SetLayoutHorizontal()
        {
            if(m_maxSize.x > 0f && rectTransform.rect.width > m_maxSize.x)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize.x);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            }

            if(m_minSize.x > 0f && rectTransform.rect.width < m_minSize.x)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minSize.x);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            }

            if (parentRectTransform != null)
            {
                parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.sizeDelta.x);
                m_Tracker.Add(this, parentRectTransform, DrivenTransformProperties.SizeDeltaX);
            }
        }

        public void SetLayoutVertical()
        {
            if(m_maxSize.y > 0f && rectTransform.rect.height > m_maxSize.y)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize.y);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }

            if(m_minSize.y > 0f && rectTransform.rect.height < m_minSize.y)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minSize.y);
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            }

            if(parentRectTransform != null)
            {
                parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.sizeDelta.y);
                m_Tracker.Add(this, parentRectTransform, DrivenTransformProperties.SizeDeltaY);
            }
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
        #endif
    }
}
