using System;
using UnityEngine;

namespace WhiskerTales.UI.Home
{
    [Serializable]
    public sealed class HomeObjectEntry
    {
        public string id;
        public Sprite sprite;
        public Vector2 anchoredPosition;
        public Vector2 size = new Vector2(320f, 220f);
        public Vector2 anchorMin = new Vector2(0.5f, 0f);
        public Vector2 anchorMax = new Vector2(0.5f, 0f);
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public HomeInteractionTarget interaction = HomeInteractionTarget.None;
    }

    [CreateAssetMenu(fileName = "HomeObjectSet", menuName = "Whisker Tales/Home Object Set")]
    public sealed class HomeObjectSet : ScriptableObject
    {
        [SerializeField] private HomeObjectEntry[] entries;

        public HomeObjectEntry[] Entries
        {
            get { return entries; }
        }
    }
}
