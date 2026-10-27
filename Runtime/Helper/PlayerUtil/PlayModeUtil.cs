using UnityEngine;

namespace HyperUnityCommons
{
    public static class PlayModeUtil
    {
        public static PlayMode GetPlayMode()
        {
            PlayMode playMode;

            if (Application.isEditor)
            {
                if (HyperPrefs.GetSimulateReleaseBuildPref())
                {
                    playMode = PlayMode.ReleaseBuild;
                }
                else
                {
                    playMode = PlayMode.Editor;
                }
            }
            else if (Debug.isDebugBuild)
            {
                playMode = PlayMode.DevelopmentBuild;
            }
            else
            {
                playMode = PlayMode.ReleaseBuild;
            }

            return playMode;
        }
    }
}
