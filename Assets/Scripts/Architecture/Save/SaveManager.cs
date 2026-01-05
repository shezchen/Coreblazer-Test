using System;
using System.IO;
using Architecture.GameSound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Newtonsoft.Json;

namespace Architecture
{
    public class SaveManager:ManagerNeedInitializeBase
    {
        [Inject] private IAudioService _audioService;
        
        private const string SettingsSaveName = "GameSettings.sav";
        private readonly string _settingsSavePath = Path.Combine(Application.persistentDataPath, SettingsSaveName);

        public bool IsFirstLaunch;
        public GameSettings CurrentSettingsSave;
        public GameSave CurrentGameSave;

        /// <summary>
        /// 初始化，会设置IsFirstLaunch和CurrentSettingsSave
        /// </summary>
        public override async UniTask Init()
        {
            await base.Init();
            IsFirstLaunch = !File.Exists(_settingsSavePath);
            if (IsFirstLaunch)
            {
                CurrentSettingsSave = new GameSettings();
                SaveSettings();
            }
            else
            {
                LoadSettings();
                
                _audioService.SetBgmVolume(CurrentSettingsSave.bgmVolume/100f);
                _audioService.SetSfxVolume(CurrentSettingsSave.sfxVolume/100f);

                var resParts = CurrentSettingsSave.gameResolution.ToString().Replace("Res_", "").Split('x');
                if (resParts.Length == 2 && int.TryParse(resParts[0], out int width) && int.TryParse(resParts[1], out int height))
                {
                    Screen.SetResolution(width, height,
                        CurrentSettingsSave.gameWindow == GameWindow.FullScreenWindow
                            ? FullScreenMode.FullScreenWindow
                            : FullScreenMode.Windowed);
                }
            }
        }

        private void LoadSettings()
        {
            string json = File.ReadAllText(_settingsSavePath);
            CurrentSettingsSave = JsonConvert.DeserializeObject<GameSettings>(json);
            Debug.Log("设置数据已从: " + _settingsSavePath + " 加载");
        }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(CurrentSettingsSave, Formatting.Indented);
            File.WriteAllText(_settingsSavePath, json);
            Debug.Log("设置数据已保存至: " + _settingsSavePath);
        }
        
        public string GetDefaultSavePath(int saveSlot)
        {
            return Path.Combine(Application.persistentDataPath, "SaveSlot" + saveSlot + ".sav");
        }
        
        public void LoadGame(string savePath)
        {
            if (!File.Exists(savePath))
            {
                Debug.LogError("存档文件不存在: " + savePath);
                return;
            }

            string json = File.ReadAllText(savePath);
            CurrentGameSave = JsonConvert.DeserializeObject<GameSave>(json);
            Debug.Log("游戏存档已从: " + savePath + " 加载");
        }
        
        public void SaveGame(string savePath)
        {
            CurrentGameSave.LastSaveTime = DateTime.Now;
            string json = JsonConvert.SerializeObject(CurrentGameSave, Formatting.Indented);
            File.WriteAllText(savePath, json);
            Debug.Log("游戏存档已保存至: " + savePath);
        }
    }
}