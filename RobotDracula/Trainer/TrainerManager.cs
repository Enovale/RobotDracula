using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
using Server;
using UnityEngine;

namespace RobotDracula.Trainer
{
    public static class TrainerManager
    {
        public static bool BattleAutomationEnabled = false;

        public static bool DungeonAutomationEnabled = false;

        public static NodeModel NextChosenNode;

        private static float _completeCooldown;

        private static float _advanceCooldown;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];

        public static void Update()
        {
            if (BattleAutomationEnabled)
            {
                if (_completeCooldown <= 0f && Singleton<StageController>.Instance.Phase == STAGE_PHASE.WAIT_COMMAND)
                {
                    BattleHelper.SetToggleToWinRate();
                    BattleHelper.CompleteCommand();
                    _completeCooldown = 1f;
                }

                if (_completeCooldown > -1f)
                    _completeCooldown -= Time.deltaTime;
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.MirrorDungeon) && NextChosenNode == null)
                {
                    var result = DungeonProgressHelper.GetCurrentNodeResult();
                    if (_advanceCooldown <= 0f && (DungeonHelper.CurrentNodeModel.encounter == ENCOUNTER.START
                                                    || result == DUNGEON_NODERESULT.WIN))
                    {
                        NextChosenNode = GetNextNode();
                        ExecuteNextEncounter();
                        _advanceCooldown = 1f;
                    }

                    if (_advanceCooldown > -1f)
                        _advanceCooldown -= Time.deltaTime;
                }
            }
        }

        public static void ExecuteNextEncounter()
        {
            if (NextChosenNode is null)
            {
                if (DungeonProgressHelper.GetCurrentNodeResult() != DUNGEON_NODERESULT.INBATTLE)
                {
                    Plugin.PluginLog.LogError("No available nodes to progress to :(");
                    return;
                }

                NextChosenNode = DungeonHelper.CurrentNodeModel;
                Plugin.PluginLog.LogWarning("Executing current node encounter! Beware!!");
            }

            // Seems to move the current node and ensure that NODERESULT gets set
            DungeonProgressManager.UpdateCurrentNode(DungeonHelper.CurrentNodeModel, NextChosenNode, new List<int>());

            // Actually enters the encounter
            // NOTE: there needs to be a valid path or the server will not allow you to advance after encounter
            DungeonHelper.MirrorMapManager._encounterManager.ExecuteEncounter(NextChosenNode);

            // This event should only ever be just ExecuteEncounter
            // but for some reason NODERESULT doesnt get set if we do it manually
            // so... uh... this is how this works.
            //DungeonHelper.MirrorDungeonManager.MirrorUIManager._stagePanelUI._enterButtonEvent.Invoke();

            // Close the panel because it bothers me
            //DungeonHelper.MirrorDungeonManager.MirrorUIManager._stagePanelUI.Close();

            NextChosenNode = null;
        }

        public static NodeModel GetNextNode()
        {
            var currentNode = _nodeDict[DungeonProgressHelper.NodeID].NodeModel;
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
                    if (!DungeonHelper.MirrorMapManager.IsValidNode(currentNode, chosenNode))
                    {
                        chosenNode = null;
                    }
                }
            }

            if (chosenNode == null && DungeonProgressHelper.CurrentNodeResult == DUNGEON_NODERESULT.INBATTLE)
                chosenNode = DungeonHelper.CurrentNodeModel;

            return chosenNode;
        }
    }
}