using RobotDracula.Battle;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace RobotDracula.UI
{
    public class Panel : UniverseLib.UI.Panels.PanelBase
    {
        public Panel(UIBase owner) : base(owner) { }

        public override string Name => "My Panel";
        public override int MinWidth => 100;
        public override int MinHeight => 200;
        public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
        public override Vector2 DefaultAnchorMax => new(0.75f, 0.75f);
        public override bool CanDragAndResize => true;

        private Text _phaseText;

        protected override void ConstructPanelContent()
        {
            _phaseText = UIFactory.CreateLabel(ContentRoot, "myText", "Hello world");
            UIFactory.SetLayoutElement(_phaseText.gameObject);
            var myBtn = UIFactory.CreateButton(ContentRoot, "myBtn", "Complete Command");
            UIFactory.SetLayoutElement(myBtn.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn.OnClick = OnCreateCommandClick;
            var myBtn3 = UIFactory.CreateButton(ContentRoot, "myBtn3", "Win Rate Toggle");
            UIFactory.SetLayoutElement(myBtn3.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn3.OnClick = OnWinRateToggleClick;
            var myBtn4 = UIFactory.CreateButton(ContentRoot, "myBtn4", "Damage Toggle");
            UIFactory.SetLayoutElement(myBtn4.GameObject, flexibleWidth: 200, flexibleHeight: 24);
            myBtn4.OnClick = OnWinRateToggleClick;
        }

        public override void Update()
        {
            _phaseText.text = $"Current Battle Phase: {BattleHelper.StagePhase.ToString()}";
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