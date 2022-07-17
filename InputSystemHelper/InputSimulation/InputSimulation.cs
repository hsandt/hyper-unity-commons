using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace CommonsHelper.InputSystemHelper
{
    public class InputSimulation : MonoBehaviour
    {
        // Code inspired by https://forum.unity.com/threads/simulating-input-via-code.397499/
        // (posted my own version as huulong)

        public static void SimulateSetInputValue(InputDevice inputDevice, string inputPath, float inputValue)
        {
            InputEventPtr eventPtr;
            using (StateEvent.From(inputDevice, out eventPtr))
            {
                float currentInputValue = ((InputControl<float>) inputDevice[inputPath]).ReadValue();
                if (currentInputValue != inputValue)
                {
                    inputDevice[inputPath].WriteValueIntoEvent(inputValue, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }
                else
                {
                    Debug.LogWarningFormat("Trying to set input value of {0} to {1}, but it is already so.",
                        inputPath, inputValue);
                }
            }
        }

        public static void SimulatePressInput(InputDevice inputDevice, string inputPath)
        {
            SimulateSetInputValue(inputDevice, inputPath, 1f);
        }

        public static void SimulateReleaseInput(InputDevice inputDevice, string inputPath)
        {
            SimulateSetInputValue(inputDevice, inputPath, 0f);
        }

        public static IEnumerator SimulateShortPressInput(InputDevice inputDevice, string inputPath)
        {
            SimulatePressInput(inputDevice, inputPath);

            // WaitForFixedUpdate is more reliable than null and can be stacked, but due to some event lag
            // we prefer adding several to make sure key is correctly released
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            SimulateReleaseInput(inputDevice, inputPath);

            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
