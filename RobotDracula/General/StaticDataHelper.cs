namespace RobotDracula.General
{
    public static class StaticDataHelper
    {
        public static StaticDataManager StaticDataManager => Singleton<StaticDataManager>.Instance;

        public static TextDataManager TextDataManager => Singleton<TextDataManager>.Instance;
    }
}