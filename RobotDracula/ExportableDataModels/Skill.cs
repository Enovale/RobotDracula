using System.Collections.Generic;

namespace RobotDracula.ExportableDataModels
{
    public class Skill
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DamageType { get; set; }
        public List<string> CoinDescriptions { get; set; } = new();
        public string Sin { get; set; }
        public LevelDependentStat AttackPower { get; set; }
        public int BaseSkillPower { get; set; }
        public int NumCoins { get; set; }
        public int CoinValue { get; set; }
        public int NumTargets { get; set; }
    }
}