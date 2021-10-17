using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CommonsHelper
{
    /// Put this script on any game object with multiple Text or TextMeshProUGUI children.
    /// The extra text children are often meant as outlines (esp. for pixel art fonts), when placed before
    /// the core text in the hierarchy.
    /// Then get this component and use its API to update all the text components on children at once.
    /// This is a bit expensive, but a quick workaround to get pixel art font outlines working without a custom shader.
    /// This works with both standard and TMP text.
    public class TextWithOutline : MonoBehaviour
    {
        /* Cached child component references */
        
        private Text[] m_TextWidgets;
        private TextMeshProUGUI[] m_TMPWidgets;

        
        void Awake ()
        {
            m_TextWidgets = GetComponentsInChildren<Text>();
            m_TMPWidgets = GetComponentsInChildren<TextMeshProUGUI>();
        }

        public void SetText (string text)
        {
            // This method is agnostic to how the core text and outlines are set:
            // it just changes all the text contents
            
            foreach (Text textWidget in m_TextWidgets)
            {
                textWidget.text = text;
            }

            foreach (TextMeshProUGUI tmpWidget in m_TMPWidgets)
            {
                tmpWidget.text = text;
            }
        }
    }
}

