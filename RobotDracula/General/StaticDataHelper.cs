using System;
using System.IO;
using System.Text.Encodings.Web;
using Il2CppInterop.Runtime;
using RobotDracula.ExportableDataModels;
using Steamworks.Data;
using System.Text.Json;
using System.Text.Unicode;
using UnityEngine;

namespace RobotDracula.General
{
    using Il2CppSystem.Collections.Generic;

    public static class StaticDataHelper
    {
        public static StaticDataManager StaticDataManager => Singleton<StaticDataManager>.Instance;

        public static TextDataManager TextDataManager => Singleton<TextDataManager>.Instance;

        public static string GetSaneDamageTypeName(ATK_BEHAVIOUR dmg)
        {
            return dmg switch
            {
                ATK_BEHAVIOUR.HIT => "Blunt",
                ATK_BEHAVIOUR.SLASH => "Slash",
                ATK_BEHAVIOUR.PENETRATE => "Pierce",
                ATK_BEHAVIOUR.NONE => "None",
                ATK_BEHAVIOUR.ERROR => "Very Sus"
            };
        }

        public static string GetSaneSinName(ATTRIBUTE_TYPE sin)
        {
            switch (sin)
            {
                case ATTRIBUTE_TYPE.CRIMSON:
                    return "Wrath";
                case ATTRIBUTE_TYPE.SCARLET:
                    return "Lust";
                case ATTRIBUTE_TYPE.AMBER:
                    return "Sloth";
                case ATTRIBUTE_TYPE.SHAMROCK:
                    return "Gluttony";
                case ATTRIBUTE_TYPE.AZURE:
                    return "Gloom";
                case ATTRIBUTE_TYPE.INDIGO:
                    return "Pride";
                case ATTRIBUTE_TYPE.VIOLET:
                    return "Envy";
                case ATTRIBUTE_TYPE.NEUTRAL:
                    return "Neutral";
                case ATTRIBUTE_TYPE.NONE:
                    return "None";
                default:
                    return "Sussy";
            }
        }

        public static void TryGetEnemyData()
        {
            List<EnemyStaticData> EnemyData = StaticDataManager.EnemyUnitList.list;

            if (EnemyData.Count == 0) Plugin.PluginLog.LogWarning("IL2Cpp back at it again god damn it.");

            foreach (EnemyStaticData d in EnemyData)
            {
                Plugin.PluginLog.LogInfo(d.GetName());
            }
        }

        public static Skill GetParsedSkillDataById(int skillId)
        {
            SkillStaticData skillData = StaticDataManager.SkillList.GetData(skillId);
            Skill parsedSkilldata = new();

            parsedSkilldata.DamageType = GetSaneDamageTypeName(skillData.GetAtkType(3));

            parsedSkilldata.Name = skillData.GetSkillName(3);
            TextData_Skill skill_text = TextDataManager.SkillList.GetData(skillId);
            parsedSkilldata.Description = skill_text.GetDesc(3);

            foreach (TextData_Skill_Coins coin_text in skill_text.GetCoinList(3))
            {
                List<string> coin_descs = coin_text.GetDetail();

                foreach (string desc in coin_descs)
                {
                    parsedSkilldata.CoinDescriptions.Add(desc);
                }
            }

            parsedSkilldata.Sin = GetSaneSinName(skillData.GetAttributeType(3));
            parsedSkilldata.BaseSkillPower = skillData.GetDefaultSkillPower(3);
            LevelDependentStat skillAtkPower = new();
            skillAtkPower.BaseValue = skillData.GetDefaultSkillLevel(3);
            skillAtkPower.GrowthPerLevel = skillData.GetSkillLevelPerUnitLevel(3);
            parsedSkilldata.AttackPower = skillAtkPower;
            parsedSkilldata.NumTargets = skillData.GetTargetNum(3);

            List<SkillCoinData> coins = skillData.GetCoins(3);
            parsedSkilldata.NumCoins = skillData.GetCoinNum(3);
            parsedSkilldata.CoinValue = skillData.GetCoinScale(3);
            if (skillData.GetCoinOperationType(3) == OPERATOR_TYPE.SUB)
                parsedSkilldata.CoinValue = -parsedSkilldata.CoinValue;

            return parsedSkilldata;
        }

