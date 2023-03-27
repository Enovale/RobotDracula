using System;
using Il2CppInterop.Runtime;

namespace RobotDracula.General;

using Il2CppSystem.Collections.Generic;

public static class StaticDataHelper
{
    public static StaticDataManager StaticDataManager => Singleton<StaticDataManager>.Instance;

    public static TextDataManager TextDataManager => Singleton<TextDataManager>.Instance;

    public static void TryGetEnemyData()
    {
        List<EnemyStaticData> EnemyData = StaticDataManager.EnemyUnitList.list;

        if (EnemyData.Count == 0) Plugin.PluginLog.LogWarning("IL2Cpp back at it again god damn it.");

        foreach (EnemyStaticData d in EnemyData)
        {
            Plugin.PluginLog.LogInfo(d.GetName());
        }
    }

    public static void TryGetAbnoData()
    {
        List<AbnormalityStaticData> AbnoData = StaticDataManager.AbnormalityUnitList.list;

        foreach (AbnormalityStaticData d in AbnoData)
        {
            TextData_AbnormalityContents test = TextDataManager.AbnormalityContentData.GetData(d.ID);
            Plugin.PluginLog.LogInfo(
                $"NAME: {test.name} | ID: {d.ID} | DEF: {d.def.defaultStat} | HP: {d.hp.defaultStat}");
            ResistInfo resists = d.ResistInfo;
            
            Plugin.PluginLog.LogInfo("Resistances:");
            foreach (ATKResistData atkResistData in resists.GetAtkResistList())
            {
                Plugin.PluginLog.LogInfo($"{atkResistData.type} | {atkResistData.value}");
            }

            foreach (AttributeResistData attrResistData in resists.GetAttributeResistList())
            {
                Plugin.PluginLog.LogInfo($"{attrResistData.type} | {attrResistData.value}");
            }

            List<int> partIds = d.AbnormalityPartIdList;
            List<int> passiveIds = d.passiveSet.PassiveIdList;
            Plugin.PluginLog.LogInfo("Passives:");
            foreach (int passiveId in passiveIds)
            {
                TextData_Passive passiveTextData = TextDataManager.PassiveList.GetData(passiveId);
                Plugin.PluginLog.LogInfo($"Name: {passiveTextData.name}");
                Plugin.PluginLog.LogInfo($"  Summary: {passiveTextData.summary}");
                Plugin.PluginLog.LogInfo($"  Desc: {passiveTextData.desc}");
            }

            Plugin.PluginLog.LogInfo("Parts:");
            foreach (int partId in partIds)
            {
                AbnormalityPartStaticData partData = StaticDataManager.AbnormalityPartList.GetData(partId);
                var name = TextDataManager.EnemyList.GetData(partId).GetName();
                Plugin.PluginLog.LogInfo($"PartName: {name} | HP: {partData.hp.defaultStat}");
                Plugin.PluginLog.LogInfo($"Resists?");
                var resistData = partData.ResistInfo;
                foreach (ATKResistData atkResistData in resistData.GetAtkResistList())
                {
                    Plugin.PluginLog.LogInfo($"{atkResistData.type} | {atkResistData.value}");
                }

                foreach (AttributeResistData attrResistData in resistData.GetAttributeResistList())
                {
                    Plugin.PluginLog.LogInfo($"{attrResistData.type} | {attrResistData.value}");
                }
            }

            List<int> skills = d.GetSkillIds();
            foreach (int s in skills)
            {
                SkillStaticData dat = StaticDataManager.SkillList.GetData(s);
                TextData_Skill textDat = TextDataManager.SkillList.GetData(s);
                foreach (SkillDataByLevel dataByLevel in dat.skillData)
                {
                    Plugin.PluginLog.LogInfo(
                        $"  Skill Name: {textDat.GetName(dataByLevel.gaksungLevel)} | Skill Desc: {textDat.GetDesc(dataByLevel.gaksungLevel)}");
                    Plugin.PluginLog.LogInfo(
                        $"      atk lvl:  | skill power: {dataByLevel.DefaultSkillPower} | #coins: {dataByLevel.GetCoinNum()} | coin power: {dataByLevel.GetCoinScale()}");
                }
            }
        }
    }
}