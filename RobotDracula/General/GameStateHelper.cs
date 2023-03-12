namespace RobotDracula.General
{
    public static class GameStateHelper
    {
        public static SCENE_STATE GameSceneState
            => GlobalGameManager.Instance.sceneState;
    }
}