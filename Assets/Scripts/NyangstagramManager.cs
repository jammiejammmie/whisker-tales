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
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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
        RewardForPosting(postType);
        return true;
    }
    
    public List<NyangstagramPost> GetFeed(int pageNumber = 0, int postsPerPage = 10)
    {
        List<NyangstagramPost> feed = new List<NyangstagramPost>();
        for (int i = allPosts.Count - 1; i >= 0 && feed.Count < postsPerPage; i--)
            feed.Add(allPosts[i]);
        return feed;
    }
    
    public void LikePost(string postId)
    {
        NyangstagramPost post = allPosts.Find(p => p.postId == postId);
        if (post == null) return;
        if (post.isLikedByPlayer) { post.likes--; post.isLikedByPlayer = false; }
        else { post.likes++; post.isLikedByPlayer = true; if (post.userId != currentPlayerId) RewardForLike(post.userId); }
    }
    
    public void CommentOnPost(string postId, string comment)
    {
        NyangstagramPost post = allPosts.Find(p => p.postId == postId);
        if (post == null) return;
        post.comments.Add($"{GetPlayerName()}: {comment}");
        if (post.userId != currentPlayerId) RewardForComment(post.userId);
    }
    
    public void FollowUser(string userId)
    {
        NyangstagramUser user = allUsers.Find(u => u.userId == userId);
        if (user != null && !user.isFollowedByPlayer) { user.followers++; user.isFollowedByPlayer = true; }
    }
    
    public void Unfo
