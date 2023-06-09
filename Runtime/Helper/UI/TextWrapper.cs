using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HyperUnityCommons
{
    /// Put this script on any game object that may have one or multiple Text or TextMeshProUGUI children,
    /// that you want to change text and/or color of. This allows compatibility with common types of text.
    /// The extra text children are often meant as outlines (esp. for pixel art fonts), when placed before
    /// the core text in the hierarchy. However, they are optional if you only need this component to benefit from
    /// classic Text + TMP Text compatibility. If you write a framework where you want to let the user use custom
    /// outline or not, this is also useful.
    /// User code must get this component and use its API to update all the text components on children at once.
    /// This is a bit expensive if there are 4 or more outline directions, but a quick workaround to get pixel art font
    /// outlines working without a custom shader.
    public class TextWrapper : MonoBehaviour
    {
        /* Child component references */

        public Text coreLabel;
        public TextMeshProUGUI coreTMPLabel;

        public Text[] outlineLabels;
        public TextMeshProUGUI[] outlineTMPLabels;


        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Awake()
        {
            Debug.AssertFormat(coreLabel != null || coreTMPLabel != null, this, "No core label nor core TMP label defined on {0}", this);
        }
        #endif

        public void SetText(string text)
        {
            // Change text on all labels

            if (coreLabel != null)
            {
                coreLabel.text = text;
            }

            if (coreTMPLabel != null)
            {
                coreTMPLabel.text = text;
            }

            foreach (Text outlineLabel in outlineLabels)
            {
                outlineLabel.text = text;
            }

            foreach (TextMeshProUGUI outlineTMPLabel in outlineTMPLabels)
            {
                outlineTMPLabel.text = text;
            }
        }

        public void SetColor(Color color)
        {
            // Change color on core label only

            if (coreLabel != null)
            {
                coreLabel.color = color;
            }

            if (coreTMPLabel != null)
            {
                coreTMPLabel.color = color;
            }
        }
    }
}
