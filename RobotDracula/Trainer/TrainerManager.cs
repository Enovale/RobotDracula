using RobotDracula.Battle.Automation;
using RobotDracula.ChoiceEvent.Automation;
using RobotDracula.Dungeon;
using RobotDracula.Dungeon.Automation;
using RobotDracula.General;

namespace RobotDracula.Trainer
{
    public static class TrainerManager
    {
        public static bool BattleAutomationEnabled = false;

        public static bool DungeonAutomationEnabled = false;
        
        public static bool ChoiceEventAutomationEnabled = false;

        public delegate void BattleUpdateEventHandler();

        public static event BattleUpdateEventHandler BattleUpdate;

        public delegate void MirrorLevelUpRewardEventHandler();

        public static event MirrorLevelUpRewardEventHandler LevelUpUpdate;

        public delegate void MirrorNewCharacterRewardEventHandler();

        public static event MirrorNewCharacterRewardEventHandler NewCharacterUpdate;

        public delegate void MirrorEgoGiftRewardEventHandler();

        public static event MirrorEgoGiftRewardEventHandler EgoGiftUpdate;

        public delegate void MirrorFormationEventHandler();

        public static event MirrorFormationEventHandler FormationUpdate;

        public delegate void MirrorDungeonEventChoiceEventHandler();

        public static event MirrorDungeonEventChoiceEventHandler MirrorDungeonEventChoiceUpdate;

        public delegate void MirrorDungeonMapEventHandler();

        public static event MirrorDungeonMapEventHandler MirrorDungeonMapUpdate;
        
        public delegate void TrainerUpdateEventHandler();
        
        public static event TrainerUpdateEventHandler TrainerUpdate;

        static TrainerManager()
        {
            BattleUpdate += BattleAutomation.HandleBattleAutomation;
            MirrorDungeonMapUpdate += DungeonAutomation.HandleDungeonAutomation;
            MirrorDungeonEventChoiceUpdate += ChoiceEventAutomation.HandleChoiceEventAutomation;
            FormationUpdate += DungeonAutomation.HandleFormationAutomation;
            LevelUpUpdate += DungeonAutomation.HandleLevelUpAutomation;
            NewCharacterUpdate += DungeonAutomation.HandleNewCharacterAutomation;
            EgoGiftUpdate += DungeonAutomation.HandleEgoGiftAutomation;
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

            if (ChoiceEventAutomationEnabled)
            {
                if (DungeonHelper.DungeonUIManager is {_choiceEventController.IsActivated: true})
                {
                    MirrorDungeonEventChoiceUpdate?.Invoke();
                }
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameManager.Instance.CheckSceneState(SCENE_STATE.MirrorDungeon))
                {
                    if (DungeonHelper.MirrorDungeonManager is {StageReward._characterLevelUpView.IsOpened: true})
                    {
                        LevelUpUpdate?.Invoke();
                    }
                    // The last two don't use the IsOpened paradigm. Why? Who knows.
                    // Also the other of the ego gift and new recruit matter apparently.
                    // After completing a floor and getting a gift and a recruit, both are opened at the same time
                    // but the ego gift is rendered on top so it must be checked first.
                    else if (DungeonHelper.MirrorDungeonManager is {StageReward._acquireEgoGiftView.gameObject.active: true})
                    {
                        EgoGiftUpdate?.Invoke();
                    }
                    else if (DungeonHelper.MirrorDungeonManager is {StageReward._acquireNewCharacterView.gameObject.active: true})
                    {
                        NewCharacterUpdate?.Invoke();
                    }
                    else if (SingletonBehavior<DungeonFormationPanel>.Instance is {gameObject.active: true })
                    {
                        FormationUpdate?.Invoke();
                    }
                    else if (DungeonHelper.MirrorDungeonManager is {StageReward.RewardStatusData.IsAllFinished: true})
                    {
                        MirrorDungeonMapUpdate?.Invoke();    
                    }
                }
            }
        }
    }
}