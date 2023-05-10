using ChoiceEvent;
using Il2CppSystem.Text;
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
        }

        public override string Name { get; } = "RD Trainer";

        public override int MinWidth { get; } = 230;

        public override int MinHeight { get; } = 250;

        public override Vector2 DefaultAnchorMin { get; } = Vector2.zero;

        public override Vector2 DefaultAnchorMax { get; } = Vector2.zero;

        public override Vector2 DefaultPosition
            => new(Display.main.renderingWidth / 2f - MinWidth, (Display.main.renderingHeight - MinHeight) / 2f);

        public override bool CanDragAndResize { get; } = true;

        protected override void ConstructPanelContent()
        {
            var trainerToggle = UiHelper.CreateToggle(TitleBar, "trainerToggle", "", true,
                () => PluginBootstrap.TrainerEnabled,
                b => PluginBootstrap.TrainerEnabled = b, out _, out _);
            trainerToggle.transform.SetSiblingIndex(0);
            UIFactory.SetLayoutElement(trainerToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
#if DEBUG
            var reactiveUiToggle = UiHelper.CreateToggle(TitleBar, "reactiveUiToggle", "ReactUI", true,
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
            
            var dungeonLabel = UIFactory.CreateLabel(ContentRoot, "dungeonLabel", "Dungeon Info:");
            UIFactory.SetLayoutElement(dungeonLabel.gameObject);
            var dungeonGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "dungeonGroup", true, false, true, true);
            var dungeonLabel4 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel4",
                () => $"Type:\n{DungeonHelper.CachedCurrentNodeModel.encounter}");
            var dungeonLabel5 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel5",
                () => $"Encounter:\n{DungeonHelper.CachedCurrentNodeModel.encounterID}");
            var dungeonLabel6 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel6",
                () => $"Result:\n{DungeonProgressHelper.CurrentNodeResult}");

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
        }

        protected override void OnClosePanelClicked()
        {
            Plugin.ShowTrainer = false;
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