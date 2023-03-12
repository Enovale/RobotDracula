using System;
using Dungeon;
using Il2CppSystem.Collections.Generic;
using Dungeon.Map;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
using UnityEngine;
using UnityEngine.Events;
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

        public override string Name => "My Panel";
        public override int MinWidth => 280;
        public override int MinHeight => 400;
        public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
        public override Vector2 DefaultAnchorMax => new(0.25f, 0.25f);
        public override bool CanDragAndResize => true;

        protected override void ConstructPanelContent()
        {
            var myLabel = UiHelper.CreateLabel(ContentRoot, "myLabel",
                () => $"Current Stage Phase: {BattleHelper.StagePhase}");
            UIFactory.SetLayoutElement(myLabel.gameObject);
            var dungeonLabel = UIFactory.CreateLabel(ContentRoot, "dungeonLabel", "Dungeon Info:");
            UIFactory.SetLayoutElement(dungeonLabel.gameObject);
            var dungeonGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "dungeonGroup", false, false, true, true);
            var dungeonLabel1 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel1",
                () => $"Floor:\n{DungeonProgressManager.FloorNumber}");
            var dungeonLabel2 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel2",
                () => $"Sector:\n{DungeonProgressManager.SectorNumber}");
            var dungeonLabel3 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel3",
                () => $"Node:\n{DungeonProgressManager.NodeID}");
            var dungeonLabel4 = UiHelper.CreateLabel(dungeonGroup, "dungeonLabel4",
                () => $"Previous Node:\n{DungeonProgressManager.PreviousNodeID}");
            var myBtn = UIFactory.CreateButton(ContentRoot, "myBtn", "Complete Command");
            UIFactory.SetLayoutElement(myBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn.OnClick = OnCreateCommandClick;
            var myBtn3 = UIFactory.CreateButton(ContentRoot, "myBtn3", "Win Rate Toggle");
            UIFactory.SetLayoutElement(myBtn3.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn3.OnClick = OnWinRateToggleClick;
            var myBtn4 = UIFactory.CreateButton(ContentRoot, "myBtn4", "Damage Toggle");
            UIFactory.SetLayoutElement(myBtn4.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn4.OnClick = OnWinRateToggleClick;
            var myBtn2 = UIFactory.CreateButton(ContentRoot, "myBtn2", "Click First Adjacent Node");
            UIFactory.SetLayoutElement(myBtn2.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn2.OnClick = AdjacentNodeClick;
            var myBtn5 = UIFactory.CreateButton(ContentRoot, "myBtn5", "Click Current Node");
            UIFactory.SetLayoutElement(myBtn5.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn5.OnClick = CurrentNodeClick;
            var myBtn6 = UIFactory.CreateButton(ContentRoot, "myBtn6", "Click Random Node In Next Sector");
            UIFactory.SetLayoutElement(myBtn6.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn6.OnClick = RandomNodeClick;
            var myToggle = UIFactory.CreateToggle(ContentRoot, "myToggle", out Toggle toggle, out Text text);
            UIFactory.SetLayoutElement(myToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
            text.text = "Move Cheat";
            //toggle.isOn = DungeonHelper.MapManager._canMoveCheat;
            toggle.onValueChanged.AddListener(val => DungeonHelper.MapManager._canMoveCheat = val);
        }

        private void RandomNodeClick()
        {
            var dungeon = DungeonHelper.MapManager._dungeonModel;
            if (dungeon is null)
            {
                Plugin.PluginLog.LogError("Dungeon was null");
                return;
            }
            var sectorId = DungeonProgressManager.SectorNumber + 1;
            var sector = dungeon.sectors[DungeonProgressManager.FloorNumber][sectorId];
            if (sector == null)
            {
                Plugin.PluginLog.LogError("Sector was null");
                return;
            }
            var node = sector.GetNodeByIndex(0);
            if (node == null)
            {
                Plugin.PluginLog.LogError("Node was null");
                return;
            }
            DungeonHelper.MapManager.TryUpdatePlayerPosition(node);
            //DungeonHelper.MapManager._encounterManager.ExecuteEncounter(node);
        }

        private void CurrentNodeClick()
        {
            DungeonHelper.MapManager.TryUpdatePlayerPosition(DungeonHelper.MapManager.GetCurrentNode());
            DungeonHelper.DungeonManager.OpenCurrentEventNode();
            //DungeonHelper.MapManager._encounterManager.ExecuteEncounter(DungeonHelper.MapManager.GetCurrentNode());
        }

        private void AdjacentNodeClick()
        {
            var node = DungeonHelper.NodeUiManager
                .FindNodeUI(DungeonHelper.CurrentAdjacentNodes.ToArray()[0].NodeId).GetNodeModel();
            DungeonHelper.MapManager.TryUpdatePlayerPosition(node);
            DungeonHelper.MapManager._encounterManager.ExecuteEncounter(node);
        }

        public override void Update()
        {
        }

        private void OnWinRateToggleClick()
        {
            BattleHelper.SetToggleToWinRate();
        }

        private void OnCreateCommandClick()
        {
            BattleHelper.CompleteCommand();
        }
    }
}