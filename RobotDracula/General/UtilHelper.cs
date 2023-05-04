using HarmonyLib;

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
                MouseButtonUp = false;
                __result = true;
                return false;
            }

            return true;
        }
    }
}