using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.Save
{
    public sealed class SaveService
    {
        private const int CurrentVersion = 1;
        private const string SaveFileName = "whisker_save.json";
        private const string BackupFileName = "whisker_save_backup.json";

        private readonly string savePath;
        private readonly string backupPath;

        public SaveService()
        {
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            backupPath = Path.Combine(Application.persistentDataPath, BackupFileName);
        }

        public GameSaveData Load()
        {
            GameSaveData data = TryLoadFromPath(savePath);

            if (data == null)
            {
                data = TryLoadFromPath(backupPath);
            }

            if (data == null)
            {
                data = GameSaveData.CreateDefault();
                Save(data);
            }

            data = Migrate(data);
            return data;
        }

        public bool Save(GameSaveData data)
        {
            if (data == null)
            {
                DebugLogger.Warning(LogCategory.Save, "SaveService.Save ignored null data.");
                return false;
            }

            try
            {
                data.version = CurrentVersion;
                data.checksum = string.Empty;
                string raw = JsonUtility.ToJson(data, true);
                data.checksum = ComputeChecksum(raw);
                string finalJson = JsonUtility.ToJson(data, true);

                if (File.Exists(savePath) == true)
                {
                    File.Copy(savePath, backupPath, true);
                }

                File.WriteAllText(savePath, finalJson, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Error(LogCategory.Save, $"Save failed: {ex.Message}");
                return false;
            }
        }

        private GameSaveData TryLoadFromPath(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                if (data == null)
                {
                    return null;
                }

                string expected = data.checksum;
                data.checksum = string.Empty;
                string raw = JsonUtility.ToJson(data, true);
                data.checksum = expected;

                if (string.IsNullOrEmpty(expected) == false && expected != ComputeChecksum(raw))
                {
                    DebugLogger.Warning(LogCategory.Save, "Save checksum mismatch.");
                    return null;
                }

                return data;
            }
            catch (Exception ex)
            {
                DebugLogger.Warning(LogCategory.Save, $"Load failed: {ex.Message}");
                return null;
            }
        }

        private GameSaveData Migrate(GameSaveData data)
        {
            if (data == null)
            {
                return GameSaveData.CreateDefault();
            }

            if (data.version < CurrentVersion)
            {
                data.version = CurrentVersion;
                Save(data);
            }

            return data;
        }

        private string ComputeChecksum(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                return Convert.ToBase64String(bytes);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void DebugResetSave()
        {
            if (File.Exists(savePath) == true)
            {
                File.Delete(savePath);
            }

            if (File.Exists(backupPath) == true)
            {
                File.Delete(backupPath);
            }

            Save(GameSaveData.CreateDefault());
        }
#endif
    }

    [Serializable]
    public sealed class GameSaveData
    {
        public int version;
        public string checksum;
        public PlayerProgress progress;
        public CurrencyData currency;
        public CatBondData cats;
        public CafeProgressData cafe;
        public SettingsData settings;
        public SleepData sleep;
        public ReferralData referral;

        public static GameSaveData CreateDefault()
        {
            return new GameSaveData
            {
                version = 1,
                checksum = string.Empty,
                progress = new PlayerProgress(),
                currency = new CurrencyData(),
                cats = new CatBondData(),
                cafe = new CafeProgressData(),
                settings = new SettingsData(),
                sleep = new SleepData(),
                referral = new ReferralData()
            };
        }
    }

    [Serializable] public sealed class PlayerProgress { public int level = 1; public int stars; public int restoredStage; }
    [Serializable] public sealed class CurrencyData { public int coins; public int hearts = 5; public int nyangiHearts; public int dailyNyangiHearts; public long lastHeartTickUtc; }
    [Serializable] public sealed class CatBondData { public int[] affinity = new int[5]; public int[] levels = new int[5]; }
    [Serializable] public sealed class CafeProgressData { public int zone1; public int zone2; public int zone3; }
    [Serializable] public sealed class SettingsData { public float bgmVolume = 0.7f; public float sfxVolume = 0.8f; public bool haptics = true; public string language = "ko"; }
    [Serializable] public sealed class SleepData { public long enteredUtc; public long exitedUtc; public int rewardPending; }
    [Serializable] public sealed class ReferralData { public string code; public string invitedBy; public int rewardsClaimed; }
}
