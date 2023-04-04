namespace RobotDracula.ExportableDataModels;

using System.Collections.Generic;

public class Identity
{
    public string Name { get; set; }
    public string Sinner { get; set; }
    public LevelDependentStat Defense { get; set; }
    public LevelDependentStat Hp { get; set; }
    public List<int> StaggerPercentages { get; set; } = new();
    public int MaxSpeed { get; set; }
    public int MinSpeed { get; set; }
    public Skill DefenseSkill { get; set; }
    public Dictionary<string, float> Resists { get; set; } = new();
    public List<Skill> Skills { get; set; } = new();
    public SinnerPassive Passive { get; set; }
    public SinnerPassive SupportPassive { get; set; }
}