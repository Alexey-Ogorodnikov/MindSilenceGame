using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Cold-start branded splash overlay. After splash, AppHost shows menu, training, or highscore.
    /// Menu Training + (i) are on MenuStub.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class SplashHost : MonoBehaviour
    {
        public const string OverlayName = "SplashOverlay";
        public const string IconName = "SplashIcon";
        public const string TitleName = "SplashTitle";

        [SerializeField] Sprite splashIcon;

        RectTransform _overlay;
        UnityTimeSource _time;
        SplashController _controller;

        public SplashController Controller => _controller;

        void Awake()
        {
            BuildHierarchy(GetComponent<RectTransform>(), splashIcon);
            CacheChildren();
            _time = new UnityTimeSource();
            _controller = new SplashController(SplashProcessGate.ConsumeColdStart(), _time);
            _controller.Changed += ApplyState;
            ApplyState();
            if (GetComponent<AppHost>() == null)
            {
                gameObject.AddComponent<AppHost>();
            }
        }

        void Start()
        {
            _controller?.OnContentMeasured();
        }

        void Update()
        {
            _time?.NotifyAdvanced();
        }

        void OnDestroy()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.Changed -= ApplyState;
            _controller.Dispose();
            _controller = null;
        }

        public static void BuildHierarchy(RectTransform panel, Sprite icon)
        {
            if (panel == null)
            {
                throw new System.ArgumentNullException(nameof(panel));
            }

            var overlay = FindOrCreate(panel, OverlayName);
            Stretch(overlay);
            var overlayImage = overlay.GetComponent<Image>() ?? overlay.gameObject.AddComponent<Image>();
            overlayImage.color = AppPalette.SplashBackground;
            overlayImage.raycastTarget = false;

            var iconRect = FindOrCreate(overlay, IconName);
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(SplashDefaults.IconSize, SplashDefaults.IconSize);
            iconRect.localScale = Vector3.one;
            iconRect.localPosition = new Vector3(iconRect.localPosition.x, iconRect.localPosition.y, 0f);
            var iconImage = iconRect.GetComponent<Image>() ?? iconRect.gameObject.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.color = Color.white;
            iconRect.gameObject.name = IconName;

            var titleRect = FindOrCreate(overlay, TitleName);
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(900f, 80f);
            var panelHeight = panel.rect.height > 1f ? panel.rect.height : panel.sizeDelta.y;
            titleRect.anchoredPosition = SplashLayout.TitleAnchoredPosition(panelHeight);
            titleRect.localScale = Vector3.one;
            titleRect.localPosition = new Vector3(titleRect.localPosition.x, titleRect.localPosition.y, 0f);
            var title = titleRect.GetComponent<Text>() ?? titleRect.gameObject.AddComponent<Text>();
            title.font = BuiltinFont();
            title.text = AppStrings.AppName;
            title.fontSize = Mathf.RoundToInt(SplashDefaults.TitleFontSize);
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = AppPalette.TitleGradientMid;
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            title.raycastTarget = false;
            var shadow = titleRect.GetComponent<Shadow>() ?? titleRect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = AppPalette.TitleShadow;
            shadow.effectDistance = new Vector2(0f, -2f);

            overlay.SetAsLastSibling();

            var surface = panel.Find("PanelSurface")?.GetComponent<Image>();
            if (surface != null)
            {
                surface.color = AppPalette.CalmBackgroundLight;
            }
        }

        void CacheChildren()
        {
            _overlay = transform.Find(OverlayName) as RectTransform;
        }

        void ApplyState()
        {
            var showSplash = _controller != null && _controller.ShowBrandedSplash;
            if (_overlay != null)
            {
                _overlay.gameObject.SetActive(showSplash);
            }
        }

        static RectTransform FindOrCreate(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
            return rect;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
