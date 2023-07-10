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
        public static readonly int[] AVERAGE_EGO_TIER_COST = new[]
        {
            103,
            130,
            180,
            269
        };

        private static float _advanceCooldown;
        
        private static float _formationCooldown;
        
        private static float _levelUpCooldown;
        
        private static float _newCharacterCooldown;
        
        private static float _egoGiftCooldown;
        
        private static float _egoGiftPopupCooldown;

        private static List<NodeModel> _shortestPath;

        public static void HandleDungeonAutomation()
        {
            // During shops and rest stops we don't wanna keep navigating obviously
            if (DungeonHelper.CachedCurrentNodeModel!.encounter is MIRROR_SHOP or MIRROR_SELECT_EVENT)
                return;
            
            var result = DungeonProgressHelper.CurrentNodeResult;
            if (_advanceCooldown <= 0f && DungeonHelper.CachedCurrentNodeModel!.encounter is not BOSS &&
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

            if (Time.deltaTime < 1.0f)
                _advanceCooldown -= Time.deltaTime;
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
                var priority = Plugin.PersonalityPriority.Values.ToList();
                var sortedUnits = formationPanel._playerUnitFormation.PlayerUnits.ToArray()
                    .OrderByDescending(u => u.PersonalityLevel)
                    .ThenByDescending(a => priority.Contains(a.PersonalityId) ? priority.Count - priority.IndexOf(a.PersonalityId) : -1)
                    .ToArray();
                for (var i = 0; i < formationPanel._playerUnitFormation.PlayerUnits.Count; i++)
                {
                    var isParticipated = i < formationPanel._curStage.ParticipantInfo.Max;
                    sortedUnits[i].UpdateParticipation(isParticipated);
                }
                
                formationPanel.OnClickStartBattle();
            }

            if (Time.deltaTime < 1.0f)
                _formationCooldown -= Time.deltaTime;
        }

        public static void HandleLevelUpAutomation()
        {
            var levelUpView = DungeonHelper.StageReward!._characterLevelUpView;
            var inAfterConfirm = levelUpView._confirmView._afterConfirmView.cg_main.isActiveAndEnabled;
            
            if (_levelUpCooldown <= 0f && levelUpView.IsOpened && !inAfterConfirm)
            {
                _levelUpCooldown = 1f;
                TryDoOneLevelUp();
            }
            else if (_levelUpCooldown <= 0f && levelUpView.IsOpened && inAfterConfirm)
            {
                _levelUpCooldown = 0.5f;
                DungeonHelper.StageReward._characterLevelUpView._confirmView.btn_close.OnClick(false);
            }

            if (Time.deltaTime < 1.0f)
                _levelUpCooldown -= Time.deltaTime;
        }

        // TODO: Edge case where it spams the unusable confirm button without actually setting up the characters
        // However, we ballin
        public static void HandleNewCharacterAutomation(RandomDungeonAcquireCharacterPanel panel)
        {
            if (_newCharacterCooldown <= 0f)
            {
                _newCharacterCooldown = 1.0f;
                
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
                        if (Plugin.PersonalityPriority.TryGetValue((int)characterItem._data._characterType, out var personalityId))
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
                            Plugin.PluginLog.LogWarning($"Personality for sinner {i} could not be found.");
                        else if (!personalityItem.Cast<FormationSwitchablePersonalityUIScrollViewItem>()._selectedFrame.enabled)
                            personalityItem.OnClick(false);
                        
                        ProgressSwitchPanel(switchPanel);

                        if (switchPanel._currentListType == FORMATION_LIST_TYPE.EGO && switchPanel.IsOpened)
                        {
                            SelectEgosAndConfirm(switchPanel);
                        }
                    }
                    
                    afterPanel.btn_confirm.OnClick(false);
                }
            }

            // The game LAGGGS when selecting all the units at once so only count down if game is operating normally
            if (Time.deltaTime < 1.0f)
                _newCharacterCooldown -= Time.deltaTime;
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

            if (Time.deltaTime < 1.0f)
                _egoGiftCooldown -= Time.deltaTime;
        }

        public static void HandleCloseAnnoyingEgoGiftPopup()
        {
            if (_egoGiftPopupCooldown <= 0f)
            {
                _egoGiftPopupCooldown = 1f;
                
                DungeonHelper.DungeonUIManager._egoGiftPopup.btn_cancel.OnClick(false);
            }

            if (Time.deltaTime < 1.0f)
                _egoGiftPopupCooldown -= Time.deltaTime;
        }

        public static void TryDoOneLevelUp()
        {
            var levelUpView = DungeonHelper.StageReward!._characterLevelUpView;

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
            SelectEgosAndConfirm(levelUpView._confirmView._switchPanel);
        }

        private static void SelectEgosAndConfirm(FormationSwitchablePersonalityUIPanel panel)
        {
            var potentialEgos = panel.EgoScrollView._itemList.ToArray();

            // Select an ego for every available risk level
            // Maybe add a priority for this later
            var sortedEgos = potentialEgos.Where(e => (bool)e && e.Data != null)
                .GroupBy(e => e.Data.ClassInfo.EgoType)
                .Select(g => g.OrderByDescending(e => e.Data.Gacksung).First());
            
            foreach (var ego in sortedEgos)
            {
                panel.EgoScrollView.OnSelect(ego, false);
            }

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

        public static int GetNodeWeight(NodeModel current, NodeModel destination)
        {
            if (!current.IsContainNextNode(destination.id))
                return int.MaxValue;
            else
                return GetNodeWeight(destination);
        }

        // TODO: Currently it will forego 2 healing events if it means it can get 1 power up
        // Kinda makes sense, but at the level of difficulty that the current mirror dungeon provides,
        // you really don't need the powerup and we're optimizing for time, here.
        public static int GetNodeWeight(NodeModel node)
        {
            switch (node)
            {
                // Mirror shop is weighed in an approximate amount of possible ego gifts
                case {encounter: MIRROR_SHOP}:
                    var currentCost = UserDataManager.Instance.MirrorDungeonSaveData.currentInfo.Cost;
                    // TODO: Really rough estimate of how many ego gifts we can afford
                    var approximateEgoGiftCount = (int)Math.Floor(currentCost / (AVERAGE_EGO_TIER_COST[0] + Math.Abs(AVERAGE_EGO_TIER_COST[1] - AVERAGE_EGO_TIER_COST[0]) / 2m));
                    
                    // If we have enough cost to buy at least 2 ego gifts,
                    // Shops become more lucrative than an Event therefore
                    // We prioritize them like multiple events.
                    //
                    // Otherwise, shops are basically just node skips
                    // And should only be prioritized over battles.
                    return 9 - (3 * approximateEgoGiftCount) + 1;
                // Power up gooooood
                case {encounter: EVENT, encounterID: 900031}:
                    return 4;
                // Regular events
                case {encounter: EVENT, encounterID: not 900021 and not (>= 900011 and <= 900013)}:
                    return 6;
                // Heals and Recruits.
                // This needs to be less than (Powerup Cost * 2), so that 2 heals are prioritized over 1 powerup
                case {encounter: EVENT}:
                    return 8;
                case {encounter: MIRROR_SELECT_EVENT}:
                    return 8;
                // Weigh all battles relatively the same, however still ordered.
                // If they are too costly we may end up avoiding events to prioritize avoiding battle; bad.
                case {encounter: BATTLE}:
                    return 10;
                case {encounter: HARD_BATTLE}:
                    return 11;
                case {encounter: AB_BATTLE}:
                    return 12;
                case {encounter: SAVE}:
                case {encounter: BOSS}:
                    return 1;
                default:
                    return int.MaxValue;
            }
        }
    }
}