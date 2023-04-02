using System.Linq;
using Dungeon;
using Dungeon.Map;
using Dungeon.Mirror;
using Dungeon.Mirror.Data;
using Dungeon.Mirror.Map.UI;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using LocalSave;
using MainUI;
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

        public static bool DungeonLevelUpAutomationEnabled = false;

        public static bool FPSCapEnabled = true;

        public static NodeModel NextChosenNode;

        public delegate void BattleUpdateEventHandler();

        public static event BattleUpdateEventHandler BattleUpdate;

        public delegate void MirrorLevelUpRewardEventHandler();

        public static event MirrorLevelUpRewardEventHandler LevelUpUpdate;

        public delegate void MirrorDungeonMapEventHandler();

        public static event MirrorDungeonMapEventHandler MirrorDungeonMapUpdate;

        private static float _completeCooldown;

        private static float _advanceCooldown;

        private static bool _waitingForLevelUpResponse = false;

        private static Dictionary<int, MirrorDungeonNodeUI> _nodeDict
            => DungeonHelper.MirrorMapManager._nodeDictionary;

        private static List<MirrorDungeonMapNodeInfo> _currentFloorNodes
            => DungeonHelper.MirrorMapManager._nodesByFloor[DungeonProgressManager.FloorNumber];

        public static void Update()
        {
            if (BattleAutomationEnabled && Singleton<StageController>.Instance.Phase == STAGE_PHASE.WAIT_COMMAND)
            {
                BattleUpdate?.Invoke();
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.MirrorDungeon))
                {
                    if (DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView != null)
                    {
                        LevelUpUpdate?.Invoke();
                    }
                    else 
                    {
                       MirrorDungeonMapUpdate?.Invoke(); 
                    }
                }
            }
        }

        /*public static void Update()
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
                    if (DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView != null)
                    {
                        MirrorDungeonLevelUpPopup levelUpView =
                            DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView;
                        if (levelUpView.isActiveAndEnabled && _waitingForLevelUpResponse)
                        {
                            if (levelUpView._confirmView._switchPanel._isOpened == false)
                            {
                                levelUpView._confirmView.btn_close.OnClick(false);
                                _waitingForLevelUpResponse = false;
                            }
                        }

                        return;
                    }

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

            if (DungeonLevelUpAutomationEnabled)
            {
            }
        }*/

        public static void DoCharacterLevelUps()
        {
            MirrorDungeonLevelUpPopup levelUpView =
                DungeonHelper.MirrorDungeonManager._stageRewardManager._characterLevelUpView;
            int numLevelUps = levelUpView._levelUpCount;
            List<MirrorDungeonLevelUpData> PotentialLevelUps = levelUpView._levelUpDataList;
            System.Collections.Generic.List<MirrorDungeonLevelUpData> data =
                PotentialLevelUps.ToArray().ToList().OrderByDescending(a => a.NextLevel).ToList();
            //OpenConfirmView(LevelUpData)
            //ConfirmView.FinishLevelUP
            //smth with egos and shit in the middle.
            for (int i = 0; i < numLevelUps; i++)
            {
                MirrorDungeonLevelUpData charToLevel = data[i];
                levelUpView.OpenConfirmView(charToLevel);
                UIScrollViewItem<Ego>[] potentialEgos =
                    levelUpView._confirmView._switchPanel.EgoScrollView._itemList._items;
                switch (potentialEgos.Length)
                {
                    case 0:
                        levelUpView._confirmView.FinishLevelUp();
                        break;
                    default:
                        levelUpView._confirmView._switchPanel.EgoScrollView.OnSelect(potentialEgos[0], false);
                        levelUpView._confirmView.FinishLevelUp();
                        break;
                }
            }
        }

        public static void tryDoOneLevelUp()
        {
            MirrorDungeonLevelUpPopup levelUpView =
                DungeonHelper.MirrorDungeonManager._stageRewardManager._characterLevelUpView;
            if (!levelUpView.isActiveAndEnabled || _waitingForLevelUpResponse)
            {
                // u clicked the btn too early ya stinkin idiot
                return;
            }

            int numLevelUps = levelUpView._levelUpCount;
            Plugin.PluginLog.LogInfo(numLevelUps);
            List<MirrorDungeonLevelUpData> PotentialLevelUps = levelUpView._levelUpDataList;
            System.Collections.Generic.List<MirrorDungeonLevelUpData> data =
                PotentialLevelUps.ToArray().ToList().OrderByDescending(a => a.NextLevel).ToList();
            MirrorDungeonLevelUpData test = data[0];
            levelUpView.OpenConfirmView(test);
            levelUpView._confirmView.btn_confirm.OnClick(false);
            levelUpView._confirmView.OpenSetEgoPanel();
            UIScrollViewItem<Ego>[] potentialEgos =
                levelUpView._confirmView._switchPanel.EgoScrollView._itemList._items;
            Plugin.PluginLog.LogInfo(levelUpView._confirmView._switchPanel.EgoScrollView._itemList._size);
            Plugin.PluginLog.LogInfo(potentialEgos.Length);
            int calculatedLength = 0;
            for (int i = 0; i < potentialEgos.Length; i++)
            {
                if (potentialEgos[i]._data == null) break;
                calculatedLength++;
            }

            Plugin.PluginLog.LogInfo(calculatedLength);
            if (calculatedLength > 0)
                levelUpView._confirmView._switchPanel.EgoScrollView.OnSelect(potentialEgos[0], false);
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