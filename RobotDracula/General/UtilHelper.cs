using HarmonyLib;
using UnityEngine;

namespace RobotDracula.General
{
    public static class UtilHelper
    {
        public static bool MouseButtonUp = false;
        
        [HarmonyPatch(typeof(Util), nameof(Util.GetMouseButtonUp))]
        [HarmonyPrefix]
        public static bool MouseUpWrapper(ref bool __result)
        {
            if (MouseButtonUp)
            {
                __result = true;
                return false;
            }

            return true;
        }
        
        [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
        [HarmonyPrefix]
        public static bool UnityMouseUpWrapper(ref bool __result)
        {
            if (MouseButtonUp)
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static void SelectOneWrapper()
        {
            Plugin.PluginLog.LogWarning("Select one wrapped yayyyyy");
        }
    }
}