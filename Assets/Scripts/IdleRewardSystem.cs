using UnityEngine;
using System;
using System.Collections;

public class IdleRewardSystem : MonoBehaviour
{
    [Header("Idle Reward Settings")]
    [SerializeField] private float coinsPerHour = 500f;
    [SerializeField] private float starsPerHour = 10f;
    [SerializeField] private float gemsPerHour = 1f;
    [SerializeField] private float maxOfflineHours = 8f;
    
    [Header("Sleep Mode Settings")]
    [SerializeField] private bool isSleepModeActive = false;
    [SerializeField] private float batteryDrainPerHour = 2f;
    [SerializeField] private float screenBrightness = 0.05f;
    
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
        if (isSleepModeActive) SaveSleepModeState();
    }
    
    void LoadLastLoginTime()
    {
        string lastLoginString = PlayerPrefs.GetString("LastLoginTime", string.Empty);
        lastLoginTime = !string.IsNullOrEmpty(lastLoginString) ? DateTime.Parse(lastLoginString) : DateTime.Now;
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
            double offlineHours = Math.Min(offlineDuration.TotalHours, maxOfflineHours);
            int coinsEarned = Mathf.FloorToInt((float)offlineHours * coinsPerHour);
            int starsEarned = Mathf.FloorToInt((float)offlineHours * starsPerHour);
            int gemsEarned = Mathf.FloorToInt((float)offlineHours * gemsPerHour);
            
            if (WhiskerTales.Core.GameManager.Instance != null)
            {
                WhiskerTales.Core.GameManager.Instance.AddCoins(coinsEarned);
                WhiskerTales.Core.GameManager.Instance.AddStars(starsEarned);
                WhiskerTales.Core.GameManager.Instance.AddGems(gemsEarned);
            }
            hasUnclaimedRewards = true;
            ShowIdleRewardPopup(coinsEarned, starsEarned, gemsEarned);
        }
    }
    
    void ShowIdleRewardPopup(int coins, int stars, int gems)
    {
        Debug.Log($"[UI] Show idle reward popup: {coins}C, {stars}S, {gems}G");
    }
    
    public void ActivateSleepMode()
    {
        isSleepModeActive = true;
        sleepModeStartTime = DateTime.Now;
        Screen.brightness = screenBrightness;
        PlayASMR();
        Debug.Log("[Sleep Mode] Activated");
    }
    
    public void DeactivateSleepMode()
    {
        isSleepModeActive = false;
        Screen.brightness = 1f;
        StopASMR();
        CalculateSleepRewards();
        Debug.Log("[Sleep Mode] Deactivated");
    }
    
    void PlayASMR()
    {
        if (asrmAudioSource == null || asmrtracks.Length == 0) return;
        AudioClip selectedTrack = asmrtracks[UnityEngine.Random.Range(0, asmrtracks.Length)];
        asrmAudioSource.clip = selectedTrack;
        asrmAudioSource.volume = asmrVolume;
        asrmAudioSource.loop = true;
        asrmAudioSource.Play();
    }
    
    void StopASMR() { if (asrmAudioSource != null) asrmAudioSource.Stop(); }
    
    void CalculateSleepRewards()
    {
        TimeSpan sleepDuration = DateTime.Now - sleepModeStartTime;
        double sleepHours = sleepDuration.TotalHours;
        int coinsEarned = Mathf.FloorToInt((float)sleepHours * coinsPerHour * 1.5f);
        int starsEarned = Mathf.FloorToInt((float)sleepHours * starsPerHour * 1.5f);
        int gemsEarned = Mathf.FloorToInt((float)sleepHours * gemsPerHour * 1.5f);
        
        if (WhiskerTales.Core.GameManager.Instance != null)
        {
            WhiskerTales.Core.GameManager.Instance.AddCoins(coinsEarned);
            WhiskerTales.Core.GameManager.Instance.AddStars(starsEarned);
            WhiskerTales.Core.GameManager.Instance.AddGems(gemsEarned);
        }
        Debug.Log($"[UI] Sleep reward: {sleepHours:F1}h, {coinsEarned}C, {starsEarned}S, {gemsEarned}G");
    }
    
    void SaveSleepModeState()
    {
        PlayerPrefs.SetString("SleepModeStartTime", sleepModeStartTime.ToString());
        PlayerPrefs.SetInt("SleepModeActive", 1);
        PlayerPrefs.Save();
    }
    
    public bool IsSleepModeActive() { return isSleepModeActive; }
    public bool HasUnclaimedRewards() { return hasUnclaimedRewards; }
    public void ClaimRewards() { hasUnclaimedRewards = false; }
}
