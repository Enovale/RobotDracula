using System;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RobotDracula
{
    public class TrainerComponent : MonoBehaviour
    {
        private static ManualLogSource _log => Plugin.PluginLog;
        
        public TrainerComponent(IntPtr ptr) : base(ptr)
        {
            _log.LogInfo($"Trainer for {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        public void Awake()
        {
            _log.LogMessage("TrainerComponent Awake() Fired!");
        }

        public void Start()
        {
            _log.LogMessage("TrainerComponent Start() Fired!");
        }

        public void OnEnable()
        {
            _log.LogMessage("TrainerComponent OnEnable() Fired!");
        }

        public void Update()
        {
            _log.LogMessage("TrainerComponent Update() Fired!");
        }

        private void OnDestroy()
        {
            _log.LogError("We got destroyed :(");
        }
    }
}