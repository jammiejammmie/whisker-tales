// This script will manage the internationalization (i18n) system.
// It will load translation data and provide methods to retrieve localized strings.

using System.Collections.Generic;
using UnityEngine;

public class I18nManager : MonoBehaviour
{
    public static I18nManager Instance { get; private set; }

    private Dictionary<string, Dictionary<string, string>> translations;
    public SystemLanguage currentLanguage = SystemLanguage.English;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        LoadTranslations();
    }

    void LoadTranslations()
    {
        // In a real scenario, this would load from a JSON or CSV file.
        // For now, we'll use dummy data.
        translations = new Dictionary<string, Dictionary<string, string>>();

        // English translations
        translations["en"] = new Dictionary<string, string>
        {
            {"game_title", "Whisker Tales"},
            {"play_button", "Play"},
            {"settings_button", "Settings"},
            {"level_complete", "Level Complete!"},
            {"collect_cat", "Collect Cat!"}
        };

        // Korean translations
        translations["ko"] = new Dictionary<string, string>
        {
            {"game_title", "위스커 테일즈"},
            {"play_button", "시작"},
            {"settings_button", "설정"},
            {"level_complete", "레벨 완료!"},
            {"collect_cat", "고양이 수집!"}
        };

        // Hindi translations (example)
        translations["hi"] = new Dictionary<string, string>
        {
            {"game_title", "व्हिस्कर टेल्स"},
            {"play_button", "खेलें"},
            {"settings_button", "सेटिंग्स"},
            {"level_complete", "स्तर पूरा हुआ!"},
            {"collect_cat", "बिल्ली इकट्ठा करें!"}
        };

        // Arabic translations (example)
        translations["ar"] = new Dictionary<string, string>
        {
            {"game_title", "حكايات الهمس"},
            {"play_button", "العب"},
            {"settings_button", "الإعدادات"},
            {"level_complete", "اكتمل المستوى!"},
            {"collect_cat", "اجمع القط!"}
        };

        Debug.Log("Translations loaded.");
    }

    public string GetLocalizedText(string key)
    {
        string langCode = GetLanguageCode(currentLanguage);
        if (translations.ContainsKey(langCode) && translations[langCode].ContainsKey(key))
        {
            return translations[langCode][key];
        }
        Debug.LogWarning($"Missing translation for key: {key} in language: {langCode}");
        return key; // Fallback to key if translation is missing
    }

    private string GetLanguageCode(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Korean: return "ko";
            case SystemLanguage.Hindi: return "hi";
            case SystemLanguage.Arabic: return "ar";
            default: return "en"; // Default to English
        }
    }

    public void SetLanguage(SystemLanguage newLanguage)
    {
        currentLanguage = newLanguage;
        Debug.Log($"Language set to: {currentLanguage}");
        // Optionally, trigger UI update event here
    }
}
