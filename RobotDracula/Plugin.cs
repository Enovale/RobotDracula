using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
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

        public Plugin()
        {
            PluginLog = Log;
        }

        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loading...");

            BattleAutomationEnabled = Config.Bind("Automation", nameof(BattleAutomationEnabled), false);
            EventAutomationEnabled = Config.Bind("Automation", nameof(EventAutomationEnabled), false);
            DungeonAutomationEnabled = Config.Bind("Automation", nameof(DungeonAutomationEnabled), false);

            Harmony.CreateAndPatchAll(typeof(UtilHelper));
            
            Universe.Init(5f, OnInitialized, UniverseLog, new UniverseLibConfig()
            {
                Disable_EventSystem_Override = true,
                Force_Unlock_Mouse = true,
                Unhollowed_Modules_Folder = UnhollowedModulesFolder
            });
            
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
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