namespace RobotDracula.General
{
    public static class GeneralAutomation
    {
        public static float DesiredTimescale = 1f;

        public static void HandleTimescaleUpdate()
        {
            if (GlobalGameHelper.TimeScale != DesiredTimescale)
            {
                GlobalGameHelper.TimeScale = DesiredTimescale;
            }
        }
    }
}