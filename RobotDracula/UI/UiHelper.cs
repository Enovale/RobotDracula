using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace RobotDracula.UI
{
    public static class UiHelper
    {
        private static readonly Dictionary<Text, Func<string>> _labelDict = new();

        public static Text CreateLabel(GameObject parent, string name, Func<string> textGetter)
        {
            var startText = string.Empty;

            try
            {
                startText = textGetter.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }

            var text = UIFactory.CreateLabel(parent, name, startText);
            _labelDict.Add(text, textGetter);

            return text;
        }

        public static void Update()
        {
            foreach (var (text, getter) in _labelDict)
            {
                try
                {
                    text.text = getter.Invoke();
                }
                catch (Exception)
                {
                    text.text = "NULL";
                }
            }
        }
    }
}