using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CommonsVisualScripting
{
    /// Start coroutine and wait until it is over.
    [UnitCategory("Coroutine")]
    [UnitTitle("Start and Wait For Coroutine")]
    [UnitShortTitle("Coroutine")]
    public class StartAndWaitForCoroutineUnit : WaitUnit
    {
        /// The coroutine to start and wait for.
        [DoNotSerialize]
        public ValueInput coroutineEnumerator { get; private set; }

        /// The script instance to run the coroutine on
        /// (not necessarily the one where the coroutine method was called)
        [DoNotSerialize]
        public ValueInput scriptInstance { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            coroutineEnumerator = ValueInput<IEnumerator>(nameof(coroutineEnumerator));
            scriptInstance = ValueInput<MonoBehaviour>(nameof(scriptInstance));
            Requirement(coroutineEnumerator, enter);
            Requirement(scriptInstance, enter);
        }

        protected override IEnumerator Await(Flow flow)
        {
            MonoBehaviour scriptInstanceValue = flow.GetValue<MonoBehaviour>(scriptInstance);
            IEnumerator coroutineEnumeratorValue = flow.GetValue<IEnumerator>(coroutineEnumerator);
            yield return scriptInstanceValue.StartCoroutine(coroutineEnumeratorValue);
            yield return exit;
        }
    }
}
