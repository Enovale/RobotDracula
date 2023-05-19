using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon;
using Dungeon.Map;
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

        private static List<NodeModel> _shortestPath;

        public static void HandleDungeonAutomation()
        {
            if (!DungeonHelper.StageReward!._characterLevelUpView.IsOpened)
                _waitingForLevelUpResponse = false;
            
            var result = DungeonProgressHelper.CurrentNodeResult;
            if (_advanceCooldown <= 0f && !_waitingForLevelUpResponse && 
                DungeonHelper.CachedCurrentNodeModel!.encounter is not BOSS &&
                (result is WIN or NONE || DungeonHelper.CachedCurrentNodeModel.encounter == START))
            {
                _advanceCooldown = 2f;
                if (_shortestPath == null || 
                    _shortestPath.Count <= 0 || 
                    !DungeonHelper.CachedCurrentNodeModel.IsContainNextNode(_shortestPath.First().id))
                {
                    _shortestPath = DijkstraImpl.RunDijkstra().Select(n => (NodeModel)n).ToList();
                }

                var next = _shortestPath.First();
                _shortestPath.Remove(next);
                ExecuteNextEncounter(next);
            }
            // TODO: Check this better because its activating after the formation panel closes but before the battle actually starts
            else if (_advanceCooldown <= 0f && result is INBATTLE && 
                     !DungeonHelper.CachedCurrentNodeModel!._isCleared &&
                     !SingletonBehavior<DungeonFormationPanel>.Instance.gameObject.active)
            {
                _advanceCooldown = 2f;
                
                ExecuteNextEncounter();
            }

            _advanceCooldown -= Time.fixedDeltaTime;
        }

        public static void ResetPathfinding()
        {
            _shortestPath = null;
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
            var levelUpView = DungeonHelper.StageReward!._characterLevelUpView;
            
            if (_levelUpCooldown <= 0f && levelUpView.IsOpened && !_waitingForLevelUpResponse && !levelUpView._confirmView.isActiveAndEnabled)
            {
                _levelUpCooldown = 1f;
                TryDoOneLevelUp();
            }
            else if (_levelUpCooldown <= 0f && levelUpView.IsOpened && levelUpView._confirmView._afterConfirmView.isActiveAndEnabled)
            {
                _levelUpCooldown = 0.5f;
                _waitingForLevelUpResponse = false;
                DungeonHelper.StageReward._characterLevelUpView._confirmView.btn_close.OnClick(false);
            }

            _levelUpCooldown -= Time.fixedDeltaTime;
        }

        // TODO: Edge case where it spams the unusable confirm button without actually setting up the characters
        // However, we ballin
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
                var list = new Il2CppSystem.Collections.Generic.List<SelectEgoGiftData>(panel._scrollView.dataList.Cast<Il2CppSystem.Collections.Generic.IEnumerable<SelectEgoGiftData>>()).ToArray();
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
            var levelUpView = DungeonHelper.StageReward!._characterLevelUpView;
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
                node = DungeonHelper.CachedCurrentNodeModel!;
            }

            var currentNode = DungeonHelper.CachedCurrentNodeModel!.Cast<INodeModel>();
            var nextNode = node.Cast<INodeModel>();
            // Seems to move the current node and ensure that NODERESULT gets set (sends move to the server)
            DungeonProgressManager.UpdateCurrentNode(currentNode, nextNode, new());

            // Opens the formation panel or enter abno event screen.
            DungeonHelper.MapManager._encounterManager.ExecuteEncounter(nextNode);
        }

        public static int GetCost(NodeModel current, NodeModel destination)
        {
            if (!current.IsContainNextNode(destination.id))
                return int.MaxValue;
            else
                return GetCost(destination);
        }

        // TODO: Currently it will forego 2 healing events if it means it can get 1 power up
        // Kinda makes sense, but at the level of difficulty that the current mirror dungeon provides,
        // you really don't need the powerup and we're optimizing for time, here.
        public static int GetCost(NodeModel node)
        {
            switch (node)
            {
                // Power up gooooood
                case {encounter: EVENT, encounterID: 900031}:
                    return 4;
                // Regular events
                case {encounter: EVENT, encounterID: not 900021 and not (>= 900011 and <= 900013)}:
                    return 6;
                // Heals and Recruits.
                // This needs to be less than (Powerup Cost * 2), so that 2 heals are prioritized over 1 powerup
                case {encounter: EVENT}:
                    return 7;
                // Weigh all battles relatively the same, however still ordered.
                // If they are too costly we may end up avoiding events to prioritize avoid them; bad.
                case {encounter: BATTLE}:
                    return 9;
                case {encounter: HARD_BATTLE}:
                    return 10;
                case {encounter: AB_BATTLE}:
                    return 11;
                case {encounter: SAVE}:
                case {encounter: BOSS}:
                    return 1;
                default:
                    return int.MaxValue;
            }
        }
    }
}