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
       
       if(EnemyData.Count == 0)Plugin.PluginLog.LogWarning("IL2Cpp back at it again god damn it.");
       
       foreach(EnemyStaticData d in EnemyData)
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
           Plugin.PluginLog.LogInfo($"NAME: {test.name} | ID: {d.ID}");
           ResistInfo resists = d.ResistInfo;
           foreach (ATKResistData atkResistData in resists.atkResistList)
           {
               
           }        
           List<int> skills = d.GetSkillIds();
           foreach (int s in skills)
           {
               SkillStaticData dat = StaticDataManager.SkillList.GetData(s);
               TextData_Skill textDat = TextDataManager.SkillList.GetData(s);
               foreach (SkillDataByLevel dataByLevel in dat.skillData)
               {
                   Plugin.PluginLog.LogInfo($"  Skill Name: {textDat.GetName(dataByLevel.gaksungLevel)} | Skill Desc: {textDat.GetDesc(dataByLevel.gaksungLevel)}");
                   Plugin.PluginLog.LogInfo($"      atk lvl: {dataByLevel.DefaultAttackLevel} | skill power: {dataByLevel.DefaultSkillPower} | #coins: {dataByLevel.GetCoinNum()} | coin power: {dataByLevel.GetCoinScale()}");
               }
           }
       }
   }
}