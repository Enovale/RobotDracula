using BattleUI;
using BattleUI.Operation;

namespace RobotDracula.Battle
{
    public static class BattleHelper
    {
        private static BattleUIRoot _uiRoot => SingletonBehavior<BattleUIRoot>.Instance;
        
        private static NewOperationController _controller
        {
            get
            {
                var controller = _uiRoot.NewOperationController;
                if (controller is null)
                    Plugin.PluginLog.LogError($"{nameof(NewOperationController)} couldn't be found.");

                return controller;
            }
        }

        private static NewOperationAutoSelectManager _autoSelectManager
        {
            get
            {
                if (_controller._autoSelectManager is null)
                    Plugin.PluginLog.LogError($"{nameof(NewOperationAutoSelectManager)} couldn't be found.");
                    
                return _controller._autoSelectManager;
            }
        }
        
        public static STAGE_PHASE StagePhase => Singleton<StageController>.Instance.Phase;

        public static void SetToggleToWinRate()
        {
            if (_autoSelectManager is not null && !_autoSelectManager.toggle_winRate.isOn)
                _autoSelectManager.SetWinRateToggle(true);
        }
        
        public static void CompleteCommand()
        {
            if (_controller is not null)
                _controller.CompleteCommand();
        }
    }
}