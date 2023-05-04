using System;
using ChoiceEvent;
using RobotDracula.Dungeon;
using RobotDracula.General;
using UnityEngine;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        private static float _eventChoiceCooldown;
        
        public static void HandleChoiceEventAutomation()
        {
            // TODO: I wrote this shit at 5 am help me pls 
            Plugin.PluginLog.LogInfo(DungeonHelper.DungeonUIManager._choiceEventController._choiceSectionUI._topPanelScrollBox.isActiveAndEnabled);
            Plugin.PluginLog.LogInfo(DungeonHelper.DungeonUIManager._choiceEventController._choiceSectionUI._topPanelScrollBox._isSkipContent);
            Plugin.PluginLog.LogInfo(DungeonHelper.DungeonUIManager._choiceEventController._choiceSectionUI._behaveScrollBox.isActiveAndEnabled);
            Plugin.PluginLog.LogInfo(DungeonHelper.DungeonUIManager._choiceEventController._choiceSectionUI._behaveScrollBox._isSkipContent);
            if (_eventChoiceCooldown <= 0)
            {
                _eventChoiceCooldown = 1f;
                
                var choiceEventController = DungeonHelper.DungeonUIManager._choiceEventController;
                var choiceSectionUI = choiceEventController._choiceSectionUI;
                var personalityChoiceManager = choiceSectionUI.PersonalityChoiceButtonManager;
                var resultSectionUI = choiceEventController._resultSectionUI;
                if (choiceSectionUI._actionChoiceButtonManager.isActiveAndEnabled)
                {
                    if (_choiceActionDict.TryGetValue(choiceEventController.GetCurrentEventID(), out var i))
                    {
                        choiceSectionUI.OnClickActionChoiceButton(i);
                    }
                }
                // TODO: Either gets called all the time or never cant figure out what to check for here
                else if (personalityChoiceManager.cg_self.interactable)
                {
                    Plugin.PluginLog.LogWarning(personalityChoiceManager._currentSectionType);
                    if (!resultSectionUI.isActiveAndEnabled)
                    {
                        var buttons = personalityChoiceManager._personalityChoiceButtons;
                        var highestIndex = 0;
                        var highestRate = 0f;
                        for (var i = 0; i < personalityChoiceManager._currentButtonInUseCount; ++i)
                        {
                            var unitDataModel = buttons[(Index)i].Cast<PersonalityChoiceButton>().UnitDataModel;
                            var winRate = choiceEventController._eventProgressData.PersonalityChoiceData.GetWinRate(unitDataModel);

                            if (highestRate < winRate && !buttons[(Index)i].Cast<PersonalityChoiceButton>().CantSelectButton())
                            {
                                highestRate = winRate;
                                highestIndex = i;
                            }
                        }
                    
                        personalityChoiceManager.CallbackOnButtonSelected(highestIndex);
                        choiceSectionUI.OnClickPersonalityJudgeButton();
                    }
                }

                if (resultSectionUI.btn_confirm.isActiveAndEnabled)
                {
                    resultSectionUI.btn_confirm.btn_confirm.OnClick(false);
                }
                else if (resultSectionUI.btn_battleEnter.isActiveAndEnabled)
                {
                    resultSectionUI.btn_battleEnter.onClick.Invoke();
                }

                if (choiceSectionUI._topPanelScrollBox.isActiveAndEnabled && !choiceSectionUI._topPanelScrollBox._isSkipContent)
                {
                    choiceSectionUI._topPanelScrollBox.OnClickStorySkipButton();
                    // TODO: Need to simulate a click or find the coroutine that uses this because
                    // It won't skip and do the things until a click even if its skipping
                    UtilHelper.MouseButtonUp = true;
                }

                if (resultSectionUI._activeResultDesc.isActiveAndEnabled && !resultSectionUI._activeResultDesc._isSkipContent)
                {
                    resultSectionUI._activeResultDesc.OnClickStorySkipButton();
                    // TODO: Need to simulate a click or find the coroutine that uses this because
                    // It won't skip and do the things until a click even if its skipping
                    UtilHelper.MouseButtonUp = true;
                }
            }

            _eventChoiceCooldown -= Time.fixedDeltaTime;
        }
    }
}