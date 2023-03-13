using Dungeon;
using Dungeon.Map;
using Dungeon.Map.UI;
using Dungeon.Mirror;
using Il2CppSystem.Collections.Generic;

namespace RobotDracula.Dungeon
{
    public static class DungeonHelper
    {
        public static DungeonManager DungeonManager
            => SingletonBehavior<DungeonManager>.Instance;
        
        public static MirrorDungeonManager MirrorDungeonManager
            => MirrorDungeonManager.MirrorInstance;

        public static MapManager MapManager
            => DungeonManager.MapManager;

        public static MirrorDungeonMapManager MirrorMapManager
            => MirrorDungeonManager.MirrorMapManager;

        public static NodeUIManager NodeUiManager
            => MapManager._nodeUIManager;

        public static NodeModel CurrentNodeModel
            => MapManager.GetCurrentNode();

        public static NodeUI CurrentNodeUI
            => NodeUiManager.FindNodeUI(CurrentNodeModel.id);

        public static List<AdjacentNode> CurrentAdjacentNodes
            => CurrentNodeModel.adjacentNodes;
    }
}