using System.Collections.Generic;

namespace RobotDracula.ExportableDataModels
{
    public class Abnormality
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public LevelDependentStat Defense { get; set; }
        public LevelDependentStat Hp { get; set; }
        public List<int> StaggerPercentages { get; set; } = new();
        public int MaxSpeed { get; set; }
        public int MinSpeed { get; set; }
        public Skill DefenseSkill { get; set; }
        public Dictionary<string, float> Resists { get; set; } = new();
        public List<AbnoPart> Parts { get; set; } = new();
        public List<Skill> Skills { get; set; } = new();
        public List<AbnormalityPassive> Passives { get; set; } = new();
        public List<int> Phases { get; set; } = new();
    }
}