using System;
using System.Globalization;
using MindSilence.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Highscore panel: daily list newest first, Back to a new Idle training.
    /// Does not reference AppNavigator.
    /// </summary>
    public sealed class HighScoresStub : MonoBehaviour
    {
        public const string TitleName = "HighScoresTitle";
        public const string BackButtonName = "HighScoresBackButton";
        public const string EmptyName = "HighScoresEmpty";
        public const string ScrollName = "HighScoresScroll";
        public const string ViewportName = "HighScoresViewport";
        public const string ContentName = "HighScoresContent";
        public const string ScrollbarName = "HighScoresScrollbar";
        public const string DayRowPrefix = "HighScoreDay_";
        public const string DayDateName = "Date";
        public const string DayAttemptsName = "Attempts";
        public const string DayTotalName = "Total";
        public const string DayBestName = "Best";
        public const float BackWidth = 200f;
        public const float BackHeight = 100f;
        public const float ScrollbarWidth = 80f;
        public const float DayCardHeight = 220f;

        IGameProgressRepository _progress;
        Button _backButton;
        Text _empty;
        RectTransform _content;
        ScrollRect _scroll;

        public event Action NavigateBack;

        public void EmitNavigateBack() => NavigateBack?.Invoke();

        public void Bind(IGameProgressRepository progress)
        {
            _progress = progress;
            Refresh();
        }

        void Awake()
        {
            BuildHierarchy(GetComponent<RectTransform>());
            CacheChildren();
            WireButtons();
            ApplyCopy();
            Refresh();
        }

        void OnEnable()
        {
            Refresh();
        }

        void OnDestroy()
        {
            UnwireButtons();
        }

        public static void BuildHierarchy(RectTransform highScores)
        {
            if (highScores == null)
            {
                throw new ArgumentNullException(nameof(highScores));
            }

            var stage = AppHost.EnsureContentRoot(highScores);
            AppHost.MoveNamedTo(highScores, stage, BackButtonName, TitleName, EmptyName, ScrollName);
            EnsureBack(stage);
            EnsureTitle(stage);
            EnsureScroll(stage);
            var stub = highScores.GetComponent<HighScoresStub>();
            if (stub != null)
            {
                stub.CacheChildren();
                stub.WireButtons();
                stub.ApplyCopy();
                stub.Refresh();
            }
        }

        void CacheChildren()
        {
            _backButton = FindStage(BackButtonName)?.GetComponent<Button>();
            _empty = FindStage(EmptyName)?.GetComponent<Text>();
            _content = FindStage(ScrollName + "/" + ViewportName + "/" + ContentName) as RectTransform;
            _scroll = FindStage(ScrollName)?.GetComponent<ScrollRect>();
        }

        void WireButtons()
        {
            Wire(_backButton, OnBackClicked);
        }

        void UnwireButtons()
        {
            Unwire(_backButton, OnBackClicked);
        }

        void ApplyCopy()
        {
            SetLabel(FindStage(TitleName), AppStrings.HighScoresTitle, 44);
            SetLabel(FindStage(BackButtonName + "/Label"), AppStrings.Back, 36);
            SetLabel(FindStage(EmptyName), AppStrings.HighScoresEmpty, 36);
        }

        void Refresh()
        {
            if (_content == null)
            {
                CacheChildren();
            }

            if (_content == null)
            {
                return;
            }

            ClearDays();
            var stats = _progress != null ? _progress.GetDailyStats() : Array.Empty<DailyStats>();
            var hasRecords = stats.Count > 0;
            if (_empty != null)
            {
                _empty.gameObject.SetActive(!hasRecords);
                _empty.text = AppStrings.HighScoresEmpty;
                if (!hasRecords)
                {
                    _empty.transform.SetAsLastSibling();
                }
            }

            if (_scroll != null)
            {
                _scroll.gameObject.SetActive(hasRecords);
            }

            if (!hasRecords)
            {
                return;
            }

            for (var i = 0; i < stats.Count; i++)
            {
                CreateDayCard(_content, stats[i]);
            }

            if (_scroll != null)
            {
                _scroll.verticalNormalizedPosition = 1f;
            }
        }

        void ClearDays()
        {
            for (var i = _content.childCount - 1; i >= 0; i--)
            {
                var child = _content.GetChild(i);
                child.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        void OnBackClicked() => NavigateBack?.Invoke();

        Transform FindStage(string path)
        {
            var staged = transform.Find(AppHost.ContentRootName + "/" + path);
            return staged != null ? staged : transform.Find(path);
        }

        static void EnsureBack(RectTransform highScores)
        {
            var back = CreateButton(
                highScores,
                BackButtonName,
                BackWidth,
                BackHeight,
                AppPalette.CalmSurfaceLight,
                AppStrings.Back,
                36,
                AppPalette.CalmBlue);
            back.anchorMin = back.anchorMax = new Vector2(0f, 1f);
            back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = new Vector2(24f, -24f);
            back.sizeDelta = new Vector2(BackWidth, BackHeight);
            var layout = back.GetComponent<LayoutElement>();
            if (layout != null)
            {
                DestroyGo(layout);
            }

            Flatten(back);
        }

        static void EnsureTitle(RectTransform highScores)
        {
            var title = FindOrCreate(highScores, TitleName);
            title.anchorMin = new Vector2(0.5f, 1f);
            title.anchorMax = new Vector2(0.5f, 1f);
            title.pivot = new Vector2(0.5f, 1f);
            title.sizeDelta = new Vector2(480f, 64f);
            title.anchoredPosition = new Vector2(0f, -42f);
            Flatten(title);
            ApplyText(title, AppStrings.HighScoresTitle, 44, AppPalette.OnBackground, TextAnchor.MiddleCenter, false);

            var empty = FindOrCreate(highScores, EmptyName);
            empty.anchorMin = new Vector2(0.5f, 0.5f);
            empty.anchorMax = new Vector2(0.5f, 0.5f);
            empty.pivot = new Vector2(0.5f, 0.5f);
            empty.sizeDelta = new Vector2(720f, 80f);
            empty.anchoredPosition = Vector2.zero;
            Flatten(empty);
            ApplyText(
                empty,
                AppStrings.HighScoresEmpty,
                36,
                new Color(AppPalette.OnBackground.r, AppPalette.OnBackground.g, AppPalette.OnBackground.b, 0.7f),
                TextAnchor.MiddleCenter,
                true);
        }

        static void EnsureScroll(RectTransform highScores)
        {
            var scrollRect = FindOrCreate(highScores, ScrollName);
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(24f, 24f);
            scrollRect.offsetMax = new Vector2(-24f, -140f);
            Flatten(scrollRect);
            var scroll = GetOrAdd<ScrollRect>(scrollRect);
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.scrollSensitivity = 40f;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            var viewport = FindOrCreate(scrollRect, ViewportName);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = new Vector2(-ScrollbarWidth - 8f, 0f);
            Flatten(viewport);
            var viewportImage = GetOrAdd<Image>(viewport);
            viewportImage.color = AppPalette.CalmBackgroundLight;
            viewportImage.raycastTarget = true;
            GetOrAdd<RectMask2D>(viewport);

            var content = FindOrCreate(viewport, ContentName);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);
            Flatten(content);
            var column = GetOrAdd<VerticalLayoutGroup>(content);
            column.spacing = 12f;
            column.padding = new RectOffset(0, 0, 0, 8);
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            var fitter = GetOrAdd<ContentSizeFitter>(content);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollbarRect = FindOrCreate(scrollRect, ScrollbarName);
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 1f);
            scrollbarRect.sizeDelta = new Vector2(ScrollbarWidth, 0f);
            scrollbarRect.anchoredPosition = Vector2.zero;
            Flatten(scrollbarRect);
            var track = GetOrAdd<Image>(scrollbarRect);
            track.color = new Color(AppPalette.CalmBlue.r, AppPalette.CalmBlue.g, AppPalette.CalmBlue.b, 0.22f);
            track.raycastTarget = true;
            var scrollbar = GetOrAdd<Scrollbar>(scrollbarRect);
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = track;

            var sliding = FindOrCreate(scrollbarRect, "SlidingArea");
            sliding.anchorMin = Vector2.zero;
            sliding.anchorMax = Vector2.one;
            sliding.offsetMin = new Vector2(8f, 8f);
            sliding.offsetMax = new Vector2(-8f, -8f);
            Flatten(sliding);

            var handle = FindOrCreate(sliding, "Handle");
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            handle.offsetMin = Vector2.zero;
            handle.offsetMax = Vector2.zero;
            Flatten(handle);
            var handleImage = GetOrAdd<Image>(handle);
            handleImage.color = AppPalette.CalmBlue;
            handleImage.raycastTarget = true;
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.verticalScrollbar = scrollbar;

            viewport.SetSiblingIndex(0);
            scrollbarRect.SetSiblingIndex(1);
        }

        static void CreateDayCard(RectTransform content, DailyStats stats)
        {
            var iso = stats.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var card = FindOrCreate(content, DayRowPrefix + iso);
            SetLayoutSize(card, 0f, DayCardHeight);
            Flatten(card);
            var image = GetOrAdd<Image>(card);
            image.color = AppPalette.CalmSurfaceLight;
            image.raycastTarget = true;
            var column = GetOrAdd<VerticalLayoutGroup>(card);
            column.spacing = 8f;
            column.padding = new RectOffset(24, 24, 20, 20);
            column.childAlignment = TextAnchor.UpperLeft;
            column.childControlWidth = true;
            column.childControlHeight = false;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            var date = FindOrCreate(card, DayDateName);
            SetLayoutSize(date, 0f, 40f);
            Flatten(date);
            ApplyText(date, AppStrings.FormatDate(stats.Date), 36, AppPalette.CalmBlue, TextAnchor.MiddleLeft, false);

            var attempts = FindOrCreate(card, DayAttemptsName);
            SetLayoutSize(attempts, 0f, 32f);
            Flatten(attempts);
            ApplyText(
                attempts,
                AppStrings.FormatDailyAttempts(stats.Attempts),
                32,
                AppPalette.OnBackground,
                TextAnchor.MiddleLeft,
                false);

            var total = FindOrCreate(card, DayTotalName);
            SetLayoutSize(total, 0f, 32f);
            Flatten(total);
            ApplyText(
                total,
                AppStrings.FormatDailyTotalTime(stats.TotalSeconds / 60, stats.TotalSeconds % 60),
                32,
                AppPalette.OnBackground,
                TextAnchor.MiddleLeft,
                false);

            var best = FindOrCreate(card, DayBestName);
            SetLayoutSize(best, 0f, 32f);
            Flatten(best);
            ApplyText(
                best,
                AppStrings.FormatDailyBestLevel(stats.BestLevel),
                32,
                AppPalette.OnBackground,
                TextAnchor.MiddleLeft,
                false);

            date.SetSiblingIndex(0);
            attempts.SetSiblingIndex(1);
            total.SetSiblingIndex(2);
            best.SetSiblingIndex(3);
        }

        static RectTransform CreateButton(
            RectTransform parent,
            string name,
            float width,
            float height,
            Color background,
            string label,
            int fontSize,
            Color labelColor)
        {
            var buttonRect = FindOrCreate(parent, name);
            SetLayoutSize(buttonRect, width, height);
            Flatten(buttonRect);
            var image = GetOrAdd<Image>(buttonRect);
            image.color = background;
            image.raycastTarget = true;
            var button = GetOrAdd<Button>(buttonRect);
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            var labelRect = FindOrCreate(buttonRect, "Label");
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            Flatten(labelRect);
            ApplyText(labelRect, label, fontSize, labelColor, TextAnchor.MiddleCenter, false);
            return buttonRect;
        }

        static void ApplyText(
            RectTransform rect,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment,
            bool wrap)
        {
            var text = GetOrAdd<Text>(rect);
            text.font = BuiltinFont();
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
        }

        static void SetLabel(Transform node, string value, int fontSize)
        {
            if (node == null)
            {
                return;
            }

            var text = node.GetComponent<Text>();
            if (text == null)
            {
                return;
            }

            text.text = value;
            text.fontSize = fontSize;
            text.raycastTarget = false;
        }

        static void SetLayoutSize(RectTransform rect, float width, float height)
        {
            if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, height);
            }
            else
            {
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            }

            var layout = GetOrAdd<LayoutElement>(rect);
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0f;
            if (width > 0f)
            {
                layout.minWidth = width;
                layout.preferredWidth = width;
                layout.flexibleWidth = 0f;
            }
            else
            {
                layout.minWidth = 0f;
                layout.preferredWidth = 0f;
                layout.flexibleWidth = 1f;
            }
        }

        static void Wire(Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }

        static void Unwire(Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(handler);
        }

        static void DestroyGo(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        static T GetOrAdd<T>(Component host) where T : Component
        {
            var found = host.GetComponent<T>();
            if (found == null)
            {
                found = host.gameObject.AddComponent<T>();
            }

            return found;
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
            Flatten(rect);
            return rect;
        }

        static void Flatten(RectTransform rect)
        {
            rect.localScale = Vector3.one;
            rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0f);
        }

        static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
