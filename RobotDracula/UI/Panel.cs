using System.Linq;
using Dungeon;
using Dungeon.Map;
using Il2CppSystem.Text;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
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

        public override string Name => "My Panel";
        public override int MinWidth => 280;
        public override int MinHeight => 400;
        public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
        public override Vector2 DefaultAnchorMax => new(0.25f, 0.25f);
        public override bool CanDragAndResize => true;

        protected override void ConstructPanelContent()
        {
            var myToggle2 = UIFactory.CreateToggle(ContentRoot, "myToggle2", out Toggle toggle2, out Text text2);
            UIFactory.SetLayoutElement(myToggle2.gameObject, flexibleWidth: 200, flexibleHeight: 8);
            text2.text = "Automate Battle";
            toggle2.isOn = Plugin.BattleAutomationEnabled;
            toggle2.onValueChanged.AddListener(val => Plugin.BattleAutomationEnabled = val);
            
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
            var myBtn2 = UIFactory.CreateButton(ContentRoot, "myBtn2", "Click First Adjacent Battle Node");
            UIFactory.SetLayoutElement(myBtn2.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn2.OnClick = AdjacentNodeClick;
            var myToggle = UIFactory.CreateToggle(ContentRoot, "myToggle", out var toggle, out var text);
            UIFactory.SetLayoutElement(myToggle.gameObject, flexibleWidth: 200, flexibleHeight: 8);
            text.text = "Move Cheat";
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(val => DungeonHelper.MapManager._canMoveCheat = val);
        }

        private void AdjacentNodeClick()
        {
            // Gets every node in the entire dungeon
            var next = DungeonHelper.MirrorMapManager._nodesByFloor;

            // Prints out all of said nodes (throws errors lol but dw about that)
            var strb = new StringBuilder();
            foreach (var f in next.Values)
            {
                foreach (var nodeInfo in f)
                {
                    strb.Append($"\n[NODE {nodeInfo.nodeId}]: Type: {nodeInfo.GetEncounterType()} Floor: {nodeInfo.floorNumber} Sector: {nodeInfo.sectorNumber} Encounter ID: {nodeInfo.encounterId}");
                }
            }
            Plugin.PluginLog.LogWarning(strb.ToString());

            // Filters down to our current sector
            var thisFloor = next[DungeonProgressManager.FloorNumber].ToArray();
            var nextSector = thisFloor.Where(n => n.sectorNumber == DungeonProgressManager.SectorNumber + 1);

            NodeModel chosenNode = null;
            foreach (var nodeInfo in nextSector)
            {
                if (chosenNode != null)
                    break;
                
                // Get the first node that isn't an event
                if (nodeInfo.IsBattleNode())
                {
                    // Gets the nodemodel in a faster way than asking the NodeUI
                    chosenNode = DungeonHelper.MirrorMapManager._nodeDictionary[nodeInfo.nodeId].NodeModel;
                    
                    // Makes sure we can travel to that node validly
                    if (!DungeonHelper.MirrorMapManager.IsValidNode(DungeonHelper.MirrorMapManager.GetCurrentNode(),
                            chosenNode))
                    {
                        chosenNode = null;
                    }
                }
            }

            // Move the train and open the Enter panel (not required)
            //DungeonHelper.MirrorMapManager.TryUpdatePlayerPosition(chosenNode);
            
            // Actually enters the encounter
            // NOTE: there needs to be a valid path or the server will not allow you to advance after encounter
            DungeonHelper.MirrorMapManager._encounterManager.ExecuteEncounter(chosenNode);
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