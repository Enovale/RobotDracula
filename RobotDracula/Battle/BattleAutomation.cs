using UnityEngine;

namespace RobotDracula.Battle
{
    public static class BattleAutomation
    {
        public static bool DoWinRateAutomation = true;
    
        private static float _doActionCooldown;
    
        public static void HandleBattleAutomation()
        {
            if (_doActionCooldown <= 0)
            {
                if (DoWinRateAutomation)
                    BattleHelper.SetToggleToWinRate();
                else
                    BattleHelper.SetToggleToDamage();
                BattleHelper.CompleteCommand();
                _doActionCooldown = 1f;
            }

            _doActionCooldown -= Time.fixedDeltaTime;
        }
    }
}