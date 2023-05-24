using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HyperUnityCommons
{
    /// Put this script on any game object with multiple Text or TextMeshProUGUI children.
    /// The extra text children are often meant as outlines (esp. for pixel art fonts), when placed before
    /// the core text in the hierarchy.
    /// Then get this component and use its API to update all the text components on children at once.
    /// This is a bit expensive, but a quick workaround to get pixel art font outlines working without a custom shader.
    /// This works with both standard and TMP text.
    public class TextWithOutline : MonoBehaviour
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
