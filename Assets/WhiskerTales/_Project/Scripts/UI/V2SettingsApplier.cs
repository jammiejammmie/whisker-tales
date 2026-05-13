using System.Reflection;
using UnityEngine;
using WhiskerTales.Core;
using WhiskerTales.Feel;
using WhiskerTales.Save;

namespace WhiskerTales.UI
{
    // Loads persisted SettingsData on boot and applies to runtime singletons
    // (BGM source volume via reflection, HapticPreference flag).
    public static class V2SettingsApplier
    {
        public static void ApplyFromSave()
        {
            SaveService save = SystemsBootstrap.SaveService;

            if (save == null)
            {
                return;
            }

            WhiskerTales.Save.GameSaveData data = save.Load();

            if (data == null || data.settings == null)
            {
                return;
            }

            HapticPreference.Enabled = data.settings.haptics;

            AudioService service = AudioService.Instance;

            if (service == null)
            {
                return;
            }

            FieldInfo field = typeof(AudioService).GetField("bgmSource", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return;
            }

            AudioSource bgm = field.GetValue(service) as AudioSource;

            if (bgm != null)
            {
                bgm.volume = Mathf.Clamp01(data.settings.bgmVolume) * 0.55f;
            }

            DebugLogger.Info(LogCategory.UI, "[V2SettingsApplier] applied. bgm=" + data.settings.bgmVolume + " sfx=" + data.settings.sfxVolume + " haptics=" + data.settings.haptics);
        }
    }
}
