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
