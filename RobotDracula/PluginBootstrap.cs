using System;
using RobotDracula.Trainer;
using RobotDracula.UI;
using UnityEngine;

namespace RobotDracula
{
    public class PluginBootstrap : MonoBehaviour
    {
        public static bool TrainerEnabled = true;
        public static bool ReactiveUIEnabled = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                Plugin.ShowTrainer = !Plugin.ShowTrainer;
        }

        // Run in fixed update because we don't need frame by frame updates
        private void FixedUpdate()
        {
            if (ReactiveUIEnabled && Plugin.ShowTrainer)
                UiHelper.Update();
            
            if(TrainerEnabled)
                TrainerManager.Update();
        }
    }
}