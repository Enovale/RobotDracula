using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace RobotDracula.UI
{
    public static class UiHelper
    {
        private static readonly Dictionary<Text, Func<string>> _labelDict = new();
        private static readonly Dictionary<Toggle, Func<bool>> _toggleDict = new();
        private static readonly Dictionary<Slider, Func<float>> _sliderDict = new();

        public static Text CreateLabel(GameObject parent, string name, Func<string> textGetter)
        {
            var startText = string.Empty;

            var text = UIFactory.CreateLabel(parent, name, startText);
            _labelDict.Add(text, textGetter);

            return text;
        }

        public static GameObject CreateToggle(
            GameObject parent,
            string name,
            string defaultText,
            bool defaultValue,
            Func<bool> valueGetter,
            Action<bool> onChangeAction,
            out Toggle toggle,
            out Text text)
        {
            var toggleGO = UIFactory.CreateToggle(parent, name, out toggle, out text);

            if (defaultText is not null)
                text.text = defaultText;

            toggle.isOn = defaultValue;

            if (valueGetter is not null)
                _toggleDict.Add(toggle, valueGetter);

            if (onChangeAction is not null)
            {
                toggle.onValueChanged.AddListener(onChangeAction);
            }

            return toggleGO;
        }

        public static GameObject CreateToggle(
            GameObject parent,
            string name,
            string defaultText,
            bool defaultValue,
            Action<bool> onChangeAction,
            out Toggle toggle,
            out Text text)
            => CreateToggle(parent, name, defaultText, defaultValue, null, onChangeAction, out toggle, out text);

        public static ButtonRef CreateButton(GameObject parent, string name, string text, Action onClick = null, Color? normalColor = null)
        {
            var btn = UIFactory.CreateButton(parent, name, text, normalColor);
            btn.OnClick = onClick;
            return btn;
        }
        
        public static void Update()
        {
            foreach (var (text, getter) in _labelDict)
            {
                try
                {
                    var newTxt = getter.Invoke();
                    
                    if (text.text != newTxt)
                        text.text = newTxt;
                }
                catch (Exception e)
                {
                    // TODO Eliminate all times this gets called
                    Plugin.PluginLog.LogWarning(e);
                    text.text = "NULL";
                }
            }
            
            foreach (var (toggle, getter) in _toggleDict)
            {
                try
                {
                    var newIsOn = getter.Invoke();
                    
                    if (toggle.isOn != newIsOn)
                        toggle.isOn = newIsOn;
                    
                    if (!toggle.interactable)
                        toggle.interactable = true;
                }
                catch (Exception)
                {
                    toggle.interactable = false;
                }
            }
        }
    }
}