using System;
using System.Linq;
using ChoiceEvent;
using Dungeon.Map;
using RobotDracula.Dungeon;
using RobotDracula.General;
using UnityEngine;
using static PERSONALITY_CHOICE_PROGRESSING_SECTION;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        private static float _eventChoiceCooldown;
        
        public static void HandleChoiceEventAutomation(ChoiceEventController controller)
        {
            // TODO: Temporary measure because shops currently break everything horribly
            if (DungeonHelper.CachedCurrentNodeModel?.encounter is ENCOUNTER.MIRROR_SHOP or ENCOUNTER.MIRROR_SELECT_EVENT)
                return;
            
            if (_eventChoiceCooldown <= 0)
            {
                _eventChoiceCooldown = 0.75f;
                
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
                        Plugin.PluginLog.LogWarning($"No action is registered for event ID {controller.GetCurrentEventID()}");
                        Plugin.PluginLog.LogWarning("Action Options: " + string.Join(", ", actionData._actionEventTextData.options.ToArray().Select(o => o.Message)));
                        _eventChoiceCooldown = 30f;
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
                
                if (resultSectionUI.btn_battleEnter.isActiveAndEnabled)
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

            _eventChoiceCooldown -= Time.fixedDeltaTime;
        }
    }
}