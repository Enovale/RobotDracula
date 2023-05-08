using System;
using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem.Collections.Generic;
using MainUI;
using UnityEngine;
using static Dungeon.Map.ENCOUNTER;
using static Server.DUNGEON_NODERESULT;

namespace RobotDracula.Dungeon.Automation
{
    public static class DungeonAutomation
    {
        private static float _advanceCooldown;
        
        private static float _formationCooldown;
        
        private static float _levelUpCooldown;
        
        private static float _newCharacterCooldown;
        
        private static float _egoGiftCooldown;
        
        private static float _egoGiftPopupCooldown;

        private static bool _waitingForLevelUpResponse;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];
        
        public static void HandleDungeonAutomation()
        {
            var result = DungeonProgressHelper.CurrentNodeResult;
            if (_advanceCooldown <= 0f && !_waitingForLevelUpResponse && 
                //TODO: Fix currentnodemodel cache after restarting
                //DungeonHelper.CachedCurrentNodeModel.encounter is not BOSS &&
                (result is WIN or NONE || DungeonHelper.CachedCurrentNodeModel.encounter == START))
            {
                _advanceCooldown = 2f;
                ExecuteNextEncounter(GetNextNode());
            }
            // TODO: Check this better because its activating after the formation panel closes but before the battle actually starts
            else if (_advanceCooldown <= 0f && result == INBATTLE && 
                     !DungeonHelper.CachedCurrentNodeModel._isCleared &&
                     !SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
            {
                _advanceCooldown = 2f;
                
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
                _levelUpCooldown = 0.5f;
                _waitingForLevelUpResponse = false;
                DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView._confirmView.btn_close.OnClick(false);
            }

            _levelUpCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleNewCharacterAutomation(RandomDungeonAcquireCharacterPanel panel)
        {
            if (_newCharacterCooldown <= 0f)
            {
                _newCharacterCooldown = 0.5f;
                
                var afterPanel = panel._afterSelectedPanel;
                if (!afterPanel.gameObject.active)
                {
                    foreach (var kvp in Plugin.PersonalityPriority)
                    {
                        if (panel.IsSelectedFull)
                            break;

                        var item = panel._characterItemList[(Index)(kvp.Key - 1)]
                            .Cast<RandomDungeonAcquireCharacterCandidateItem>();
                        if (item._btn.interactable && !item.IsSelected)
                        {
                            item._btn.OnClick(false);
                        }
                    }

                    foreach (var item in panel._characterItemList)
                    {
                        if (panel.IsSelectedFull)
                            break;

                        if (item._btn.interactable && !item.IsSelected)
                        {
                            item._btn.OnClick(false);
                        }
                    }

                    panel._confirmStartMemberButton.OnClick(false);
                }
                else
                {
                    for (var i = 0; i < panel._selectedCount; i++)
                    {
                        var characterItem = afterPanel._selectedCharactersList[(Index)i].Cast<RandomDungeonAcquireCharacterSelectedItem>();
                        characterItem._btn.OnClick(false);

                        var switchPanel = afterPanel.SwitchPanel;
                        UIScrollViewItem<IPersonality> personalityItem = null;
                        if (Plugin.PersonalityPriority.TryGetValue((int)characterItem._characterType, out var personalityId))
                        {
                            personalityItem = switchPanel.PersonalityScrollView.GetItemByPersonalityId(personalityId);
                        }
                        else
                        {
                            Plugin.PluginLog.LogWarning($"No specified personality id for {i}");
                            foreach (var pScrollViewItem in switchPanel.PersonalityScrollView._itemList)
                            {
                                if (!pScrollViewItem.Cast<FormationSwitchablePersonalityUIScrollViewItem>()._selectedFrame.enabled)
                                {
                                    personalityItem = pScrollViewItem;
                                    break;
                                }
                            }
                        }

                        if (personalityItem == null)
                        {
                            Plugin.PluginLog.LogWarning($"Personality for {i} sinner could not be found.");
                            break;
                        }
                        
                        if (!personalityItem.Cast<FormationSwitchablePersonalityUIScrollViewItem>()._selectedFrame.enabled)
                            personalityItem.OnClick(false);
                        ProgressSwitchPanel(switchPanel);

                        while (switchPanel._currentListType == FORMATION_LIST_TYPE.EGO && switchPanel.IsOpened)
                        {
                            SelectEgoAndConfirm(switchPanel);
                        }
                    }
                    
                    afterPanel.btn_confirm.OnClick(false);
                }
            }

            _newCharacterCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleEgoGiftAutomation(SelectEgoGiftPanel panel)
        {
            if (_egoGiftCooldown <= 0f)
            {
                _egoGiftCooldown = 1f;

                var priority = Plugin.EgoGiftPriority;
                var list = new List<SelectEgoGiftData>(panel._scrollView.dataList.Cast<IEnumerable<SelectEgoGiftData>>()).ToArray();
                var sorted = list.OrderByDescending(e => priority.Contains(e.Id) ? priority.Count - priority.IndexOf(e.Id) : -1).ToList();
                
                Plugin.PluginLog.LogInfo("Ego Gifts sorted: " + string.Join(", ", sorted.Select(e =>
                    $"{e.Id} {TextDataManager.Instance.EgoGiftData.GetData(e.Id).name}")));
                panel.SetSelected(sorted.First(), false);
                panel.btn_confirm.OnClick(false);
            }

            _egoGiftCooldown -= Time.fixedDeltaTime;
        }

        public static void HandleCloseAnnoyingEgoGiftPopup()
        {
            if (_egoGiftPopupCooldown <= 0f)
            {
                _egoGiftPopupCooldown = 1f;
                
                DungeonHelper.DungeonUIManager._egoGiftPopup.btn_cancel.OnClick(false);
            }

            _egoGiftPopupCooldown -= Time.fixedDeltaTime;
        }

        public static void TryDoOneLevelUp()
        {
            var levelUpView = DungeonHelper.MirrorDungeonManager._stageRewardManager._characterLevelUpView;
            if (!levelUpView.isActiveAndEnabled || _waitingForLevelUpResponse)
                return;

            var potentialLevelUps = levelUpView._levelUpDataList;
            var priority = Plugin.PersonalityPriority.Values.ToList();
            var data = potentialLevelUps.ToArray().ToList().
                // Highest level always goes first
                OrderByDescending(a => a.NextLevel)
                // Level up the next person based on personality priority config
                .ThenByDescending(a => priority.Contains(a.PersonalityId) ? priority.Count - priority.IndexOf(a.PersonalityId) : -1);
            var idToLevelUp = data.First();
            levelUpView.OpenConfirmView(idToLevelUp);
            levelUpView._confirmView.btn_confirm.OnClick(false);
            levelUpView._confirmView.OpenSetEgoPanel();
            SelectEgoAndConfirm(levelUpView._confirmView._switchPanel);
            _waitingForLevelUpResponse = true;
        }

        private static void SelectEgoAndConfirm(FormationSwitchablePersonalityUIPanel panel)
        {
            var potentialEgos = panel.EgoScrollView._itemList;
            var calculatedLength = 0;
            foreach (var ego in potentialEgos)
            {
                if (ego._data == null) break;
                calculatedLength++;
            }
            
            if (calculatedLength > 0)
                panel.EgoScrollView.OnSelect(potentialEgos[(Index)0].Cast<FormationSwitchableEgoUIScrollViewItem>(), false);

            ProgressSwitchPanel(panel);
        }

        private static void ProgressSwitchPanel(FormationSwitchablePersonalityUIPanel panel)
        {
            panel.btn_confirm.btn_button.OnClick(false);
        }

        public static void ExecuteNextEncounter(NodeModel node = null)
        {
            if (node is null)
            {
                if (DungeonProgressHelper.CurrentNodeResult != INBATTLE)
                {
                    Plugin.PluginLog.LogError("No available nodes to progress to :(");
                    return;
                }

                Plugin.PluginLog.LogWarning("Executing Current Node Instead of Next");
                node = DungeonHelper.CachedCurrentNodeModel;
            }

            var currentNode = DungeonHelper.CachedCurrentNodeModel.Cast<INodeModel>();
            var nextNode = node.Cast<INodeModel>();
            // Seems to move the current node and ensure that NODERESULT gets set (sends move to the server)
            DungeonProgressManager.UpdateCurrentNode(currentNode, nextNode, new());

            // Opens the formation panel or enter abno event screen.
            DungeonHelper.MirrorMapManager._encounterManager.ExecuteEncounter(nextNode);
        }

        public static NodeModel GetNextNode()
        {
            // TODO: Use cache here but the cache breaks upon beating MD and restarting for some reason
            var currentNode = DungeonHelper.CurrentNodeModel;
            var nextSector = _currentFloorNodes.ToArray()
                .Where(n => n.sectorNumber == DungeonProgressManager.SectorNumber + 1).ToList();

            if (!nextSector.Any())
                return null;

            var sortedSector = nextSector.Where(i => DungeonHelper.MirrorMapManager.IsValidNode(currentNode, _nodeDict[i.nodeId].NodeModel))
                // Prioritize sinner power-up
                .OrderByDescending(i => i.GetEncounterType() == EVENT && i.encounterId is 900031)
                // Prioritize regular events, not new recruit or healing
                .ThenByDescending(i => i.GetEncounterType() == EVENT && i.encounterId is not 900021 and not (>= 900011 and <= 900013))
                .ThenByDescending(i => i.GetEncounterType() == EVENT)
                .ThenByDescending(i => i.GetEncounterType() == BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == HARD_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == AB_BATTLE)
                .ThenByDescending(i => i.GetEncounterType() == BOSS)
                .ThenByDescending(i => i.GetEncounterType() == SAVE);
            var nodeInfo = sortedSector.FirstOrDefault();

            if (nodeInfo == null && DungeonProgressHelper.CurrentNodeResult == INBATTLE)
                return currentNode;
            
            var chosenNode = _nodeDict[nodeInfo.nodeId].NodeModel;

            return chosenNode;
        }
    }
}