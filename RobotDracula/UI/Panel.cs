using System;
using Dungeon;
using Dungeon.Map;
using Il2CppSystem.Text;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
using RobotDracula.General;
using RobotDracula.Trainer;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace RobotDracula.UI
{
    public class Panel : UniverseLib.UI.Panels.PanelBase
    {
        public Panel(UIBase owner) : base(owner)
        {
            var display = Display.main;
            DefaultPosition = new Vector2((display.renderingWidth / 2) - MinWidth,
                (display.renderingHeight - MinHeight) / 2);
        }

        public override string Name { get; } = "RD Trainer";

        public override int MinWidth { get; } = 300;

        public override int MinHeight { get; } = 550;

        public override Vector2 DefaultAnchorMin { get; } = Vector2.zero;

        public override Vector2 DefaultAnchorMax { get; } = Vector2.zero;

        public override Vector2 DefaultPosition { get; }

        public override bool CanDragAndResize { get; } = true;

        private bool _watchPrediction;
        private NodeModel _predictedNode;

        protected override void ConstructPanelContent()
        {
            var trainerToggle = UiHelper.CreateToggle(TitleBar, "trainerToggle", "Trainer", true,
                () => PluginBootstrap.TrainerEnabled,
                b => PluginBootstrap.TrainerEnabled = b, out _, out _);
            trainerToggle.transform.SetSiblingIndex(1);
            UIFactory.SetLayoutElement(trainerToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
#if DEBUG
            var reactiveUiToggle = UiHelper.CreateToggle(TitleBar, "reactiveUiToggle", "ReactiveUI", true,
                () => PluginBootstrap.ReactiveUIEnabled,
                b => PluginBootstrap.ReactiveUIEnabled = b, out _, out _);
            reactiveUiToggle.transform.SetSiblingIndex(2);
            UIFactory.SetLayoutElement(reactiveUiToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
#endif

            UIFactory.CreateLabel(ContentRoot, "automationLabel", "Automate:");
            var automationRow = UIFactory.CreateUIObject("automationRow", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(automationRow, false, false, true, true, 2);
            UIFactory.SetLayoutElement(automationRow, minHeight: 25, flexibleWidth: 9999);

            var battleToggle = UiHelper.CreateToggle(automationRow, "battleToggle", "Battle", false,
                () => TrainerManager.BattleAutomationEnabled,
                b => TrainerManager.BattleAutomationEnabled = b, out _, out _);
            UIFactory.SetLayoutElement(battleToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
            var dungeonToggle = UiHelper.CreateToggle(automationRow, "dungeon", "Dungeon", false,
                () => TrainerManager.DungeonAutomationEnabled,
                b => TrainerManager.DungeonAutomationEnabled = b, out _, out _);
            UIFactory.SetLayoutElement(dungeonToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);

            var myLabel = UiHelper.CreateLabel(ContentRoot, "myLabel",
                () => $"Current Stage Phase: {BattleHelper.StagePhase}");
            UIFactory.SetLayoutElement(myLabel.gameObject);
            var dungeonLabel = UIFactory.CreateLabel(ContentRoot, "dungeonLabel", "Dungeon Info:");
            UIFactory.SetLayoutElement(dungeonLabel.gameObject);
            var dungeonGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "dungeonGroup", true, false, true, true);
            var dungeonLabel1 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel1",
                () => $"Floor:\n{DungeonProgressManager.FloorNumber}");
            var dungeonLabel2 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel2",
                () => $"Sector:\n{DungeonProgressManager.SectorNumber}");
            var dungeonLabel3 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel3",
                () => $"Node:\n{DungeonProgressManager.NodeID}");
            var dungeonLabel4 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel4",
                () => $"Type:\n{DungeonHelper.CachedCurrentNodeModel.encounter}");
            var dungeonLabel5 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel5",
                () => $"Result:\n{DungeonProgressHelper.CurrentNodeResult}");

            var predictLabelGroup = UIFactory.CreateUIObject("predictLabelGroup", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(predictLabelGroup, false, false, true, true, 2);
            var predictLabel = UIFactory.CreateLabel(predictLabelGroup, "predictLabel", "Next Node:");
            UIFactory.SetLayoutElement(predictLabel.gameObject);
            var predictButton = UiHelper.CreateButton(predictLabelGroup, "predictButton", "↻",
                () => TrainerManager.GetNextNode());
            var predictToggle = UiHelper.CreateToggle(predictLabelGroup, "predictToggle", "Watch", false,
                val => _watchPrediction = val, out _, out _);
            UIFactory.SetLayoutElement(predictToggle, minHeight: 25, flexibleWidth: 9999);
            UIFactory.SetLayoutElement(predictButton.GameObject, preferredWidth: 24, preferredHeight: 24);
            var predictGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "predictGroup", true, false, true, true);
            var predictLabel1 = UiHelper.CreateLabel(predictGroup, "predictLabel1",
                () => $"Floor:\n{_predictedNode.floorNumber}");
            var predictLabel2 = UiHelper.CreateLabel(predictGroup, "predictLabel2",
                () => $"Sector:\n{_predictedNode.sectorNumber}");
            var predictLabel3 = UiHelper.CreateLabel(predictGroup, "predictLabel3",
                () => $"NodeID:\n{_predictedNode.id}");
            var predictLabel4 = UiHelper.CreateLabel(predictGroup, "predictLabel4",
                () => $"Type:\n{_predictedNode.encounter}");

            var myBtn = UiHelper.CreateButton(ContentRoot, "myBtn", "Complete Command", OnCreateCommandClick);
            UIFactory.SetLayoutElement(myBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn.OnClick = OnCreateCommandClick;
            var myBtn3 = UiHelper.CreateButton(ContentRoot, "myBtn3", "Win Rate Toggle", OnWinRateToggleClick);
            UIFactory.SetLayoutElement(myBtn3.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var myBtn4 = UiHelper.CreateButton(ContentRoot, "myBtn4", "Damage Toggle", OnDamageToggleClick);
            UIFactory.SetLayoutElement(myBtn4.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var myBtn2 = UiHelper.CreateButton(ContentRoot, "myBtn2", "Execute Next Encounter",
                TrainerManager.ExecuteNextEncounter);
            UIFactory.SetLayoutElement(myBtn2.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var myBtn6 = UiHelper.CreateButton(ContentRoot, "myBtn6", "Print Dungeon", PrintDungeon);
            UIFactory.SetLayoutElement(myBtn6.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            // var recordBtn = UiHelper.CreateButton(ContentRoot, "recordBtn", "Start Profiler",
            //     GlobalGameHelper.StartProfiler);
            // UIFactory.SetLayoutElement(recordBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            // var recordStopBtn = UiHelper.CreateButton(ContentRoot, "recordStopBtn", "Stop Profiler",
            //     GlobalGameHelper.StopProfiler);
            // UIFactory.SetLayoutElement(recordStopBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            // var loginGuestBtn = UiHelper.CreateButton(ContentRoot, "loginGuestBtn", "Dev Guest Login",
            //     GlobalGameHelper.DevLogin);
            //UIFactory.SetLayoutElement(loginGuestBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            //var tryGetEnemyDataBtn = UiHelper.CreateButton(ContentRoot, "fetchDataBtn", "Try to fetch enemy data",
            //StaticDataHelper.TryGetEnemyData);
            //UIFactory.SetLayoutElement(tryGetEnemyDataBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var tryGetAbnoDataBtn = UiHelper.CreateButton(ContentRoot, "fetchDataBtn2i", "Try to fetch abnormality data",
                            StaticDataHelper.TryGetAbnoData);
            UIFactory.SetLayoutElement(tryGetAbnoDataBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var tryDoLevelUpBtn = UiHelper.CreateButton(ContentRoot, "tryLevelBtn", "Test level-ups pt.1",
                TrainerManager.tryDoOneLevelUp);
            UIFactory.SetLayoutElement(tryDoLevelUpBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var timeScaleSlider = UIFactory.CreateSlider(ContentRoot, "timeScaleScrollbar", out var slider);
            UIFactory.SetLayoutElement(timeScaleSlider, minHeight: 25, minWidth: 70, flexibleWidth: 999,
                flexibleHeight: 0);
            slider.value = 1f;
            slider.maxValue = 10f;
            slider.minValue = 0f;
            slider.onValueChanged.AddListener(f => GlobalGameHelper.TimeScale = f);
            var timeScaleLabel =
                UiHelper.CreateLabel(ContentRoot, "timeScaleLabel", () => $"Timescale: {slider.value}");

            var toggleRow = UIFactory.CreateUIObject("toggleRow1", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(toggleRow, false, false, true, true, 2);
            UIFactory.SetLayoutElement(toggleRow, minHeight: 25, flexibleWidth: 9999);

            var myToggle = UiHelper.CreateToggle(toggleRow, "myToggle", "Move Cheat", false,
                val => DungeonHelper.MapManager._canMoveCheat = val, out _, out _);
            UIFactory.SetLayoutElement(myToggle, minHeight: 25, flexibleWidth: 9999);
            var myToggle3 = UiHelper.CreateToggle(toggleRow, "myToggle2", "Debug View", false,
                val => GlobalGameHelper.DebugCanvasEnabled = val, out _, out _);
            UIFactory.SetLayoutElement(myToggle3, minHeight: 25, flexibleWidth: 9999);
            var fpsCapToggle = UiHelper.CreateToggle(toggleRow, "fpsCapToggle", "FPS Cap", true,
                val => TrainerManager.FPSCapEnabled = val, out _, out _);
            UIFactory.SetLayoutElement(fpsCapToggle, minHeight: 25, flexibleWidth: 9999);
        }

        protected override void OnClosePanelClicked()
        {
            Plugin.ShowTrainer = false;
        }

        public override void Update()
        {
            if (_watchPrediction)
            {
                try
                {
                    _predictedNode = TrainerManager.GetNextNode();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void OnWinRateToggleClick()
        {
            BattleHelper.SetToggleToWinRate();
        }

        private void OnDamageToggleClick()
        {
            BattleHelper.SetToggleToDamage();
        }

        private void OnCreateCommandClick()
        {
            BattleHelper.CompleteCommand();
        }

        private void PrintDungeon()
        {
            // Gets every node in the entire dungeon
            var next = DungeonHelper.MirrorMapManager._nodesByFloor;

            // Prints out all of said nodes (throws errors lol but dw about that)
            var strb = new StringBuilder();
            foreach (var f in next.Values)
            {
                foreach (var nodeInfo in f)
                {
                    strb.Append(
                        $"\n[NODE {nodeInfo.nodeId}]: Type: {nodeInfo.GetEncounterType()} Floor: {nodeInfo.floorNumber} Sector: {nodeInfo.sectorNumber} Encounter ID: {nodeInfo.encounterId}");
                }
            }

            Plugin.PluginLog.LogWarning(strb.ToString());
        }
    }
}