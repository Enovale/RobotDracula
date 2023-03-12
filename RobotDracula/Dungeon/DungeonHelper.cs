using Dungeon;
using Dungeon.Map;
using Dungeon.Map.UI;
using Il2CppSystem.Collections.Generic;

namespace RobotDracula.Dungeon
{
    public static class DungeonHelper
    {
        public static DungeonManager DungeonManager
            => SingletonBehavior<DungeonManager>.Instance;

        public static MapManager MapManager
            => DungeonManager.MapManager;

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