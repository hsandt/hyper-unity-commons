using System.Collections.Generic;
using UnityEngine;

namespace HyperUnityCommons
{
    public static class AwaitableUtil
    {
        // SisusCo's simplified WhenAll extension (doesn't catch exceptions)
        // from https://discussions.unity.com/t/awaitables-whenall/1551331/20
        public static async Awaitable WhenAll(this IEnumerable<Awaitable> awaitables)
        {
            foreach (var awaitable in awaitables)
            {
                await awaitable;
            }
        }
    }
}
