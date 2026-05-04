// Nyangstagram Manager - In-game Social SNS System
// Allows players to share cat photos, cafe decorations, and interact with other players
// Core end-game content for long-term engagement

using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class NyangstagramPost
{
    public string postId;
    public string userId;
    public string userName;
    public string postType; // "cat_photo", "cafe_decoration", "achievement"
    public string caption;
    public string imageUrl;
    public DateTime postTime;
    public int likes;
    public List<string> comments;
    public bool isLikedByPlayer;
}

[System.Serializable]
public class NyangstagramUser
{
    public string userId;
    public string userName;
    public string profileImageUrl;
    public int followers;
    public int following;
    public List<string> posts;
    public bool isFollowedByPlayer;
}

public class NyangstagramManager : MonoBehaviour
{
    public static NyangstagramManager Instance { get; private set; }
    
    [Header("Nyangstagram Settings")]
    [SerializeField] private int maxPostsPerDay = 5;
    [SerializeField] private int maxFollowsPerPlayer = 500;
    [SerializeField] private int likeRewardCoins = 10;
    [SerializeField] private int commentRewardCoins = 5;
    
    private List<NyangstagramPost> allPosts = new List<NyangstagramPost>();
    private List<NyangstagramUser> allUsers = new List<NyangstagramUser>();
    private string currentPlayerId;
    private int postsCreatedToday = 0;
    private DateTime lastPostDate;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        LoadNyangstagramData();
        ResetDailyPostCount();
    }
    
    // ===== POST MANAGEMENT =====
    
    public bool CreatePost(string postType, string caption, string imageUrl)
    {
        // Check daily post limit
        if (postsCreatedToday >= maxPostsPerDay)
        {
            Debug.LogWarning("[Nyangstagram] Daily post limit reached!");
            return false;
        }
        
        NyangstagramPost newPost = new NyangstagramPost
        {
            postId = System.Guid.NewGuid().ToString(),
            userId = currentPlayerId,
            userName = GetPlayerName(),
            postType = postType,
            caption = caption,
            imageUrl = imageUrl,
            postTime = DateTime.Now,
            likes = 0,
            comments = new List<string>(),
            isLikedByPlayer = false
        };
        
        allPosts.Add(newPost);
        postsCreatedToday++;
        
        Debug.Log($"[Nyangstagram] Post created: {postType} - {caption}");
        
        // Reward for posting
        RewardForPosting(postType);
        
        return true;
    }
    
    public List<NyangstagramPost> GetFeed(int pageNumber = 0, int postsPerPage = 10)
    {
        // Return paginated feed (newest first)
        int startIndex = pageNumber * postsPerPage;
        int endIndex = Mathf.Min(startIndex + postsPerPage, allPosts.Count);
        
        List<NyangstagramPost> feed = new List<NyangstagramPost>();
        for (int i = allPosts.Count - 1; i >= 0 && feed.Count < postsPerPage; i--)
        {
            feed.Add(allPosts[i]);
        }
        
        return feed;
    }
    
    public void DeletePost(string postId)
    {
        NyangstagramPost postToDelete = allPosts.Find(p => p.postId == postId);
        if (postToDelete != null && postToDelete.userId == currentPlayerId)
        {
            allPosts.Remove(postToDelete);
            Debug.Log("[Nyangstagram] Post deleted");
        }
    }
    
    // ===== INTERACTION SYSTEM =====
    
    public void LikePost(string postId)
    {
        NyangstagramPost post = allPosts.Find(p => p.postId == postId);
        if (post == null) return;
        
        if (post.isLikedByPlayer)
        {
            // Unlike
            post.likes--;
            post.isLikedByPlayer = false;
        }
        else
        {
            // Like
            post.likes++;
            post.isLikedByPlayer = true;
            
            // Reward post owner
            if (post.userId != currentPlayerId)
            {
                RewardForLike(post.userId);
            }
        }
        
        Debug.Log($"[Nyangstagram] Post {postId} now has {post.likes} likes");
    }
    
    public void CommentOnPost(string postId, string comment)
    {
        NyangstagramPost post = allPosts.Find(p => p.postId == postId);
        if (post == null) return;
        
        post.comments.Add($"{GetPlayerName()}: {comment}");
        
        // Reward post owner
        if (post.userId != currentPlayerId)
        {
            RewardForComment(post.userId);
        }
        
        Debug.Log($"[Nyangstagram] Comment added to post {postId}");
    }
    
    // ===== FOLLOW SYSTEM =====
    
    public void FollowUser(string userId)
    {
        NyangstagramUser userToFollow = allUsers.Find(u => u.userId == userId);
        if (userToFollow == null) return;
        
        if (!userToFollow.isFollowedByPlayer)
        {
            userToFollow.followers++;
            userToFollow.isFollowedByPlayer = true;
            
            Debug.Log($"[Nyangstagram] Following user {userId}");
        }
    }
    
    public void UnfollowUser(string userId)
    {
        NyangstagramUser userToUnfollow = allUsers.Find(u => u.userId == userId);
        if (userToUnfollow == null) return;
        
        if (userToUnfollow.isFollowedByPlayer)
        {
            userToUnfollow.followers--;
            userToUnfollow.isFollowedByPlayer = false;
            
            Debug.Log($"[Nyangstagram] Unfollowed user {userId}");
        }
    }
    
    public List<NyangstagramPost> GetUserPosts(string userId)
    {
        List<NyangstagramPost> userPosts = new List<NyangstagramPost>();
        foreach (NyangstagramPost post in allPosts)
        {
            if (post.userId == userId)
            {
                userPosts.Add(post);
            }
        }
        return userPosts;
    }
    
    // ===== REWARD SYSTEM =====
    
    void RewardForPosting(string postType)
    {
        int rewardCoins = 0;
        
        switch (postType)
        {
            case "cat_photo":
                rewardCoins = 50;
                break;
            case "cafe_decoration":
                rewardCoins = 75;
                break;
            case "achievement":
                rewardCoins = 100;
                break;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency("coin", rewardCoins);
            Debug.Log($"[Nyangstagram Reward] +{rewardCoins} coins for posting");
        }
    }
    
    void RewardForLike(string postOwnerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency("coin", likeRewardCoins);
            Debug.Log($"[Nyangstagram Reward] +{likeRewardCoins} coins for receiving like");
        }
    }
    
    void RewardForComment(string postOwnerId)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency("coin", commentRewardCoins);
            Debug.Log($"[Nyangstagram Reward] +{commentRewardCoins} coins for receiving comment");
        }
    }
    
    // ===== UTILITY METHODS =====
    
    void ResetDailyPostCount()
    {
        if (lastPostDate != DateTime.Now)
        {
            postsCreatedToday = 0;
            lastPostDate = DateTime.Now;
        }
    }
    
    string GetPlayerName()
    {
        // TODO: Get from GameManager or PlayerPrefs
        return PlayerPrefs.GetString("PlayerName", "Anonymous");
    }
    
    void LoadNyangstagramData()
    {
        // TODO: Load from server or local storage
        Debug.Log("[Nyangstagram] Data loaded");
    }
    
    public void SaveNyangstagramData()
    {
        // TODO: Save to server or local storage
        Debug.Log("[Nyangstagram] Data saved");
    }
    
    public int GetPostsCreatedToday()
    {
        return postsCreatedToday;
    }
    
    public int GetRemainingPostsToday()
    {
        return maxPostsPerDay - postsCreatedToday;
    }
}
