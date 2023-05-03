using System;
using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using MainUI;
using Server;
using UnityEngine;

namespace RobotDracula.Dungeon.Automation
{
    public static class DungeonAutomation
    {
        public static NodeModel NextChosenNode;

        private static float _advanceCooldown;
        
        private static float _formationCooldown;
        
        private static float _levelUpCooldown;

        private static bool _waitingForLevelUpResponse;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];
        
        public static void HandleDungeonAutomation()
        {
            var result = DungeonProgressHelper.CurrentNodeResult;
            if (_advanceCooldown <= 0f && !_waitingForLevelUpResponse && 
                (result == DUNGEON_NODERESULT.WIN || DungeonHelper.CachedCurrentNodeModel.encounter == ENCOUNTER.START))
            {
                _advanceCooldown = 1f;
                NextChosenNode = GetNextNode();
                
                if (NextChosenNode != null)
                    ExecuteNextEncounter();
            }
            // TODO: Check this better because its activating after the formation panel closes but before the battle actually starts
            else if (_advanceCooldown <= 0f && result == DUNGEON_NODERESULT.INBATTLE && 
                     !DungeonHelper.CachedCurrentNodeModel._isCleared && !SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
            {
                _advanceCooldown = 1f;
                NextChosenNode = DungeonHelper.CachedCurrentNodeModel;
                ExecuteNextEncounter();
            }

            _advanceCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleFormationAutomation()
        {
            if (_formationCooldown <= 0f)
            {
                Plugin.PluginLog.LogWarning("Done formation close");
                _formationCooldown = 1f;
                var formationPanel = SingletonBehavior<DungeonFormationPanel>.Instance;
                var sortedUnits = formationPanel._playerUnitFormation.PlayerUnits.ToArray()
                    .OrderByDescending(u => u.PersonalityLevel).ToArray();
                for (var i = 0; i < formationPanel._playerUnitFormation.PlayerUnits.Count; i++)
                {
                    var isParticipated = i < formationPanel._curStage.ParticipantInfo.Max;
                    sortedUnits[i].UpdateParticipation(isParticipated);
                }
                
                formationPanel.OnClickStartBattle();
            }

            _formationCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleLevelUpAutomation()
        {
            var levelUpView = DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView;
            
            if (_levelUpCooldown <= 0f && levelUpView.IsOpened && !_waitingForLevelUpResponse && !levelUpView._confirmView.isActiveAndEnabled)
            {
                _levelUpCooldown = 1f;
                TryDoOneLevelUp();
            }
            else if (_levelUpCooldown <= 0f && levelUpView.IsOpened && levelUpView._confirmView._afterConfirmView.isActiveAndEnabled)
            {
                _levelUpCooldown = 1f;
                _waitingForLevelUpResponse = false;
                DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView._confirmView.btn_close.OnClick(false);
            }

            _levelUpCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleNewCharacterAutomation()
        {
        }

        public static void HandleEgoGiftAutomation()
        {
        }

        public static void TryDoOneLevelUp()
        {
            var levelUpView = DungeonHelper.MirrorDungeonManager._stageRewardManager._characterLevelUpView;
            if (!levelUpView.isActiveAndEnabled || _waitingForLevelUpResponse)
                return;

            var numLevelUps = levelUpView._levelUpCount;
            var PotentialLevelUps = levelUpView._levelUpDataList;
            var data = PotentialLevelUps.ToArray().ToList().OrderByDescending(a => a.NextLevel).ToList();
            var test = data[0];
            levelUpView.OpenConfirmView(test);
            levelUpView._confirmView.btn_confirm.OnClick(false);
            levelUpView._confirmView.OpenSetEgoPanel();
            var potentialEgos = levelUpView._confirmView._switchPanel.EgoScrollView._itemList;
            var calculatedLength = 0;
            foreach (var ego in potentialEgos)
            {
                if (ego._data == null) break;
                calculatedLength++;
            }
            
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

            var sortedSector = nextSector.Where(i => DungeonHelper.MirrorMapManager.IsValidNode(currentNode, _nodeDict[i.nodeId].NodeModel))
                // Prioritize sinner power-up
                .OrderByDescending(i => i.GetEncounterType() == ENCOUNTER.EVENT && i.encounterId is 900031)
                // Prioritize regular events, not new recruit or healing
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.EVENT && i.encounterId is not 900021 and not (>= 900011 and <= 900013))
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.EVENT)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.HARD_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.AB_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BOSS)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.SAVE);
            var nodeInfo = sortedSector.First();
            var chosenNode = _nodeDict[nodeInfo.nodeId].NodeModel;

            if (chosenNode == null && DungeonProgressHelper.CurrentNodeResult == DUNGEON_NODERESULT.INBATTLE)
                chosenNode = currentNode;

            return chosenNode;
        }
    }
}