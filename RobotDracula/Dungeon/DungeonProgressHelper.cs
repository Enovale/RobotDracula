using Dungeon;
using Server;

namespace RobotDracula.Dungeon
{
    public static class DungeonProgressHelper
    {
        public static int NodeID
            => DungeonProgressManager.ProgressBridge == null ? -1 : DungeonProgressManager.NodeID;

        public static DUNGEON_NODERESULT CurrentNodeResult => GetCurrentNodeResult();

        public static DUNGEON_NODERESULT GetCurrentNodeResult() => DungeonProgressManager.ProgressBridge == null
            ? default
            : DungeonProgressManager.GetCurrentNodeResult();
    }
}