        public static void TryGetSinnerData()
        {
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRange(UnicodeRanges.All);
            encoderSettings.AllowCharacter('\u002B');
            JsonSerializerOptions jsonOpts = new() {WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
            int assumedThreadSpinLevel = 3;

            List<PersonalityStaticData> personalityData = StaticDataManager.PersonalityStaticDataList.list;
            System.Collections.Generic.List<Identity> parsedSinnerData = new();

            foreach (PersonalityStaticData p in personalityData)
            {
                Identity sinnerData = new Identity();
                string characterName = StaticDataManager.CharacterList.GetData(p.CharacterId).GetName();
                sinnerData.Sinner = characterName;
                List<int> skillIds = p.GetSkillIds();

                sinnerData.MaxSpeed = p.GetMaxSpeed(3);
                sinnerData.MinSpeed = p.GetMinSpeed(3);

                foreach (ATKResistData resist in p.ResistInfo.atkResistList)
                {
                    sinnerData.Resists.Add(GetSaneDamageTypeName(resist.Type), resist.Value);
                }
            
                LevelDependentStat sinnerHp = new() {BaseValue = p.hp.defaultStat, GrowthPerLevel = p.hp.incrementByLevel};
                LevelDependentStat sinnerDef = new()
                    {BaseValue = p.def.defaultStat, GrowthPerLevel = p.def.incrementByLevel};

                sinnerData.Hp = sinnerHp;
                sinnerData.Defense = sinnerDef;

                List<int> staggerPercents = p.BreakSection.SectionList;

                foreach (int staggerPercent in staggerPercents)
                {
                    sinnerData.StaggerPercentages.Add(staggerPercent);
                }

                sinnerData.Name = p.GetName();
                Plugin.PluginLog.LogInfo($"ID: {p.GetName()}");

                PassiveStaticData charPassive = StaticDataManager.PassiveList.GetData(p.PassiveSetInfo.FirstId);

                PassiveConditionStaticData[] passiveRequirements = charPassive.attributeResonanceCondition._items;

                SinnerPassive passiveData = new SinnerPassive()
                    {Description = charPassive.GetDesc(), Name = charPassive.GetName()};
                Plugin.PluginLog.LogInfo($"Passive: {charPassive.GetName()} | Desc: {charPassive.GetDesc()}");

                Plugin.PluginLog.LogInfo($"Requirements: ");
                foreach (PassiveConditionStaticData requirement in passiveRequirements)
                {
                    passiveData.SinRequired = GetSaneSinName(requirement.AttributeType);
                    passiveData.NumSinRequired = requirement.Value;
                    Plugin.PluginLog.LogInfo($"  {requirement.AttributeType} | {requirement.Value}");
                }

                SinnerPassive supportPassiveData = new SinnerPassive();
                PassiveStaticData charSupportPassive =
                    StaticDataManager.PassiveList.GetData(p.SupporterPassiveSetInfo.FirstId);

                PassiveConditionStaticData[] supportPassiveRequirements =
                    charSupportPassive.attributeResonanceCondition._items;
                supportPassiveData.Name = charSupportPassive.GetName();
                supportPassiveData.Description = charSupportPassive.GetDesc();
                Plugin.PluginLog.LogInfo(
                    $"Support Passive: {charSupportPassive.GetName()} | Desc: {charSupportPassive.GetDesc()}");

                Plugin.PluginLog.LogInfo($"Requirements: ");
                foreach (PassiveConditionStaticData requirement in supportPassiveRequirements)
                {
                    supportPassiveData.SinRequired = GetSaneSinName(requirement.AttributeType);
                    supportPassiveData.NumSinRequired = requirement.Value;
                    Plugin.PluginLog.LogInfo($"  {requirement.AttributeType} | {requirement.Value}");
                }

                sinnerData.Passive = passiveData;
                sinnerData.SupportPassive = supportPassiveData;

                int[] defenseSkills = p.DefenseSkillIDList._items;

                Skill defenseSkill = GetParsedSkillDataById(defenseSkills[0]);

                sinnerData.DefenseSkill = defenseSkill;
            
                sinnerData.Skills = new();

                foreach (int skillId in skillIds)
                {
                    sinnerData.Skills.Add(GetParsedSkillDataById(skillId));
                }
            
                parsedSinnerData.Add(sinnerData);
            
                //Plugin.PluginLog.LogInfo(sinnerData);
                //string json = JsonSerializer.Serialize(sinnerData, jsonOpts);

                //Plugin.PluginLog.LogInfo(json);
            }

            string json = JsonSerializer.Serialize(parsedSinnerData, jsonOpts);
            string fileName = "IdentityData.json";
        
            string filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), fileName);
        
            File.WriteAllText(filePath, json);
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
                        Plugin.PluginLog.LogInfo($"Threadspin level: {dataByLevel.gaksungLevel}");

                        Plugin.PluginLog.LogInfo(
                            $"  Skill Name: {textDat.GetName(dataByLevel.gaksungLevel)} | Skill Desc: {textDat.GetDesc(dataByLevel.gaksungLevel)}");
                        Plugin.PluginLog.LogInfo(
                            $"      atk lvl: {dataByLevel.DefaultSkillLevel} | growth: {dataByLevel.SkillLevelPerUnitLevel} | skill power: {dataByLevel.DefaultSkillPower} | #coins: {dataByLevel.GetCoinNum()} | coin power: {dataByLevel.GetCoinScale()}");
                    }
                }
            }
        }
    }
}