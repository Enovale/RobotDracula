using BattleUI;
using BattleUI.Operation;
using RobotDracula.Trainer;
using UnityEngine;

namespace RobotDracula.Battle
{
    public static class BattleHelper
    {
        private static float doActionCooldown = 0f;
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

        static BattleHelper()
        {
            TrainerManager.BattleUpdate += HandleBattleAutomation;
        }

        public static void HandleBattleAutomation()
        {
            if (doActionCooldown <= 0)
            {
                SetToggleToWinRate();
                CompleteCommand();
                doActionCooldown = 5f;
            }

            doActionCooldown -= Time.fixedDeltaTime;
        }

        public static void SetToggleToWinRate()
        {
            if (_autoSelectManager is not null && !_autoSelectManager.toggle_winRate.isOn)
                _autoSelectManager.SetWinRateToggle(true);
        }

        public static void SetToggleToDamage()
        {
            if (_autoSelectManager is not null && !_autoSelectManager.toggle_damage.isOn)
                _autoSelectManager.SetDamageToggle(true);
        }
        
        public static void CompleteCommand()
        {
            if (_controller is not null)
                _controller.CompleteCommand();
        }
    }
}