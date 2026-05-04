// Enhanced Idle Reward System with Sleep Mode Integration
// Manages offline rewards, sleep mode ASMR, and battery optimization
// Integrated with Detox Mode philosophy

using UnityEngine;
using System;
using System.Collections;

public class IdleRewardSystem : MonoBehaviour
{
    [Header("Idle Reward Settings")]
    [SerializeField] private float coinsPerHour = 500f;
    [SerializeField] private float starsPerHour = 10f;
    [SerializeField] private float gemsPerHour = 1f;
    [SerializeField] private float maxOfflineHours = 8f; // Maximum 8 hours offline reward
    
    [Header("Sleep Mode Settings")]
    [SerializeField] private bool isSleepModeActive = false;
    [SerializeField] private float batteryDrainPerHour = 2f; // 2% per hour (16% for 8 hours)
    [SerializeField] private float screenBrightness = 0.05f; // 5% brightness
    
    [Header("ASMR Settings")]
    [SerializeField] private AudioClip[] asmrtracks;
    [SerializeField] private AudioSource asrmAudioSource;
    [SerializeField] private float asmrVolume = 0.3f;
    
    private DateTime lastLoginTime;
    private DateTime sleepModeStartTime;
    private bool hasUnclaimedRewards = false;
    
    void Start()
    {
        LoadLastLoginTime();
        CheckForIdleRewards();
    }
    
    void OnApplicationQuit()
    {
        SaveLastLoginTime();
        if (isSleepModeActive)
        {
            SaveSleepModeState();
        }
    }
    
    // ===== IDLE REWARD SYSTEM =====
    
    void LoadLastLoginTime()
    {
        string lastLoginString = PlayerPrefs.GetString("LastLoginTime", string.Empty);
        if (!string.IsNullOrEmpty(lastLoginString))
        {
            lastLoginTime = DateTime.Parse(lastLoginString);
        }
        else
        {
            lastLoginTime = DateTime.Now;
        }
    }
    
    void SaveLastLoginTime()
    {
        PlayerPrefs.SetString("LastLoginTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
    
    void CheckForIdleRewards()
    {
        TimeSpan offlineDuration = DateTime.Now - lastLoginTime;
        
        if (offlineDuration.TotalMinutes > 0)
        {
            // Cap offline duration at maxOfflineHours
            double offlineHours = Math.Min(offlineDuration.TotalHours, maxOfflineHours);
            
            // Calculate rewards
            int coinsEarned = Mathf.FloorToInt((float)offlineHours * coinsPerHour);
            int starsEarned = Mathf.FloorToInt((float)offlineHours * starsPerHour);
            int gemsEarned = Mathf.FloorToInt((float)offlineHours * gemsPerHour);
            
            Debug.Log($"[Idle Rewards] Offline for {offlineDuration.TotalHours:F1} hours");
            Debug.Log($"[Idle Rewards] Earned: {coinsEarned} coins, {starsEarned} stars, {gemsEarned} gems");
            
            // Add rewards to player (integrate with GameManager)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCurrency("coin", coinsEarned);
                GameManager.Instance.AddCurrency("star", starsEarned);
                GameManager.Instance.AddCurrency("gem", gemsEarned);
            }
            
            hasUnclaimedRewards = true;
            ShowIdleRewardPopup(coinsEarned, starsEarned, gemsEarned);
        }
    }
    
    void ShowIdleRewardPopup(int coins, int stars, int gems)
    {
        // TODO: Show UI popup with idle rewards
        Debug.Log($"[UI] Show idle reward popup: {coins}C, {stars}S, {gems}G");
    }
    
    // ===== SLEEP MODE SYSTEM =====
    
    public void ActivateSleepMode()
    {
        isSleepModeActive = true;
        sleepModeStartTime = DateTime.Now;
        
        // Optimize battery
        Screen.brightness = screenBrightness;
        
        // Start ASMR
        PlayASMR();
        
        // Show sleep mode UI
        ShowSleepModeUI();
        
        Debug.Log("[Sleep Mode] Activated - Battery optimization enabled");
    }
    
    public void DeactivateSleepMode()
    {
        isSleepModeActive = false;
        
        // Restore brightness
        Screen.brightness = 1f;
        
        // Stop ASMR
        StopASMR();
        
        // Calculate and award sleep rewards
        CalculateSleepRewards();
        
        Debug.Log("[Sleep Mode] Deactivated");
    }
    
    void PlayASMR()
    {
        if (asrmAudioSource == null || asmrtracks.Length == 0)
            return;
        
        // Select random ASMR track
        AudioClip selectedTrack = asmrtracks[UnityEngine.Random.Range(0, asmrtracks.Length)];
        asrmAudioSource.clip = selectedTrack;
        asrmAudioSource.volume = asmrVolume;
        asrmAudioSource.loop = true;
        asrmAudioSource.Play();
        
        Debug.Log($"[ASMR] Playing: {selectedTrack.name}");
    }
    
    void StopASMR()
    {
        if (asrmAudioSource != null)
        {
            asrmAudioSource.Stop();
        }
    }
    
    void CalculateSleepRewards()
    {
        TimeSpan sleepDuration = DateTime.Now - sleepModeStartTime;
        double sleepHours = sleepDuration.TotalHours;
        
        // Sleep mode rewards (higher than idle)
        int coinsEarned = Mathf.FloorToInt((float)sleepHours * coinsPerHour * 1.5f); // 1.5x multiplier
        int starsEarned = Mathf.FloorToInt((float)sleepHours * starsPerHour * 1.5f);
        int gemsEarned = Mathf.FloorToInt((float)sleepHours * gemsPerHour * 1.5f);
        
        Debug.Log($"[Sleep Mode Rewards] Slept for {sleepHours:F1} hours");
        Debug.Log($"[Sleep Mode Rewards] Earned: {coinsEarned} coins, {starsEarned} stars, {gemsEarned} gems");
        
        // Add rewards
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency("coin", coinsEarned);
            GameManager.Instance.AddCurrency("star", starsEarned);
            GameManager.Instance.AddCurrency("gem", gemsEarned);
        }
        
        ShowSleepRewardPopup(coinsEarned, starsEarned, gemsEarned, sleepHours);
    }
    
    void ShowSleepModeUI()
    {
        // TODO: Show sleep mode UI with message:
        // "고양이가 자고 있어요. 깨우지 않게 폰을 내려놓아 주세요."
        Debug.Log("[UI] Show sleep mode UI");
    }
    
    void ShowSleepRewardPopup(int coins, int stars, int gems, double hours)
    {
        // TODO: Show UI popup with sleep rewards
        Debug.Log($"[UI] Show sleep reward popup: Slept {hours:F1}h, earned {coins}C, {stars}S, {gems}G");
    }
    
    void SaveSleepModeState()
    {
        PlayerPrefs.SetString("SleepModeStartTime", sleepModeStartTime.ToString());
        PlayerPrefs.SetInt("SleepModeActive", 1);
        PlayerPrefs.Save();
    }
    
    // ===== UTILITY METHODS =====
    
    public bool IsSleepModeActive()
    {
        return isSleepModeActive;
    }
    
    public bool HasUnclaimedRewards()
    {
        return hasUnclaimedRewards;
    }
    
    public void ClaimRewards()
    {
        hasUnclaimedRewards = false;
    }
}
