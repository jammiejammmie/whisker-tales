using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §5. 로딩 오버레이. 일반 스피너 대신 고양이 얼굴이 Z축으로 회전 (2초당 1회전).
    /// 고양이별 한국어 메시지 5종 (§5-3). cat_purring.wav 루프 40% 볼륨.
    /// SoundMode.Mute일 때는 골골송도 OFF.
    /// AppBootstrap이 entries / purring AudioClip / 회전 시간 등을 주입.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [System.Serializable]
        public class CatLoadingEntry
        {
            public int catId;             // Constants.CAT_*
            public Sprite face;           // 고양이 풀샷 (얼굴 영역에 회전 마스킹은 후속)
            public string messageKo;
            public string messageEn;
        }

        [SerializeField] private Image catFaceImage;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private AudioSource purringSource;
        [SerializeField] private CatLoadingEntry[] entries;
        [SerializeField] private float rotationPeriodSeconds = 2f;
        [SerializeField, Range(0f, 1f)] private float purringVolume = 0.4f;

        public bool IsShown { get; private set; }

        private void Awake()
        {
            if (purringSource != null)
            {
                purringSource.loop = true;
                purringSource.volume = purringVolume;
            }
        }

        public void Show(int catId = -1)
        {
            CatLoadingEntry pick = ResolveEntry(catId);
            if (pick != null)
            {
                if (catFaceImage != null && pick.face != null) catFaceImage.sprite = pick.face;
                if (messageText != null) messageText.text = StripSupplementaryPlane(pick.messageKo);
            }

            gameObject.SetActive(true);
            IsShown = true;

            if (purringSource != null && SoundManager.Instance != null
                && SoundManager.Instance.CurrentMode != SoundMode.Mute)
            {
                purringSource.volume = purringVolume;
                if (!purringSource.isPlaying) purringSource.Play();
            }
        }

        public void Hide()
        {
            if (purringSource != null && purringSource.isPlaying) purringSource.Stop();
            gameObject.SetActive(false);
            IsShown = false;
        }

        private static string StripSupplementaryPlane(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                {
                    i++;
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString().TrimEnd();
        }

        private CatLoadingEntry ResolveEntry(int catId)
        {
            if (entries == null || entries.Length == 0) return null;
            if (catId > 0)
            {
                foreach (var e in entries)
                {
                    if (e != null && e.catId == catId) return e;
                }
            }
            return entries[Random.Range(0, entries.Length)];
        }

        private void Update()
        {
            if (!IsShown || catFaceImage == null || rotationPeriodSeconds <= 0f) return;
            float speed = 360f / rotationPeriodSeconds;
            catFaceImage.transform.Rotate(0f, 0f, -speed * Time.unscaledDeltaTime);
        }
    }
}
