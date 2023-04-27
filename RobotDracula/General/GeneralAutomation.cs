using LocalSave;
using UnityEngine;

namespace RobotDracula.General
{
    public static class GeneralAutomation
    {
        public static bool FPSCapEnabled = true;
        
        public static float DesiredTimescale = 1f;
        
        public static void HandleFPSUncap()
        {
            if (!FPSCapEnabled && Application.targetFrameRate != -1)
                Application.targetFrameRate = -1;
            else if (FPSCapEnabled && Application.targetFrameRate == -1 &&
                     GlobalGameHelper.SceneState != SCENE_STATE.Login)
                Singleton<LocalPlayerPrefsManager>.Instance.OptionData.ApplyFrameRate();
        }

        public static void HandleTimescaleUpdate()
        {
            if (GlobalGameHelper.TimeScale != DesiredTimescale)
            {
                GlobalGameHelper.TimeScale = DesiredTimescale;
            }
        }
    }
}