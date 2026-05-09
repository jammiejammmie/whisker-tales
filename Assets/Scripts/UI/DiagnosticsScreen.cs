using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;
using WhiskerTales.Core;
using WhiskerTales.Diagnostics;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Stage 4B 품질 진단 결과 화면. 51+개 항목을 카테고리별로 색상 표시.
    /// AppBootstrap이 패널 + ScrollRect content + 버튼들을 빌드하고 SerializeField로 주입.
    /// </summary>
    public class DiagnosticsScreen : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button copyReportButton;
        [SerializeField] private Button rerunButton;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private RectTransform scrollContent;

        private static readonly Color PassColor = new Color(0.20f, 0.55f, 0.30f);
        private static readonly Color FailColor = new Color(0.85f, 0.25f, 0.25f);
        private static readonly Color WarnColor = new Color(0.85f, 0.55f, 0.20f);
        private static readonly Color CategoryColor = new Color(0.30f, 0.20f, 0.12f);
        private static readonly Color RowBgPass = new Color(0.92f, 0.97f, 0.92f);
        private static readonly Color RowBgFail = new Color(0.99f, 0.91f, 0.91f);
        private static readonly Color RowBgWarn = new Color(0.99f, 0.96f, 0.85f);
        private static readonly Color CategoryBgColor = new Color(0.96f, 0.93f, 0.85f);

        private DiagnosticReport currentReport;

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
            if (copyReportButton != null) copyReportButton.onClick.AddListener(HandleCopy);
            if (rerunButton != null) rerunButton.onClick.AddListener(RunAndRender);

            // 진입할 때마다 자동 실행
            RunAndRender();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (copyReportButton != null) copyReportButton.onClick.RemoveListener(HandleCopy);
            if (rerunButton != null) rerunButton.onClick.RemoveListener(RunAndRender);
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.RequestNavigation(NavigationTarget.Settings);
        }

        private void HandleCopy()
        {
            AudioManager.instance?.PlayButtonClick();
            if (currentReport == null) return;
            string text = QualityDiagnostics.BuildTextReport(currentReport);
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("[Diagnostics] Report copied to clipboard (" + text.Length + " chars)");

            if (copyReportButton != null)
            {
                TMP_Text label = copyReportButton.GetComponentInChildren<TMP_Text>();
                if (label != null) StartCoroutine(FlashLabel(label, "복사 완료 ✓", "📋 리포트 복사"));
            }
        }

        private System.Collections.IEnumerator FlashLabel(TMP_Text label, string flashText, string restoreText)
        {
            string originalText = label.text;
            label.text = flashText;
            yield return new WaitForSeconds(1.2f);
            label.text = restoreText;
        }

        public void RunAndRender()
        {
            currentReport = QualityDiagnostics.Run();
            Render(currentReport);
        }

        private void Render(DiagnosticReport report)
        {
            if (summaryText != null)
            {
                summaryText.text = $"{report.passed}/{report.Total} PASS"
                    + (report.failed > 0 ? $"  ❌ {report.failed} FAIL" : "")
                    + (report.warned > 0 ? $"  ⚠ {report.warned} WARN" : "");
                summaryText.color = report.failed == 0 ? PassColor : FailColor;
            }

            if (scrollContent == null) return;

            // 기존 자식 모두 제거
            for (int i = scrollContent.childCount - 1; i >= 0; i--)
            {
                Destroy(scrollContent.GetChild(i).gameObject);
            }

            string lastCategory = "";
            foreach (var item in report.items)
            {
                if (item.category != lastCategory)
                {
                    BuildCategoryHeader(scrollContent, item.category, report);
                    lastCategory = item.category;
                }
                BuildItemRow(scrollContent, item);
            }
        }

        private void BuildCategoryHeader(Transform parent, string category, DiagnosticReport report)
        {
            int catPass = 0, catFail = 0, catWarn = 0, catTotal = 0;
            foreach (var i in report.items)
            {
                if (i.category != category) continue;
                catTotal++;
                if (i.status == DiagnosticStatus.Pass) catPass++;
                else if (i.status == DiagnosticStatus.Fail) catFail++;
                else catWarn++;
            }

            GameObject go = new GameObject($"Section_{category}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.flexibleWidth = 1f;
            Image bg = go.GetComponent<Image>();
            bg.color = CategoryBgColor;

            // 카테고리 라벨
            GameObject txtGO = new GameObject("HeaderText", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform txtRt = txtGO.GetComponent<RectTransform>();
            txtRt.SetParent(rt, false);
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(20, 0);
            txtRt.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI text = txtGO.AddComponent<TextMeshProUGUI>();
            text.fontSize = 36;
            text.fontStyle = FontStyles.Bold;
            text.color = CategoryColor;
            text.alignment = TextAlignmentOptions.Left;
            text.text = $"{category}    ({catPass}/{catTotal} PASS"
                + (catFail > 0 ? $", {catFail} FAIL" : "")
                + (catWarn > 0 ? $", {catWarn} WARN" : "")
                + ")";
            text.raycastTarget = false;
        }

        private void BuildItemRow(Transform parent, DiagnosticItem item)
        {
            GameObject go = new GameObject($"Row_{item.label}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.flexibleWidth = 1f;
            Image bg = go.GetComponent<Image>();
            bg.color = item.status == DiagnosticStatus.Pass ? RowBgPass
                     : item.status == DiagnosticStatus.Fail ? RowBgFail
                     : RowBgWarn;

            // 상태 아이콘 (왼쪽)
            GameObject iconGO = new GameObject("StatusIcon", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform iconRt = iconGO.GetComponent<RectTransform>();
            iconRt.SetParent(rt, false);
            iconRt.anchorMin = new Vector2(0, 0);
            iconRt.anchorMax = new Vector2(0, 1);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(20, 0);
            iconRt.sizeDelta = new Vector2(80, 0);
            TextMeshProUGUI icon = iconGO.AddComponent<TextMeshProUGUI>();
            icon.fontSize = 36;
            icon.alignment = TextAlignmentOptions.Center;
            icon.fontStyle = FontStyles.Bold;
            icon.text = item.status == DiagnosticStatus.Pass ? "✓"
                      : item.status == DiagnosticStatus.Fail ? "✗"
                      : "⚠";
            icon.color = item.status == DiagnosticStatus.Pass ? PassColor
                       : item.status == DiagnosticStatus.Fail ? FailColor
                       : WarnColor;
            icon.raycastTarget = false;

            // 라벨 + 디테일 (수직 스택)
            GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.SetParent(rt, false);
            labelRt.anchorMin = new Vector2(0, 0.5f);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.offsetMin = new Vector2(110, 0);
            labelRt.offsetMax = new Vector2(-20, -4);
            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = 30;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = item.status == DiagnosticStatus.Pass ? new Color(0.18f, 0.18f, 0.18f)
                            : item.status == DiagnosticStatus.Fail ? FailColor
                            : new Color(0.40f, 0.30f, 0.10f);
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.text = item.label;
            labelText.raycastTarget = false;

            GameObject detailGO = new GameObject("Detail", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform detailRt = detailGO.GetComponent<RectTransform>();
            detailRt.SetParent(rt, false);
            detailRt.anchorMin = new Vector2(0, 0);
            detailRt.anchorMax = new Vector2(1, 0.5f);
            detailRt.offsetMin = new Vector2(110, 4);
            detailRt.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI detailText = detailGO.AddComponent<TextMeshProUGUI>();
            detailText.fontSize = 22;
            detailText.color = new Color(0.30f, 0.30f, 0.32f);
            detailText.alignment = TextAlignmentOptions.Left;
            detailText.text = string.IsNullOrEmpty(item.detail) ? "" : item.detail;
            detailText.raycastTarget = false;
        }
    }
}
