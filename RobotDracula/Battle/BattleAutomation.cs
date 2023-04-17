using UnityEngine;

namespace RobotDracula.Battle;

public static class BattleAutomation
{
    private static float doActionCooldown = 0f;
    public static void HandleBattleAutomation()
    {
        if (doActionCooldown <= 0)
        {
            BattleHelper.SetToggleToWinRate();
            BattleHelper.CompleteCommand();
            doActionCooldown = 5f;
        }

        doActionCooldown -= Time.fixedDeltaTime;
    }
}