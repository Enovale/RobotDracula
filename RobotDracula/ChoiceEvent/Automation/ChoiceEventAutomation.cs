using RobotDracula.Dungeon;
using UnityEngine;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        private static float _eventChoiceCooldown;
        
        public static void HandleChoiceEventAutomation()
        {
            if (_eventChoiceCooldown <= 0)
            {
                _eventChoiceCooldown = 1f;
                
                var choiceEventController = DungeonHelper.DungeonUIManager._choiceEventController;
                var id = choiceEventController.GetCurrentEventID();

                Plugin.PluginLog.LogInfo(id);
                Plugin.PluginLog.LogInfo(choiceEventController._eventProgressData.CurrentEventID);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._actionChoiceButtonManager
                    .isActiveAndEnabled);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._topPanelScrollBox._isSkipContent);
                Plugin.PluginLog.LogInfo(choiceEventController._choiceSectionUI._behaveScrollBox._isSkipContent);
                if (choiceEventController._choiceSectionUI._actionChoiceButtonManager.isActiveAndEnabled)
                {
                    if (_choiceActionDict.TryGetValue(id, out var actions))
                    {
                        //choiceEventController._choiceSectionUI.OnClickActionChoiceButton(actions);
                    }
                }

                if (!choiceEventController._choiceSectionUI._topPanelScrollBox._isSkipContent)
                {
                    choiceEventController._choiceSectionUI._topPanelScrollBox.OnClickStorySkipButton();
                    // TODO: Need to simulate a click or find the coroutine that uses this because
                    // It won't skip and do the things until a click even if its skipping
                }
            }

            _eventChoiceCooldown -= Time.fixedDeltaTime;
        }
    }
}