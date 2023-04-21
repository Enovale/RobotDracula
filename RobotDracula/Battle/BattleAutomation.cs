using UnityEngine;

namespace RobotDracula.Battle
{
    public static class BattleAutomation
    {
        private static float _doActionCooldown = 0f;
    
        public static void HandleBattleAutomation()
        {
            if (_doActionCooldown <= 0)
            {
                BattleHelper.SetToggleToWinRate();
                BattleHelper.CompleteCommand();
                _doActionCooldown = 5f;
            }

            _doActionCooldown -= Time.fixedDeltaTime;
        }
    }
}