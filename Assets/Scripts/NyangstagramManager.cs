using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class NyangstagramPost
{
    public string postId;
    public string userId;
    public string userName;
    public string postType;
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

    [SerializeField] private int maxPostsPerDay = 5;
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

    public bool CreatePost(string postType, string caption, string imageUrl)
    {
        if (postsCreatedToday >= maxPostsPerDay) return false;

        NyangstagramPost newPost = new NyangstagramPost();
        newPost.postId = System.DateTime.Now.Ticks.ToString();
        newPost.userId = currentPlayerId;
        newPost.userName = GetPlayerName();
        newPost.postType = postType;
        newPost.caption = caption;
        newPost.imageUrl = imageUrl;
        newPost.postTime = DateTime.Now;
        newPost.likes = 0;
        newPost.comments = new List<string>();
        newPost.isLikedByPlayer = false;

        allPosts.Add(newPost);
        postsCreatedToday++;
        RewardForPosting(postType);
        return true;
    }

    public void LikePost(string postId)
    {
        NyangstagramPost post = null;
        for (int i = 0; i < allPosts.Count; i++)
        {
            if (allPosts[i].postId == postId) { post = allPosts[i]; break; }
        }
        if (post == null) return;

        if (post.isLikedByPlayer)
        {
            post.likes--;
            post.isLikedByPlayer = false;
        }
        else
        {
            post.likes++;
            post.isLikedByPlayer = true;
            if (post.userId != currentPlayerId) RewardForLike(post.userId);
        }
    }

    public void CommentOnPost(string postId, string comment)
    {
        NyangstagramPost post = null;
        for (int i = 0; i < allPosts.Count; i++)
        {
            if (allPosts[i].postId == postId) { post = allPosts[i]; break; }
        }
        if (post == null) return;

        post.comments.Add(GetPlayerName() + ": " + comment);
        if (post.userId != currentPlayerId) RewardForComment(post.userId);
    }

    public void FollowUser(string userId)
    {
        for (int i = 0; i < allUsers.Count; i++)
        {
            if (allUsers[i].userId == userId && !allUsers[i].isFollowedByPlayer)
            {
                allUsers[i].followers++;
                allUsers[i].isFollowedByPlayer = true;
                break;
            }
        }
    }

    public void UnfollowUser(string userId)
    {
        for (int i = 0; i < allUsers.Count; i++)
        {
            if (allUsers[i].userId == userId && allUsers[i].isFollowedByPlayer)
            {
                allUsers[i].followers--;
                allUsers[i].isFollowedByPlayer = false;
                break;
            }
        }
    }

    void RewardForPosting(string postType)
    {
        int rewardCoins = 100;
        if (postType == "cat_photo") rewardCoins = 50;
        else if (postType == "cafe_decoration") rewardCoins = 75;
        if (WhiskerTales.Core.GameManager.Instance != null)
            WhiskerTales.Core.GameManager.Instance.AddCoins(rewardCoins);
    }

    void RewardForLike(string postOwnerId)
    {
        if (WhiskerTales.Core.GameManager.Instance != null)
            WhiskerTales.Core.GameManager.Instance.AddCoins(likeRewardCoins);
    }

    void RewardForComment(string postOwnerId)
    {
        if (WhiskerTales.Core.GameManager.Instance != null)
            WhiskerTales.Core.GameManager.Instance.AddCoins(commentRewardCoins);
    }

    void ResetDailyPostCount()
    {
        if (lastPostDate.Date != DateTime.Now.Date)
        {
            postsCreatedToday = 0;
            lastPostDate = DateTime.Now;
        }
    }

    string GetPlayerName() { return PlayerPrefs.GetString("PlayerName", "Anonymous"); }
    void LoadNyangstagramData() { Debug.Log("[Nyangstagram] Data loaded"); }
    public void SaveNyangstagramData() { Debug.Log("[Nyangstagram] Data saved"); }
    public int GetPostsCreatedToday() { return postsCreatedToday; }
    public int GetRemainingPostsToday() { return maxPostsPerDay - postsCreatedToday; }
}
