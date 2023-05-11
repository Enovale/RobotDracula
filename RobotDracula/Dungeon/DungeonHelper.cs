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

        public static DungeonUIManager DungeonUIManager
            => SingletonBehavior<DungeonUIManager>.Instance;

        public static MirrorDungeonUIManager MirrorDungeonUIManager
            => SingletonBehavior<DungeonUIManager>.Instance.Cast<MirrorDungeonUIManager>();

        public static MapManager MapManager
            => DungeonManager.MapManager;
        
        public static MirrorDungeonMapManager MirrorMapManager
            => MirrorDungeonManager.MirrorMapManager;

        public static NodeUIManager NodeUiManager
            => MapManager._nodeUIManager;

        public static NodeModel CurrentNodeModel
            => MapManager.GetCurrentNode();

        private static int _cachedNodeId = -1;

        private static NodeModel _cachedNodeModel;

        public static NodeModel CachedCurrentNodeModel
        {
            get
            {
                var nodeId = DungeonProgressHelper.NodeID;

                if (nodeId == -1)
                    return null;

                if (_cachedNodeId != nodeId || _cachedNodeModel == null)
                {
                    _cachedNodeId = nodeId;

                    _cachedNodeModel = CurrentNodeModel;
                }

                return _cachedNodeModel;
            }
        }

        public static NodeUI CurrentNodeUI
            => NodeUiManager.FindNodeUI(CurrentNodeModel.id);

        public static List<AdjacentNode> CurrentAdjacentNodes
            => CurrentNodeModel.adjacentNodes;
    }
}