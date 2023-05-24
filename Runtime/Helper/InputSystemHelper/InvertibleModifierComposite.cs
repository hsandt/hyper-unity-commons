// Script based on
// https://github.com/Unity-Technologies/InputSystem/blob/develop/Assets/Samples/CustomComposite/CustomComposite.cs
// and the discussion at
// https://forum.unity.com/threads/input-system-1-1-0-preview-2-axis-with-modifier-key-composite.1035397/
// but adding invertModifier to a child class of OneModifierComposite instead of AxisComposite
// It allows the developer to setup an input binding with a modifier that needs to be *released* to confirm input.
// This way, you can bind an action to an input with modifier while making sure that the input *without* modifier
// never triggers that action.

// Example
// Say you want Alt+Enter to toggle fullscreen, but only Enter (with Alt released) to trigger UI Submit,
// to avoid submitting and toggling fullscreen at the same time.
// First, bind a OneModifierComposite input Alt+Enter to your action ToggleFullscreen. You can also use an
// InvertibleModifierComposite with invertModifier = false.
// Then, bind an InvertibleModifierComposite with invertModifier = true to the UI Submit action (in this particular
// case, you'll also want to remove the default entry based on Usages "Submit [Any]" which overlaps Enter).

// Project requirements
// To use it, you need to check Project Settings > Player > Allow 'unsafe' Code
// This is because ReadValue override has been adapted from OneModifierComposite.ReadValue which is itself unsafe.

using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HyperUnityCommons.InputSystemHelper
{
    // We need to register our composite with the input system. And we
    // want to do it in a way that makes the composite visible in the action
    // editor of the input system.
    //
    // For that to happen, we need to call InputSystem.RegisterBindingComposite
    // sometime during startup. We make that happen by using [InitializeOnLoad]
    // in the editor and [RuntimeInitializeOnLoadMethod] in the player.
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    [DisplayStringFormat("{modifier}+{binding}")]
    [DisplayName("Binding With Invertible Modifier")]
    public class InvertibleModifierComposite : OneModifierComposite
    {
        // In the editor, the static class constructor will be called on startup
        // because of [InitializeOnLoad].
        #if UNITY_EDITOR
        static InvertibleModifierComposite()
        {
            // Trigger our RegisterBindingComposite code in the editor.
            Initialize();
        }
        #endif

        // In the player, [RuntimeInitializeOnLoadMethod] will make sure our
        // initialization code gets called during startup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // This registers the composite with the input system. After calling this
            // method, we can have bindings reference the composite. Also, the
            // composite will show up in the action editor.
            //
            // NOTE: We don't supply a name for the composite here. The default logic
            //       will take the name of the type ("InvertibleModifierComposite" in our case)
            //       and snip off "Composite" if used as a suffix (which is the case
            //       for us) and then use that as the name. So in our case, we are
            //       registering a composite called "Custom" here.
            //
            //       If we were to use our composite with the AddCompositeBinding API,
            //       for example, it would look like this:
            //
            //       myAction.AddCompositeBinding("InvertibleModifier")
            //           .With("Binding", "<Gamepad>/rightTrigger");
            InputSystem.RegisterBindingComposite<InvertibleModifierComposite>();
        }

        /// <summary>
        /// When set to true, the modifier must be released for the binding to be considered.
        /// Else, the modifier must be pressed, and this behaves like OneModifierComposite.
        /// </summary>
        public bool invertModifier = false;

        // Below, we override all methods of OneModifierComposite that check modifier, and prefix the condition
        // with `invertModifier ^`

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            if (invertModifier ^ context.ReadValueAsButton(modifier))
                return context.EvaluateMagnitude(binding);
            return default;
        }

        public override unsafe void ReadValue(ref InputBindingCompositeContext context, void* buffer, int bufferSize)
        {
            if (invertModifier ^ context.ReadValueAsButton(modifier))
                context.ReadValue(binding, buffer, bufferSize);
            else
                UnsafeUtility.MemClear(buffer, valueSizeInBytes);
        }

        public override object ReadValueAsObject(ref InputBindingCompositeContext context)
        {
            if (invertModifier ^ context.ReadValueAsButton(modifier))
                return context.ReadValueAsObject(binding);
            return null;
        }
    }
}