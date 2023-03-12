﻿using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using RobotDracula.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace RobotDracula;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    const string IL2CPP_LIBS_FOLDER =
#if UNHOLLOWER
            "unhollowed"
#else
            "interop"
#endif
        ;
    public string UnhollowedModulesFolder => Path.Combine(Paths.BepInExRootPath, IL2CPP_LIBS_FOLDER);
    
    public static ManualLogSource PluginLog;

    public static UIBase UiBase { get; private set; }

    public Plugin()
    {
        PluginLog = Log;
    }

    public override void Load()
    {
        // Plugin startup logic
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Universe.Init(5f, OnInitialized, UniverseLog, new UniverseLibConfig()
        {
            Force_Unlock_Mouse = true,
            Unhollowed_Modules_Folder = UnhollowedModulesFolder
        });
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
        UiBase = UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, UiUpdate);
        UiBase.SetOnTop();
        var panel = new Panel(UiBase);
    }

    private void UiUpdate()
    {
    }
}