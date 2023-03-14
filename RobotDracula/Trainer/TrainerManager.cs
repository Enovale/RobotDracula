using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using LocalSave;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
using RobotDracula.General;
using Server;
using UnityEngine;

namespace RobotDracula.Trainer
{
    public static class TrainerManager
    {
        public static bool BattleAutomationEnabled = false;

        public static bool DungeonAutomationEnabled = false;

        public static bool FPSCapEnabled = true;

        public static NodeModel NextChosenNode;

        private static float _completeCooldown;

        private static float _advanceCooldown;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];

        public static void Update()
        {
            if (!FPSCapEnabled && Application.targetFrameRate != -1)
                Application.targetFrameRate = -1;
            else if (FPSCapEnabled && Application.targetFrameRate == -1 &&
                     GlobalGameHelper.SceneState != SCENE_STATE.Login)
                Singleton<LocalPlayerPrefsManager>.Instance.OptionData.ApplyFrameRate();

            if (BattleAutomationEnabled)
            {
                if (_completeCooldown <= 0f && Singleton<StageController>.Instance.Phase == STAGE_PHASE.WAIT_COMMAND)
                {
                    BattleHelper.SetToggleToWinRate();
                    BattleHelper.CompleteCommand();
                    _completeCooldown = 1f;
                }

                if (_completeCooldown > -1f)
                    _completeCooldown -= Time.fixedDeltaTime;
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.MirrorDungeon) && NextChosenNode == null)
                {
                    var result = DungeonProgressHelper.CurrentNodeResult;
                    if (_advanceCooldown <= 0f && (
                            (!SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active &&
                             result == DUNGEON_NODERESULT.INBATTLE)
                            || DungeonHelper.CachedCurrentNodeModel.encounter == ENCOUNTER.START
                            || result == DUNGEON_NODERESULT.WIN))
                    {
                        NextChosenNode = GetNextNode();
                        ExecuteNextEncounter();
                        _advanceCooldown = 1f;
                    }

                    if (_advanceCooldown > -1f)
                        _advanceCooldown -= Time.fixedDeltaTime;
                }
            }
        }

        public static void ExecuteNextEncounter()
        {
            if (NextChosenNode is null)
            {
                if (DungeonProgressHelper.CurrentNodeResult != DUNGEON_NODERESULT.INBATTLE)
                {
                    Plugin.PluginLog.LogError("No available nodes to progress to :(");
                    return;
                }

                NextChosenNode = DungeonHelper.CachedCurrentNodeModel;
                Plugin.PluginLog.LogWarning("Executing current node encounter! Beware!!");
            }

            // Seems to move the current node and ensure that NODERESULT gets set (sends move to the server)
            DungeonProgressManager.UpdateCurrentNode(DungeonHelper.CachedCurrentNodeModel, NextChosenNode, new());

            // Opens the formation panel or enter abno event screen.
            DungeonHelper.MirrorMapManager._encounterManager.ExecuteEncounter(NextChosenNode);

            NextChosenNode = null;
        }

        public static NodeModel GetNextNode()
        {
            var currentNode = DungeonHelper.CachedCurrentNodeModel;
            var nextSector = _currentFloorNodes.ToArray()
                .Where(n => n.sectorNumber == DungeonProgressManager.SectorNumber + 1);

            NodeModel chosenNode = null;
            foreach (var nodeInfo in nextSector)
            {
                if (chosenNode != null)
                    break;

                // Get the first node that isn't an event
                if (nodeInfo.IsBattleNode())
                {
                    // Gets the nodemodel in a faster way than asking the NodeUI
                    chosenNode = _nodeDict[nodeInfo.nodeId].NodeModel;

                    // Makes sure we can travel to that node validly
                    // NOTE: there needs to be a valid path or the server will not allow you to advance after encounter
                    if (!DungeonHelper.MirrorMapManager.IsValidNode(currentNode, chosenNode))
                    {
                        chosenNode = null;
                    }
                }
            }

            if (chosenNode == null && DungeonProgressHelper.CurrentNodeResult == DUNGEON_NODERESULT.INBATTLE)
                chosenNode = currentNode;

            return chosenNode;
        }
    }
}