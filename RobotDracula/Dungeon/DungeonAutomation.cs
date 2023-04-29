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
    public static partial class DungeonAutomation
    {
        public static NodeModel NextChosenNode;

        private static float _advanceCooldown;
        
        private static float _eventChoiceCooldown;
        
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
            if (_advanceCooldown <= 0f && 
                !_waitingForLevelUpResponse && 
                (result == DUNGEON_NODERESULT.WIN || DungeonHelper.CachedCurrentNodeModel.encounter == ENCOUNTER.START))
            {
                _advanceCooldown = 1f;
                NextChosenNode = GetNextNode();
                
                if (NextChosenNode != null)
                    ExecuteNextEncounter();
            }
            else if (_advanceCooldown <= 0f && result == DUNGEON_NODERESULT.INBATTLE && !DungeonHelper.CachedCurrentNodeModel._isCleared && !SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
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

        public static void HandleChoiceEventAutomation()
        {
            if (_eventChoiceCooldown <= 0)
            {
                _eventChoiceCooldown = 1f;
                
                var choiceEventController = DungeonHelper.DungeonUIManager._choiceEventController;
                var id = choiceEventController.GetCurrentEventID();

                Plugin.PluginLog.LogInfo(id);
                Plugin.PluginLog.LogInfo(choiceEventController._eventProgressData.CurrentEventID);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._actionChoiceButtonManager
                    .isActiveAndEnabled);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._topPanelScrollBox._isSkipContent);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._behaveScrollBox._isSkipContent);
                if (choiceEventController._choiceSectionUI._actionChoiceButtonManager.isActiveAndEnabled)
                {
                    if (_choiceActionDict.TryGetValue(id, out var actions))
                    {
                        //choiceEventController._choiceSectionUI.OnClickActionChoiceButton(actions);
                    }
                }

                if (!choiceEventController._choiceSectionUI._topPanelScrollBox._isSkipContent)
                {
                    choiceEventController._choiceSectionUI._topPanelScrollBox.OnClickStorySkipButton();
                    // TODO: Need to simulate a click or find the coroutine that uses this because
                    // It won't skip and do the things until a click even if its skipping
                }
            }

            _eventChoiceCooldown -= Time.fixedDeltaTime;
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

            var sortedSector = nextSector.Where(i => DungeonHelper.MirrorMapManager.IsValidNode(currentNode, _nodeDict[i.nodeId].NodeModel))
                .OrderByDescending(i => i.GetEncounterType() == ENCOUNTER.EVENT)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.HARD_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.AB_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.BOSS)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.SAVE)
                .ThenByDescending(i => i.GetEncounterType() == ENCOUNTER.START);
            var nodeInfo = sortedSector.First();
            var chosenNode = _nodeDict[nodeInfo.nodeId].NodeModel;

            if (chosenNode == null && DungeonProgressHelper.CurrentNodeResult == DUNGEON_NODERESULT.INBATTLE)
                chosenNode = currentNode;

            return chosenNode;
        }
    }
}