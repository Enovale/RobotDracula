using Unity.Profiling;
using UnityEngine;
using Utils;

namespace RobotDracula.General
{
    public static class GlobalGameHelper
    {
        public static GlobalGameManager GlobalGameManager => GlobalGameManager.Instance;
        
        public static bool DebugCanvasEnabled
        {
            get => GlobalGameManager.Instance._debugCanvas.enabled;
            set
            {
                GlobalGameManager._debugCanvas.enabled = value;
                GlobalGameManager._curMemory.enabled = value;
                GlobalGameManager._curFrame.enabled = value;
                GlobalGameManager._debugCanvas.gameObject.SetActiveRecursively(true);
            }
        }

        public static float TimeScale
        {
            get => GlobalGameManager._currentTimeScale;
            set
            {
                GlobalGameManager._currentTimeScale = value;
                Time.timeScale = value;
            }
        }

        public static void StartProfiler()
        {
            GlobalGameManager._memoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
        }

        public static void StopProfiler()
        {
            GlobalGameManager._memoryRecorder.Stop();
        }
    }
}