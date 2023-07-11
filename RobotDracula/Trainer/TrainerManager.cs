using BattleUI;
using Dungeon;
using MainUI;
using RobotDracula.Dungeon;
using RobotDracula.General;
using UnityEngine;
using static MainUI.MAINUI_PANEL_TYPE;

namespace RobotDracula.Trainer
{
    public static partial class TrainerManager
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

        private static float _trainerCooldown;

        public static void SetCooldown(float value)
        {
            _trainerCooldown = value;
        }

        public static void Update()
        {
            UtilHelper.MouseButtonUp = false;
            
            TrainerUpdate?.Invoke();

            // Don't cooldown at all if we hit a lag spike
            // This can happen in the New Character panel
            if (Time.deltaTime < 0.5f)
                _trainerCooldown -= Time.deltaTime;

            if (_trainerCooldown > 0)
                return;

            _trainerCooldown = 0.5f;
            
            if (BattleAutomationEnabled && GlobalGameHelper.SceneState is SCENE_STATE.Battle)
            {
                BattleUpdate?.Invoke();
            }

            if (ChoiceEventAutomationEnabled)
            {
                if (GlobalGameManager.Instance.sceneState is SCENE_STATE.Battle)
                {
                    if (SingletonBehavior<BattleUIRoot>.Instance is { AbUIController._choiceEventController.IsActivated: true })
                        MirrorDungeonEventChoiceUpdate?.Invoke(SingletonBehavior<BattleUIRoot>.Instance.AbUIController._choiceEventController);
                }
                else if (GlobalGameHelper.IsInDungeon())
                {
                    if (DungeonHelper.MirrorDungeonUIManager is { _choiceEventController.IsActivated: true, _shopUI.isActiveAndEnabled: false })
                        MirrorDungeonEventChoiceUpdate?.Invoke(DungeonHelper.DungeonUIManager._choiceEventController);
                }
            }

            if (DungeonAutomationEnabled)
            {
                if (GlobalGameHelper.IsInDungeon() && DungeonProgressManager.IsOnDungeon())
                {
                    if (DungeonHelper.DungeonUIManager is {_egoGiftPopup.IsOpened: true})
                        EgoGiftPopupUpdate?.Invoke();

                    if (DungeonHelper.StageReward is { _characterLevelUpView.IsOpened: true })
                        LevelUpUpdate?.Invoke();
                    // The last two don't use the IsOpened paradigm. Why? Who knows.
                    // Also the order of the ego gift and new recruit matter apparently.
                    // After completing a floor and getting a gift and a recruit, both are opened at the same time
                    // but the ego gift is rendered on top so it must be checked first.
                    else if (DungeonHelper.StageReward is { _acquireEgoGiftView.gameObject.active: true })
                        EgoGiftUpdate?.Invoke(DungeonHelper.StageReward._acquireEgoGiftView);
                    else if (DungeonHelper.StageReward is { _acquireNewCharacterView.gameObject.active: true })
                        NewCharacterUpdate?.Invoke(DungeonHelper.StageReward._acquireNewCharacterView);
                    else if (SingletonBehavior<DungeonFormationPanel>.Instance is { gameObject.active: true })
                        FormationUpdate?.Invoke();
                    else if (DungeonHelper.MirrorDungeonUIManager is { _shopUI.isActiveAndEnabled: true })
                        MirrorDungeonShopUpdate?.Invoke(DungeonHelper.MirrorDungeonUIManager._shopUI);
                    else if (DungeonHelper.StageReward is {RewardStatusData.IsAllFinished: true})
                        MirrorDungeonMapUpdate?.Invoke();
                }
                else if (GlobalGameHelper.SceneState is SCENE_STATE.Main)
                {
                    if (UIPresenter.Controller != null)
                    {
                        var egoGiftPanel = UIPresenter.Controller.GetPanel(SELECT_EGO_GIFT)
                            ?.TryCast<SelectEgoGiftPanel>();
                        //var personalityPanel = (FormationSwitchablePersonalityUIPanel)UIPresenter.Controller.GetPanel(SELECT_PERSONALITY_FOR_DUNGEON);
                        if (egoGiftPanel is { gameObject.active: true })
                            EgoGiftUpdate?.Invoke(egoGiftPanel);

                        var newCharPanel = UIPresenter.Controller.GetPanel(SELECT_PERSONALITY_FOR_DUNGEON)
                            ?.TryCast<RandomDungeonAcquireCharacterPanel>();
                        if (newCharPanel is { gameObject.active: true })
                            NewCharacterUpdate?.Invoke(newCharPanel);
                    }
                }
            }
        }
    }
}