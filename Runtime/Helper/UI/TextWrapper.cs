using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if NL_ELRACCOONE_TWEENS
using ElRaccoone.Tweens;
#endif

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
            Debug.AssertFormat(coreLabel == null || coreTMPLabel == null, this, "Both core label and core TMP label are defined on {0}, " +
                "we will only consider core label when getting information", this);
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

        public Color GetColor()
        {
            // As mentioned in second Awake assert, we give priority to core label if both core labels are defined

            if (coreLabel != null)
            {
                return coreLabel.color;
            }

            if (coreTMPLabel != null)
            {
                return coreTMPLabel.color;
            }

            // No need to assert, as we've already checked that at least one label was defined in Awake,
            // so just fallback to white
            return Color.white;
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

        #if NL_ELRACCOONE_TWEENS
        public async Task FadeOutAsync(float duration)
        {
            List<Task> tasks = new();

            if (coreLabel != null)
            {
                tasks.Add(coreLabel.TweenGraphicAlpha(0f, duration).Await());
            }

            if (coreTMPLabel != null)
            {
                tasks.Add(coreTMPLabel.TweenGraphicAlpha(0f, duration).Await());
            }

            tasks.AddRange(outlineLabels.Select(outlineLabel => outlineLabel.TweenGraphicAlpha(0f, duration).Await()));
            tasks.AddRange(outlineTMPLabels.Select(outlineTMPLabel => outlineTMPLabel.TweenGraphicAlpha(0f, duration).Await()));

            await Task.WhenAll(tasks);
        }
        #endif
    }
}
