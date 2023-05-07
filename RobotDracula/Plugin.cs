using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using RobotDracula.General;
using RobotDracula.UI;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;
using Object = UnityEngine.Object;

namespace RobotDracula
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private const string IL2CPP_LIBS_FOLDER = "interop";

        private static string _jsonConfigFolder;
        
        private static string _egoGiftListPath = "ego_gifts.json";
        
        private static string _personalityListPath = "personalities.json";

        public string UnhollowedModulesFolder => Path.Combine(Paths.BepInExRootPath, IL2CPP_LIBS_FOLDER);

        public static ManualLogSource PluginLog;

        public static UIBase UiBase { get; private set; }
    
        public static GameObject UIRoot => UiBase?.RootObject;
    
        public static bool ShowTrainer
        {
            get => UiBase is { Enabled: true };
            set
            {
                if (UiBase == null || !UIRoot || UiBase.Enabled == value)
                    return;

                UniversalUI.SetUIActive(MyPluginInfo.PLUGIN_GUID, value);
            }
        }

        public static ConfigEntry<bool> BattleAutomationEnabled;
        
        public static ConfigEntry<bool> EventAutomationEnabled;
        
        public static ConfigEntry<bool> DungeonAutomationEnabled;

        public static Dictionary<int, int> PersonalityPriority;
        
        public static List<int> EgoGiftPriority;

        public Plugin()
        {
            PluginLog = Log;
        }

        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loading...");

            _jsonConfigFolder = Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath), "RobotDracula");
            
            LoadJsonConfig();

            BattleAutomationEnabled = Config.Bind("Automation", nameof(BattleAutomationEnabled), false);
            EventAutomationEnabled = Config.Bind("Automation", nameof(EventAutomationEnabled), false);
            DungeonAutomationEnabled = Config.Bind("Automation", nameof(DungeonAutomationEnabled), false);

            //var method = AccessTools.Method(typeof(Util), nameof(Util.SelectOne),
            //    new[] { typeof(Il2CppSystem.Collections.Generic.List<>) }, new []{Type.});
            //var baseMethod = typeof(Util).GetMethod(nameof(Util.SelectOne), 1, ReflectionUtility.FLAGS, null, new [] { typeof(Il2CppSystem.Collections.Generic.List<>) }, null);
            //var method = baseMethod?.MakeGenericMethod(typeof(int));
            //PluginLog.LogWarning(baseMethod);
            //PluginLog.LogWarning(method);

            Harmony.CreateAndPatchAll(typeof(UtilHelper));//.Patch(method, new HarmonyMethod(typeof(UtilHelper), nameof(UtilHelper.SelectOneWrapper)));
            
            Universe.Init(5f, OnInitialized, UniverseLog, new UniverseLibConfig()
            {
                Disable_EventSystem_Override = true,
                Force_Unlock_Mouse = true,
                Unhollowed_Modules_Folder = UnhollowedModulesFolder
            });
            
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void LoadJsonConfig()
        {
            if (!Directory.Exists(_jsonConfigFolder))
                Directory.CreateDirectory(_jsonConfigFolder);

            var egoPath = Path.Combine(_jsonConfigFolder, _egoGiftListPath);
            try
            {
                if (File.Exists(egoPath))
                {
                    using var stream = File.OpenRead(egoPath);
                    EgoGiftPriority = JsonSerializer.Deserialize<List<int>>(stream);
                }
                else
                {
                    File.WriteAllText(egoPath, "[]");
                }
            }
            catch (Exception e)
            {
                PluginLog.LogError(e);
            }
            finally
            {
                EgoGiftPriority ??= new();
            }
            
            var personalityPath = Path.Combine(_jsonConfigFolder, _personalityListPath);
            try
            {
                if (File.Exists(personalityPath))
                {
                    using var stream = File.OpenRead(personalityPath);
                    PersonalityPriority = JsonSerializer.Deserialize<Dictionary<int, int>>(stream);
                }
                else
                {
                    File.WriteAllText(personalityPath, "{}");
                }
            }
            catch (Exception e)
            {
                PluginLog.LogError(e);
            }
            finally
            {
                PersonalityPriority ??= new();
            }
        }

        private void UniverseLog(string message, LogType type)
        {
            PluginLog.Log(type switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Assert => LogLevel.Debug,
                LogType.Warning => LogLevel.Warning,
                LogType.Log => LogLevel.Message,
                LogType.Exception => LogLevel.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            }, message);
        }

        private void OnInitialized()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PluginBootstrap>();
        
            var bootstrap = new GameObject("draculaBootstrap");
            bootstrap.AddComponent<PluginBootstrap>();
            Object.DontDestroyOnLoad(bootstrap);
        
            UiBase = UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, UiUpdate);
            UiBase.SetOnTop();
            var panel = new Panel(UiBase);
        }

        private void UiUpdate()
        {
        }
    }
}