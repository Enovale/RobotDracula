using System;
using ChoiceEvent;
using Dungeon;
using Dungeon.Map;
using Il2CppSystem.Text;
using RobotDracula.Battle;
using RobotDracula.Battle.Automation;
using RobotDracula.Dungeon;
using RobotDracula.Dungeon.Automation;
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

        public override int MinWidth { get; } = 350;

        public override int MinHeight { get; } = 350;

        public override Vector2 DefaultAnchorMin { get; } = Vector2.zero;

        public override Vector2 DefaultAnchorMax { get; } = Vector2.zero;

        public override Vector2 DefaultPosition { get; }

        public override bool CanDragAndResize { get; } = true;

        private ChoiceEventProgressData _choiceEventData = null;

        private bool _watchPrediction;
        private bool _watchChoiceDebug;
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
            var eventToggle = UiHelper.CreateToggle(automationRow, "eventToggle", "Event", true,
                () => TrainerManager.ChoiceEventAutomationEnabled,
                b => TrainerManager.ChoiceEventAutomationEnabled = b, out _, out _);
            UIFactory.SetLayoutElement(eventToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
            var dungeonToggle = UiHelper.CreateToggle(automationRow, "dungeon", "Dungeon", false,
                () => TrainerManager.DungeonAutomationEnabled,
                b => TrainerManager.DungeonAutomationEnabled = b, out _, out _);
            UIFactory.SetLayoutElement(dungeonToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);

            var myLabel = UiHelper.CreateLabel(ContentRoot, "myLabel",
                () => $"Current Stage Phase: {BattleHelper.StagePhase}");
            UIFactory.SetLayoutElement(myLabel.gameObject);
            var eventLabel = UiHelper.CreateLabel(ContentRoot, "eventLabel",
                () => $"Current Event ID: {(DungeonHelper.CachedCurrentNodeModel.encounter == ENCOUNTER.EVENT ? DungeonHelper.DungeonUIManager._choiceEventController._eventProgressData.CurrentEventID : null)}");
            UIFactory.SetLayoutElement(eventLabel.gameObject);
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
                () => $"Encounter:\n{DungeonHelper.CachedCurrentNodeModel.encounterID}");
            var dungeonLabel6 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel6",
                () => $"Result:\n{DungeonProgressHelper.CurrentNodeResult}");

            var predictLabelGroup = UIFactory.CreateUIObject("predictLabelGroup", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(predictLabelGroup, false, false, true, true, 2);
            var predictLabel = UIFactory.CreateLabel(predictLabelGroup, "predictLabel", "Next Node:");
            UIFactory.SetLayoutElement(predictLabel.gameObject);
            var predictButton = UiHelper.CreateButton(predictLabelGroup, "predictButton", "↻",
                () => _predictedNode = DungeonAutomation.GetNextNode());
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
            
            var choiceLabelGroup = UIFactory.CreateUIObject("choiceLabelGroup", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(choiceLabelGroup, false, false, true, true, 2);
            var choiceLabel = UIFactory.CreateLabel(choiceLabelGroup, "choiceLabel", "Event Things:");
            UIFactory.SetLayoutElement(choiceLabel.gameObject);
            var choiceButton = UiHelper.CreateButton(choiceLabelGroup, "choiceButton", "↻",
                () => _choiceEventData = DungeonHelper.DungeonUIManager._choiceEventController._eventProgressData);
            var choiceToggle = UiHelper.CreateToggle(choiceLabelGroup, "choiceToggle", "Watch", false,
                val => _watchChoiceDebug = val, out _, out _);
            UIFactory.SetLayoutElement(choiceToggle, minHeight: 25, flexibleWidth: 9999);
            UIFactory.SetLayoutElement(choiceButton.GameObject, preferredWidth: 24, preferredHeight: 24);
            var choiceGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "choiceGroup", true, false, true, true);
            var choiceLabel1 = UiHelper.CreateLabel(choiceGroup, "choiceLabel1",
                () => $"Event ID:\n{_choiceEventData.CurrentEventID}");
            var choiceLabel2 = UiHelper.CreateLabel(choiceGroup, "choiceLabel2",
                () => $"Event Type:\n{_choiceEventData.CurrentEventType}");
            var choiceLabel3 = UiHelper.CreateLabel(choiceGroup, "choiceLabel3",
                () => $"Next Event ID:\n{_choiceEventData.ResultData.NextEventID}");
            var choiceLabel4 = UiHelper.CreateLabel(choiceGroup, "choiceLabel4",
                () => $"Result Index:\n{_choiceEventData.ResultData.RessultIndex}");

            var myBtn6 = UiHelper.CreateButton(ContentRoot, "myBtn6", "Print Dungeon", PrintDungeon);
            UIFactory.SetLayoutElement(myBtn6.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            var timeScaleSlider = UIFactory.CreateSlider(ContentRoot, "timeScaleScrollbar", out var slider);
            UIFactory.SetLayoutElement(timeScaleSlider, minHeight: 25, minWidth: 70, flexibleWidth: 999,
                flexibleHeight: 0);
            slider.value = 1f;
            slider.maxValue = 10f;
            slider.minValue = 0f;
            slider.onValueChanged.AddListener(f => GeneralAutomation.DesiredTimescale = f);
            var timeScaleLabelGroup = UIFactory.CreateUIObject("timeScaleLabelGroup", ContentRoot);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(timeScaleLabelGroup, false, false, true, true, 2);
            var timeScaleButton = UiHelper.CreateButton(timeScaleLabelGroup, "timeScaleButton", "↻",
                () =>
                {
                    GeneralAutomation.DesiredTimescale = 1f;
                    slider.value = GeneralAutomation.DesiredTimescale;
                });
            UIFactory.SetLayoutElement(timeScaleButton.GameObject, preferredWidth: 24, preferredHeight: 24);
            var timeScaleLabel = UiHelper.CreateLabel(timeScaleLabelGroup, "timeScaleLabel", () => $"Timescale: {GlobalGameHelper.TimeScale}");
            UIFactory.SetLayoutElement(timeScaleLabel.gameObject);

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
                val => GeneralAutomation.FPSCapEnabled = val, out _, out _);
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
                    _predictedNode = DungeonAutomation.GetNextNode();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (_watchChoiceDebug)
            {
                try
                {
                    _choiceEventData = DungeonHelper.DungeonUIManager._choiceEventController._eventProgressData;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
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