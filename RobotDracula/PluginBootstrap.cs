using System;
using System.IO;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using RobotDracula.Trainer;
using RobotDracula.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

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
            else if (Input.GetKeyDown(KeyCode.Backspace))
                Singleton<StageController>.Instance.EndStageForcely();
            else if (Input.GetKeyDown(KeyCode.Home))
                GlobalGameManager.Instance.LoadScene(SCENE_STATE.Login);
            else if (Input.GetKeyDown(KeyCode.End))
            {
                var str = string.Empty;
                foreach (var resourceLocator in Addressables.ResourceLocators.ToList())
                {
                    foreach (var key in resourceLocator.Keys.ToList())
                    {
                        var keyStr = key.ToString();
                        if (keyStr.Length != "01f09278f4396ee4798db52eac694b36".Length ||
                            keyStr.StartsWith("Assets") || keyStr.Contains("."))
                        {
                            var path = "./Export/" + keyStr;
                            str += path + "\n";

                            try
                            {
                                var dir = Path.GetDirectoryName(path);
                                if (!Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);

                                var res = new List<IResourceLocation>(Addressables
                                    .LoadResourceLocationsAsync(keyStr).WaitForCompletion()
                                    .Cast<IEnumerable<IResourceLocation>>());
                                foreach (var rl in res)
                                {
                                    if (rl.ResourceType == Il2CppType.Of<TextAsset>())
                                        File.WriteAllBytes(path,
                                            Addressables.LoadAsset<TextAsset>(keyStr).WaitForCompletion().bytes);
                                    else if (rl.ResourceType == Il2CppType.Of<Sprite>())
                                        File.WriteAllBytes(path,
                                            ImageConversion.EncodeToPNG(Addressables.LoadAsset<Sprite>(keyStr)
                                                .WaitForCompletion().texture));
                                    else if (rl.ResourceType == Il2CppType.Of<Texture2D>())
                                        File.WriteAllBytes(path,
                                            ImageConversion.EncodeToPNG(Addressables.LoadAsset<Texture2D>(keyStr)
                                                .WaitForCompletion()));
                                }
                            }
                            catch (Exception e)
                            {
                                Plugin.PluginLog.LogError(e);
                            }
                        }
                    }
                }
                
                File.WriteAllText("./paths.txt", str);
            }
        }

        // Run in fixed update because we don't need frame by frame updates
        private void FixedUpdate()
        {
            if (GlobalGameManager.Instance == null || SceneManager.GetActiveScene().name == "LoadingScene")
                return;
            
            if (ReactiveUIEnabled && Plugin.ShowTrainer)
                UiHelper.Update();

            if (TrainerEnabled)
                TrainerManager.Update();
        }
    }
}