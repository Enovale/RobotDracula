using System.Collections.Generic;
using System.Linq;

namespace RobotDracula.Battle.Automation
{
    public static class BattleAutomation
    {
        private static List<int> _unitIDsForDamageAuto = new()
        {
            8032, // Lightning Beast
            8031, // Water Fish
            8001 // Legally Destinct Snow White
        };

        public static void HandleBattleAutomation()
        {
            var stageController = Singleton<StageController>.Instance;
            if (stageController.Phase == STAGE_PHASE.WAIT_COMMAND)
            {
                var enemyData = stageController.StageModel.GetAllEnemyDataByEnemy().ToArray().ToList();
                if (enemyData.Any(e => _unitIDsForDamageAuto.Contains(e.GetID())))
                {
                    Plugin.PluginLog.LogWarning("Using damage because enemyData contains a damage enemy!");
                    BattleHelper.SetToggleToDamage();
                }
                else if (stageController.StageType == STAGE_BATTLE_TYPE.Abnormality)
                    BattleHelper.SetToggleToWinRate();
                else
                    BattleHelper.SetToggleToDamage();

                BattleHelper.CompleteCommand();
            }
        }
    }
}