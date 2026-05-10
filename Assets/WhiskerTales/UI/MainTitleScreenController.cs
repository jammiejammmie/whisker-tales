using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class MainTitleScreenController : UIScreenBase
    {
        [SerializeField] private Image background;
        [SerializeField] private RectTransform logoRect;
        [SerializeField] private RectTransform catsRect;
        [SerializeField] private RectTransform taglineRect;
        [SerializeField] private TextMeshProUGUI tagline;

        protected override void Awake()
        {
            base.Awake();
            ApplyLayout();
        }

        public void ApplyLayout()
        {
            if (background != null)
            {
                RectTransform bg = background.GetComponent<RectTransform>();

                if (bg != null)
                {
                    UIFactory.ApplyRect(bg, Vector2.zero, new Vector2(1080, 1920), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                }
            }

            if (logoRect != null)
            {
                UIFactory.ApplyRect(logoRect, new Vector2(0, -160), new Vector2(520, 250), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            }

            if (catsRect != null)
            {
                UIFactory.ApplyRect(catsRect, new Vector2(0, -130), new Vector2(900, 980), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            }

            if (taglineRect != null)
            {
                UIFactory.ApplyRect(taglineRect, new Vector2(0, 280), new Vector2(900, 90), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            }

            if (tagline != null)
            {
                tagline.text = "고요한 한옥에서 고양이와 함께";
                tagline.fontSize = 54;
                tagline.color = UILayoutConstants.Text;
                tagline.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}
