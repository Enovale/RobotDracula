using Dungeon;
using RobotDracula.Battle;
using RobotDracula.Dungeon;
using RobotDracula.General;

namespace RobotDracula.Trainer
{
    public static class TrainerManager
    {
        public static bool BattleAutomationEnabled = false;

        public static bool DungeonAutomationEnabled = false;

        public static bool DungeonLevelUpAutomationEnabled = false;

        public delegate void BattleUpdateEventHandler();

        public static event BattleUpdateEventHandler BattleUpdate;

        public delegate void MirrorLevelUpRewardEventHandler();

        public static event MirrorLevelUpRewardEventHandler LevelUpUpdate;

        public delegate void MirrorNewCharacterRewardEventHandler();

        public static event MirrorNewCharacterRewardEventHandler NewCharacterUpdate;

        public delegate void MirrorEgoGiftRewardEventHandler();

        public static event MirrorEgoGiftRewardEventHandler EgoGiftUpdate;

        public delegate void MirrorDungeonMapEventHandler();

        public static event MirrorDungeonMapEventHandler MirrorDungeonMapUpdate;
        
        public delegate void TrainerUpdateEventHandler();
        
        public static event TrainerUpdateEventHandler TrainerUpdate;

        static TrainerManager()
        {
            BattleUpdate += BattleAutomation.HandleBattleAutomation;
            MirrorDungeonMapUpdate += DungeonAutomation.HandleDungeonAutomation;
            LevelUpUpdate += DungeonAutomation.HandleLevelUpAutomation;
            TrainerUpdate += GeneralAutomation.HandleFPSUncap;
            TrainerUpdate += GeneralAutomation.HandleTimescaleUpdate;
        }

        public static void Update()
        {
            TrainerUpdate?.Invoke();
            
            if (BattleAutomationEnabled && GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.Battle))
            {
                BattleUpdate?.Invoke();
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.MirrorDungeon))
                {
                    if (DungeonHelper.MirrorDungeonManager.StageReward._characterLevelUpView is {IsOpened: true})
                    {
                        LevelUpUpdate?.Invoke();
                    }
                    else if (DungeonHelper.MirrorDungeonManager.StageReward._acquireNewCharacterView is {IsOpened: true})
                    {
                        NewCharacterUpdate?.Invoke();
                    }
                    else if (DungeonHelper.MirrorDungeonManager.StageReward._acquireEgoGiftView is {IsOpened: true} || 
                             DungeonUIManager.Instance._egoGiftPopup is {IsOpened: true})
                    {
                        EgoGiftUpdate?.Invoke();
                    }
                    else
                    {
                        MirrorDungeonMapUpdate?.Invoke();    
                    }
                }
            }
        }
    }
}