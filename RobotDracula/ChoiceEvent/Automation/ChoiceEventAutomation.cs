using System;
using System.Linq;
using ChoiceEvent;
using RobotDracula.General;
using RobotDracula.Trainer;
using static PERSONALITY_CHOICE_PROGRESSING_SECTION;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        public static void HandleChoiceEventAutomation(ChoiceEventController controller)
        {
            var choiceSectionUI = controller._choiceSectionUI;
            var personalityChoiceManager = choiceSectionUI.PersonalityChoiceButtonManager;
            var resultSectionUI = controller._resultSectionUI;
            if (choiceSectionUI._actionChoiceButtonManager.isActiveAndEnabled)
            {
                var actionData = choiceSectionUI._actionProgressData;
                if (_choiceActionDict.TryGetValue(controller.GetCurrentEventID(), out var i))
                {
                    if (actionData.ActionChoiceActionList._items[i])
                    {
                        choiceSectionUI.OnClickActionChoiceButton(i);
                    }
                    else
                    {
                        for (var j = 0; j < actionData.ActionChoiceActionList.Count; j++)
                        {
                            if (!actionData.ActionChoiceActionList._items[j])
                                continue;

                            choiceSectionUI.OnClickActionChoiceButton(j);
                            break;
                        }
                    }
                }
                else
                {
                    Plugin.PluginLog.LogWarning(
                        $"No action is registered for event ID {controller.GetCurrentEventID()}");
                    Plugin.PluginLog.LogWarning("Action Options: " + string.Join(", ",
                        actionData._actionEventTextData.options.ToArray().Select(o => o.Message)));
                    TrainerManager.SetCooldown(5f);
                    /*
                    for (var j = 0; j < actionData.ActionChoiceActionList.Count; j++)
                    {
                        if (!actionData.ActionChoiceActionList._items[j])
                            continue;
                        
                        choiceSectionUI.OnClickActionChoiceButton(j);
                        break;
                    }
                    */
                }
            }
            else if (personalityChoiceManager._currentSectionType is CHOICE or RESULT_CALCULATING)
            {
                if (personalityChoiceManager.cg_self.interactable)
                {
                    var buttons = personalityChoiceManager._personalityChoiceButtons;
                    var highestIndex = 0;
                    var highestRate = 0f;
                    for (var i = 0; i < personalityChoiceManager._currentButtonInUseCount; ++i)
                    {
                        var unitDataModel = buttons[(Index)i].Cast<PersonalityChoiceButton>().UnitModel;
                        var winRate = controller._eventProgressData.PersonalityChoiceData.GetWinRate(unitDataModel);

                        if (highestRate < winRate &&
                            !buttons[(Index)i].Cast<PersonalityChoiceButton>().CantSelectButton())
                        {
                            highestRate = winRate;
                            highestIndex = i;
                        }
                    }

                    personalityChoiceManager.CallbackOnButtonSelected(highestIndex);
                    choiceSectionUI.OnClickPersonalityJudgeButton();
                }
            }

            if (resultSectionUI is { btn_confirm.isActiveAndEnabled: true })
            {
                resultSectionUI.btn_confirm.btn_confirm.OnClick(false);
            }

            if (resultSectionUI is { btn_battleEnter.isActiveAndEnabled: true })
            {
                resultSectionUI.btn_battleEnter.onClick.Invoke();
            }

            if (choiceSectionUI._topPanelScrollBox is { isActiveAndEnabled: true, coroutine_flickeringTriangle: not null })
            {
                choiceSectionUI._topPanelScrollBox.OnClickStorySkipButton();
                UtilHelper.MouseButtonUp = true;
            }

            if (resultSectionUI._activeResultDesc is { isActiveAndEnabled: true, coroutine_flickeringTriangle: not null })
            {
                resultSectionUI._activeResultDesc.OnClickStorySkipButton();
                UtilHelper.MouseButtonUp = true;
            }

            if (resultSectionUI._scenarioScrollBox is { isActiveAndEnabled: true, coroutine_flickeringTriangle: not null })
            {
                resultSectionUI._scenarioScrollBox.OnClickStorySkipButton();
                UtilHelper.MouseButtonUp = true;
            }
        }
    }
}