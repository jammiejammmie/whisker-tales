using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Feel;
using WhiskerTales.Save;

namespace WhiskerTales.UI.Screens
{
    public sealed class SettingsScreenController : BackNavScreenBase
    {
        [Header("Controls")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle hapticsToggle;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI bgmValueLabel;
        [SerializeField] private TextMeshProUGUI sfxValueLabel;

        private bool suppressEvents;

        protected override void Awake()
        {
            base.Awake();

            if (bgmSlider != null)
            {
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.onValueChanged.AddListener(OnBgmChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
            }
        }

        public override void Show(bool instant)
        {
            base.Show(instant);
            LoadFromSave();
        }

        private void LoadFromSave()
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

            suppressEvents = true;

            if (bgmSlider != null)
            {
                bgmSlider.value = data.settings.bgmVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = data.settings.sfxVolume;
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.isOn = data.settings.haptics;
            }

            UpdateValueLabels(data.settings.bgmVolume, data.settings.sfxVolume);

            suppressEvents = false;

            ApplyBgmVolume(data.settings.bgmVolume);
            HapticPreference.Enabled = data.settings.haptics;
        }

        private void OnBgmChanged(float value)
        {
            if (suppressEvents == true)
            {
                return;
            }

            ApplyBgmVolume(value);
            PersistAndLog("bgm", v => v.bgmVolume = value, value);
            UpdateValueLabels(value, sfxSlider != null ? sfxSlider.value : 0f);
        }

        private void OnSfxChanged(float value)
        {
            if (suppressEvents == true)
            {
                return;
            }

            PersistAndLog("sfx", v => v.sfxVolume = value, value);
            UpdateValueLabels(bgmSlider != null ? bgmSlider.value : 0f, value);
        }

        private void OnHapticsChanged(bool value)
        {
            if (suppressEvents == true)
            {
                return;
            }

            HapticPreference.Enabled = value;
            PersistAndLog("haptics", v => v.haptics = value, value ? 1f : 0f);
        }

        private void UpdateValueLabels(float bgm, float sfx)
        {
            if (bgmValueLabel != null)
            {
                bgmValueLabel.text = Mathf.RoundToInt(bgm * 100f) + "%";
            }

            if (sfxValueLabel != null)
            {
                sfxValueLabel.text = Mathf.RoundToInt(sfx * 100f) + "%";
            }
        }

        private void PersistAndLog(string field, System.Action<SettingsData> mutator, float value)
        {
            SaveService save = SystemsBootstrap.SaveService;

            if (save == null || mutator == null)
            {
                return;
            }

            WhiskerTales.Save.GameSaveData data = save.Load();

            if (data == null)
            {
                return;
            }

            if (data.settings == null)
            {
                data.settings = new SettingsData();
            }

            mutator.Invoke(data.settings);
            save.Save(data);
            DebugLogger.Info(LogCategory.UI, "[Settings] " + field + " -> " + value);
        }

        private static void ApplyBgmVolume(float value)
        {
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
                bgm.volume = Mathf.Clamp01(value) * 0.55f;
            }
        }
    }
}
