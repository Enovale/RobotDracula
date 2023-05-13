using RobotDracula.Dungeon;
using UnityEngine;

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
            get => Time.timeScale;
            set
            {
                GlobalGameManager._currentTimeScale = value;
                Time.timeScale = value;
            }
        }

        public static SCENE_STATE SceneState
            => GlobalGameManager.sceneState;

        public static bool IsInDungeon() => DungeonHelper.IsDungeonState(SceneState);
    }
}