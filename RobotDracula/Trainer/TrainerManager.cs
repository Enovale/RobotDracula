using BattleUI;
using ChoiceEvent;
using Dungeon;
using MainUI;
using RobotDracula.Battle.Automation;
using RobotDracula.ChoiceEvent.Automation;
using RobotDracula.Dungeon;
using RobotDracula.Dungeon.Automation;
using RobotDracula.General;
using static MainUI.MAINUI_PANEL_TYPE;

namespace RobotDracula.Trainer
{
    public static class TrainerManager
    {
        public static bool BattleAutomationEnabled
        {
            get => Plugin.BattleAutomationEnabled.Value;
            set => Plugin.BattleAutomationEnabled.Value = value;
        }

        public static bool DungeonAutomationEnabled
        {
            get => Plugin.DungeonAutomationEnabled.Value;
            set => Plugin.DungeonAutomationEnabled.Value = value;
        }

        public static bool ChoiceEventAutomationEnabled
        {
            get => Plugin.EventAutomationEnabled.Value;
            set => Plugin.EventAutomationEnabled.Value = value;
        }

        public delegate void BattleUpdateEventHandler();

        public static event BattleUpdateEventHandler BattleUpdate;

        public delegate void MirrorLevelUpRewardEventHandler();

        public static event MirrorLevelUpRewardEventHandler LevelUpUpdate;

        public delegate void MirrorNewCharacterRewardEventHandler(RandomDungeonAcquireCharacterPanel panel);

        public static event MirrorNewCharacterRewardEventHandler NewCharacterUpdate;

        public delegate void MirrorEgoGiftRewardEventHandler(SelectEgoGiftPanel panel);

        public static event MirrorEgoGiftRewardEventHandler EgoGiftUpdate;

        public delegate void MirrorEgoGiftPopupEventHandler();

        public static event MirrorEgoGiftPopupEventHandler EgoGiftPopupUpdate;

        public delegate void MirrorFormationEventHandler();

        public static event MirrorFormationEventHandler FormationUpdate;

        public delegate void MirrorDungeonEventChoiceEventHandler(ChoiceEventController controller);

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
            EgoGiftPopupUpdate += DungeonAutomation.HandleCloseAnnoyingEgoGiftPopup;
            TrainerUpdate += GeneralAutomation.HandleTimescaleUpdate;
        }

        public static void Update()
        {
            UtilHelper.MouseButtonUp = false;
            
            // Need to know when a dungeon has been reloaded
            if (GlobalGameHelper.SceneState is SCENE_STATE.Main)
                DungeonAutomation.ResetPathfinding();
            
            TrainerUpdate?.Invoke();
            
            if (BattleAutomationEnabled && GlobalGameHelper.SceneState is SCENE_STATE.Battle)
            {
                BattleUpdate?.Invoke();
            }

            if (ChoiceEventAutomationEnabled)
            {
                if (GlobalGameManager.Instance.sceneState is SCENE_STATE.Battle)
                {
                    if (SingletonBehavior<BattleUIRoot>.Instance is
                             { AbUIController._choiceEventController.IsActivated: true })
                    {
                        MirrorDungeonEventChoiceUpdate?.Invoke(SingletonBehavior<BattleUIRoot>.Instance.AbUIController
                            ._choiceEventController);
                    }
                }
                else if (GlobalGameHelper.IsInDungeon())
                {
                    if (DungeonHelper.DungeonUIManager is { _choiceEventController.IsActivated: true })
                    {
                        MirrorDungeonEventChoiceUpdate?.Invoke(DungeonHelper.DungeonUIManager._choiceEventController);
                    }
                }
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameHelper.IsInDungeon() && DungeonProgressManager.IsOnDungeon())
                {
                    if (DungeonHelper.DungeonUIManager is {_egoGiftPopup.IsOpened: true})
                    {
                        EgoGiftPopupUpdate?.Invoke();
                    }
                    
                    if (DungeonHelper.StageReward is {_characterLevelUpView.IsOpened: true})
                    {
                        LevelUpUpdate?.Invoke();
                    }
                    // The last two don't use the IsOpened paradigm. Why? Who knows.
                    // Also the order of the ego gift and new recruit matter apparently.
                    // After completing a floor and getting a gift and a recruit, both are opened at the same time
                    // but the ego gift is rendered on top so it must be checked first.
                    else if (DungeonHelper.StageReward is {_acquireEgoGiftView.gameObject.active: true})
                    {
                        EgoGiftUpdate?.Invoke(DungeonHelper.StageReward._acquireEgoGiftView);
                    }
                    else if (DungeonHelper.StageReward is {_acquireNewCharacterView.gameObject.active: true})
                    {
                        NewCharacterUpdate?.Invoke(DungeonHelper.StageReward._acquireNewCharacterView);
                    }
                    else if (SingletonBehavior<DungeonFormationPanel>.Instance is {gameObject.active: true })
                    {
                        FormationUpdate?.Invoke();
                    }
                    else if (DungeonHelper.StageReward is {RewardStatusData.IsAllFinished: true})
                    {
                        MirrorDungeonMapUpdate?.Invoke();
                    }
                }
                else if (GlobalGameHelper.SceneState is SCENE_STATE.Main)
                {
                    if (UIPresenter.Controller != null)
                    {
                        var egoGiftPanel = UIPresenter.Controller.GetPanel(SELECT_EGO_GIFT)
                            ?.TryCast<SelectEgoGiftPanel>();
                        //var personalityPanel = (FormationSwitchablePersonalityUIPanel)UIPresenter.Controller.GetPanel(SELECT_PERSONALITY_FOR_DUNGEON);
                        if (egoGiftPanel is { gameObject.active: true })
                        {
                            EgoGiftUpdate?.Invoke(egoGiftPanel);
                        }
                        
                        var newCharPanel = UIPresenter.Controller.GetPanel(SELECT_PERSONALITY_FOR_DUNGEON)
                            ?.TryCast<RandomDungeonAcquireCharacterPanel>();
                        if (newCharPanel is { gameObject.active: true })
                        {
                            NewCharacterUpdate?.Invoke(newCharPanel);
                        }
                    }
                }
            }
        }
    }
}