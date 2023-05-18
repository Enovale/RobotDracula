using Dungeon;
using Dungeon.HellsChicken;
using Dungeon.Map;
using Dungeon.Map.UI;
using Dungeon.Mirror;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using RobotDracula.General;

namespace RobotDracula.Dungeon
{
#nullable enable
    public static class DungeonHelper
    {
        public static DungeonManager DungeonManager
            => SingletonBehavior<DungeonManager>.Instance;
        
        public static MirrorDungeonManager? MirrorDungeonManager
            => DungeonManager.TryCast<MirrorDungeonManager>();
        
        public static HellsChickenDungeonManager? HellsChickenDungeonManager
            => DungeonManager.TryCast<HellsChickenDungeonManager>();

        public static DungeonUIManager DungeonUIManager
            => SingletonBehavior<DungeonUIManager>.Instance;

        public static MapManager MapManager
            => DungeonManager.MapManager;
        
        public static MirrorDungeonMapManager? MirrorMapManager
            => MirrorDungeonManager ? MirrorDungeonManager!.MirrorMapManager : null;

        public static Dictionary<int, MirrorDungeonNodeUI>? NodeUIDictionary
        {
            get
            {
                if (MirrorDungeonManager)
                    return MirrorDungeonManager!._nodeUIDictionary;
                else if (HellsChickenDungeonManager)
                    return HellsChickenDungeonManager!._nodeUIDictionary;
                else
                    return null;
            }
        }

        public static Dictionary<int, List<MirrorDungeonMapNodeInfo>>? NodesByFloor
        {
            get
            {
                if (MirrorDungeonManager)
                    return MirrorDungeonManager!._nodesDictionaryByFloor;
                else if (HellsChickenDungeonManager)
                    return HellsChickenDungeonManager!._nodesDictionaryByFloor;
                else
                    return null;
            }
        }

        public static List<MirrorDungeonMapNodeInfo>? CurrentFloorNodes
            => NodesByFloor?[DungeonProgressManager.FloorNumber];

        public static RandomDungeonStageRewardManager? StageReward
        {
            get
            {
                var reward = DungeonManager.TryCast<MirrorDungeonManager>()?.StageReward;
                if (reward is not null)
                {
                    return reward;
                }
                
                reward = DungeonManager.TryCast<HellsChickenDungeonManager>()?.StageReward;
                if (reward is not null)
                {
                    return reward;
                }

                return null;
            }
        }

        public static NodeUIManager NodeUiManager
            => MapManager._nodeUIManager;

        public static NodeModel CurrentNodeModel
            => MapManager.GetCurrentNode();

        private static int _cachedNodeId = -1;

        private static NodeModel? _cachedNodeModel;
        
        public static NodeModel? CachedCurrentNodeModel
        {
            get
            {
                var nodeId = DungeonProgressHelper.NodeID;

                if (nodeId == -1 || !DungeonProgressManager.IsOnDungeon() || !GlobalGameHelper.IsInDungeon())
                    return null;

                if (_cachedNodeId != nodeId || _cachedNodeModel?.id != nodeId)
                {
                    _cachedNodeId = nodeId;
                    _cachedNodeModel = CurrentNodeModel;
                }

                return _cachedNodeModel;
            }
        }
#nullable disable

        public static NodeUI CurrentNodeUI
            => NodeUiManager.FindNodeUI(CurrentNodeModel.id);

        public static List<AdjacentNode> CurrentAdjacentNodes
            => CurrentNodeModel.adjacentNodes;

        public static bool IsDungeonState(SCENE_STATE state)
            => state is SCENE_STATE.Dungeon 
                or SCENE_STATE.MirrorDungeon 
                or SCENE_STATE.RailwayDungeon
                or SCENE_STATE.HellsChickenDungeon;
    }
}