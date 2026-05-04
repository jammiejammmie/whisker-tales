// This script simulates the initialization of core game systems in a Unity scene.
// It would typically be attached to a GameObject in the first scene of the game.

using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        // Initialize I18nManager
        if (I18nManager.Instance == null)
        {
            GameObject i18nObject = new GameObject("I18nManager");
            i18nObject.AddComponent<I18nManager>();
        }

        // Initialize IdleRewardSystem
        if (FindObjectOfType<IdleRewardSystem>() == null)
        {
            GameObject idleRewardObject = new GameObject("IdleRewardSystem");
            idleRewardObject.AddComponent<IdleRewardSystem>();
        }

        // Set initial language (e.g., based on device language or user preference)
        I18nManager.Instance.SetLanguage(Application.systemLanguage);

        Debug.Log($"Game Title: {I18nManager.Instance.GetLocalizedText("game_title")}");
        Debug.Log($"Play Button Text: {I18nManager.Instance.GetLocalizedText("play_button")}");
    }

    void Start()
    {
        // Further game initialization logic can go here
        Debug.Log("Game initialization complete.");
    }
}
