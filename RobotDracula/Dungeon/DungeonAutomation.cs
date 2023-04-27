using System;
using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using MainUI;
using Server;
using UnityEngine;

namespace RobotDracula.Dungeon
{
    public static class DungeonAutomation
    {
        public static NodeModel NextChosenNode;

        private static float _advanceCooldown;

        private static bool _waitingForLevelUpResponse = false;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];
        
        public static void HandleDungeonAutomation()
        {
            var result = DungeonProgressHelper.CurrentNodeResult;
            if (_advanceCooldown <= 0f && 
                !_waitingForLevelUpResponse && 
                result != DUNGEON_NODERESULT.INBATTLE && 
                result != DUNGEON_NODERESULT.RUNNING && 
                result != DUNGEON_NODERESULT.DEFEAT)
            {
                NextChosenNode = GetNextNode();
                
                if (NextChosenNode != null)
                    ExecuteNextEncounter();
                
                _advanceCooldown = 1f;
            }
            else if (_advanceCooldown <= 0f && result == DUNGEON_NODERESULT.INBATTLE && !SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
            {
                NextChosenNode = DungeonHelper.CachedCurrentNodeModel;
                ExecuteNextEncounter();
                
                _advanceCooldown = 1f;
            }
            else if (_advanceCooldown <= 0f && SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
            {
                var formationPanel = SingletonBehavior<DungeonFormationPanel>.Instance;
                Plugin.PluginLog.LogInfo(formationPanel._curStage.ParticipantInfo.Max);
                var sortedUnits = formationPanel._playerUnitFormation.PlayerUnits.ToArray()
                    .OrderByDescending(u => u.PersonalityLevel).ToArray();
                for (var i = 0; i < formationPanel._playerUnitFormation.PlayerUnits.Count; i++)
                {
                    var isParticipated = i < formationPanel._curStage.ParticipantInfo.Max;
                    sortedUnits[i].UpdateParticipation(isParticipated);
                }
                Plugin.PluginLog.LogInfo(formationPanel.GetParticipantsCount());
                Plugin.PluginLog.LogInfo(string.Join(", ", formationPanel.ParticipantsIdList.ToArray().ToList()));
                formationPanel.OnClickStartBattle();
                
                // Important because we have no way of knowing the difference between this branch and the one above
                _advanceCooldown = 1f;
            }

            _advanceCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleLevelUpAutomation()
        {
            var levelUpView = DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView;
            
            if (levelUpView.IsOpened && !_waitingForLevelUpResponse && !levelUpView._confirmView.isActiveAndEnabled)
            {
                TryDoOneLevelUp();
            }
            else if (levelUpView.IsOpened && levelUpView._confirmView._afterConfirmView.isActiveAndEnabled)
            {
                Plugin.PluginLog.LogWarning("Closing afterConfirmView");
                _waitingForLevelUpResponse = false;
                DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView._confirmView.btn_close.OnClick(false);
            }
        }

        public static void TryDoOneLevelUp()
        {
            var levelUpView = DungeonHelper.MirrorDungeonManager._stageRewardManager._characterLevelUpView;
            if (!levelUpView.isActiveAndEnabled || _waitingForLevelUpResponse)
                return;

            var numLevelUps = levelUpView._levelUpCount;
            Plugin.PluginLog.LogInfo(numLevelUps);
            var PotentialLevelUps = levelUpView._levelUpDataList;
            var data = PotentialLevelUps.ToArray().ToList().OrderByDescending(a => a.NextLevel).ToList();
            var test = data[0];
            levelUpView.OpenConfirmView(test);
            levelUpView._confirmView.btn_confirm.OnClick(false);
            levelUpView._confirmView.OpenSetEgoPanel();
            var potentialEgos = levelUpView._confirmView._switchPanel.EgoScrollView._itemList;
            Plugin.PluginLog.LogInfo(levelUpView._confirmView._switchPanel.EgoScrollView._itemList._size);
            Plugin.PluginLog.LogInfo(potentialEgos.Count);
            var calculatedLength = 0;
            foreach (var ego in potentialEgos)
            {
                if (ego._data == null) break;
                calculatedLength++;
            }
            
            Plugin.PluginLog.LogInfo(calculatedLength);
            if (calculatedLength > 0)
                levelUpView._confirmView._switchPanel.EgoScrollView.OnSelect(potentialEgos[(Index)0].Cast<FormationSwitchableEgoUIScrollViewItem>(), false);
            levelUpView._confirmView.FinishLevelUp();
            _waitingForLevelUpResponse = true;
            //levelUpView._confirmView.UpdateConfirmedData();
            //levelUpView._confirmView.btn_close.OnClick(false);
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

            var currentNode = DungeonHelper.CachedCurrentNodeModel.Cast<INodeModel>();
            var nextNode = NextChosenNode.Cast<INodeModel>();
            // Seems to move the current node and ensure that NODERESULT gets set (sends move to the server)
            DungeonProgressManager.UpdateCurrentNode(currentNode, nextNode, new());

            // Opens the formation panel or enter abno event screen.
            DungeonHelper.MirrorMapManager._encounterManager.ExecuteEncounter(nextNode);

            NextChosenNode = null;
        }

        public static NodeModel GetNextNode()
        {
            var currentNode = DungeonHelper.CachedCurrentNodeModel;
            var nextSector = _currentFloorNodes.ToArray()
                .Where(n => n.sectorNumber == DungeonProgressManager.SectorNumber + 1).ToList();

            if (!nextSector.Any())
                return null;

            var sortedSector = nextSector.OrderByDescending(i => i.GetEncounterType() == ENCOUNTER.EVENT)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.HARD_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.AB_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BOSS)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.SAVE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.START);
            var nodeInfo = sortedSector.First();
            var chosenNode = _nodeDict[nodeInfo.nodeId].NodeModel;
            Plugin.PluginLog.LogError(chosenNode);

            // Makes sure we can travel to that node validly
            // NOTE: there needs to be a valid path or the server will not allow you to advance after encounter
            if (!DungeonHelper.MirrorMapManager.IsValidNode(currentNode, chosenNode))
            {
                chosenNode = null;
            }

            if (chosenNode == null && DungeonProgressHelper.CurrentNodeResult == DUNGEON_NODERESULT.INBATTLE)
                chosenNode = currentNode;

            return chosenNode;
        }
    }
}