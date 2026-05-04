using System.Collections.Generic;
using UnityEngine;

public class NyangClubManager : MonoBehaviour
{
    public string clubName;
    public int maxMembers = 30;
    public List<string> members = new List<string>();
    
    // Core: Help system (Hearts)
    public void RequestHeart(string userId) {
        Debug.Log($"User {userId} requested a heart from the club.");
        // Notify other club members via server
    }

    public void GiveHeart(string donorId, string recipientId) {
        Debug.Log($"User {donorId} gave a heart to {recipientId}.");
        // Reward donor with Club Coins
    }

    // Competitive: Club Tournament
    public void UpdateClubScore(int levelScore) {
        Debug.Log($"Adding {levelScore} to Club's weekly tournament score.");
    }

    // Social: Chat placeholder
    public void SendChatMessage(string userId, string message) {
        Debug.Log($"[Club Chat] {userId}: {message}");
    }
}
