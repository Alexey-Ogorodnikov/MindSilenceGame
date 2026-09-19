using System;
using System.Globalization;
using MindSilence.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Training panel bound to GameSession, including Session complete overlay.
    /// Does not reference AppNavigator.
    /// </summary>
    public sealed class TrainingStub : MonoBehaviour
    {
        public const string BackButtonName = "TrainingBackButton";
        public const string FocusName = "TrainingFocus";
        public const string LevelLabelName = "TrainingLevelLabel";
        public const string RingSlotName = "TrainingRingSlot";
        public const string RingImageName = "TrainingRingImage";
        public const string LevelGlyphName = "TrainingLevelGlyph";
        public const string ProgressName = "TrainingProgress";
        public const string ProgressTrackName = "TrainingProgressTrack";
        public const string ProgressFillName = "TrainingProgressFill";
        public const string ProgressLabelName = "TrainingProgressLabel";
        public const string ButtonRowName = "TrainingButtonRow";
        public const string StartButtonName = "TrainingStartButton";
        public const string ThoughtButtonName = "TrainingThoughtButton";
        public const string SummaryOverlayName = "SessionComplete";
        public const string SummaryScrimName = "SessionCompleteScrim";
        public const string SummaryCardName = "SessionCompleteCard";
        public const string SummaryTitleName = "SessionCompleteTitle";
        public const string SummaryLevelName = "SessionCompleteLevel";
        public const string SummaryBestName = "SessionCompleteBest";
        public const string SummaryDurationName = "SessionCompleteDuration";
        public const string SummaryOkButtonName = "SessionCompleteOk";
        public const string SummaryHighscoreButtonName = "SessionCompleteHighscore";
        public const float SummaryCardWidth = 820f;
        public const float SummaryCardHeight = 480f;
        public const float SummaryButtonWidth = 250f;
        public const float SummaryButtonHeight = 100f;
        public const float SummaryButtonSpacing = 50f;

        [SerializeField] Sprite ringSprite;

        GameSession _session;
        Text _levelGlyph;
        Text _progressLabel;
        CanvasGroup _progressGroup;
        RectTransform _progressFill;
        Button _startButton;
        Button _thoughtButton;
        Button _backButton;
        Button _summaryOkButton;
        Button _summaryHighscoreButton;
        Button _summaryScrimButton;
        Image _ringImage;
        RectTransform _summaryOverlay;
        Text _summaryLevel;
        Text _summaryBest;
        Text _summaryDuration;

        public GameSession Session => _session;

        public event Action NavigateToHighScores;

        public event Action NavigateBackToMenu;

        public void EmitNavigateToHighScores() => NavigateToHighScores?.Invoke();

        public void EmitNavigateBackToMenu() => NavigateBackToMenu?.Invoke();

        public void Bind(GameSession session)
        {
            UnbindSession();
            _session = session;
            if (_session == null)
            {
                ApplyState();
                return;
            }

            _session.Changed += ApplyState;
            _session.NavigateBackToMenu += OnSessionNavigateBackToMenu;
            _session.NavigateToHighScores += OnSessionNavigateToHighScores;
            ApplyState();
        }

        public void Unbind()
        {
            UnbindSession();
            ApplyState();
        }

        void Awake()
        {
            BuildHierarchy(GetComponent<RectTransform>(), ResolveRingSprite(ringSprite));
            CacheChildren();
            WireButtons();
            ApplyCopy();
            ApplyState();
        }

        void OnDestroy()
        {
            UnwireButtons();
            UnbindSession();
        }

        public static void BuildHierarchy(RectTransform training, Sprite ring)
        {
            if (training == null)
            {
                throw new ArgumentNullException(nameof(training));
            }

            var stage = AppHost.EnsureContentRoot(training);
            AppHost.MoveNamedTo(training, stage, BackButtonName, FocusName, ButtonRowName);
            ring = ResolveRingSprite(ring);
            EnsureBack(stage);
            EnsureFocus(stage, ring);
            EnsureButtons(stage);
            EnsureSummary(training);
            var stub = training.GetComponent<TrainingStub>();
            if (stub != null)
            {
                if (ring != null)
                {
                    stub.ringSprite = ring;
                }

                stub.CacheChildren();
                stub.WireButtons();
                stub.ApplyCopy();
                stub.ApplyRingSprite();
                stub.ApplyState();
            }
        }

        void UnbindSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.Changed -= ApplyState;
            _session.NavigateBackToMenu -= OnSessionNavigateBackToMenu;
            _session.NavigateToHighScores -= OnSessionNavigateToHighScores;
            _session = null;
        }

        void CacheChildren()
        {
            _levelGlyph = FindStage(FocusName + "/" + RingSlotName + "/" + LevelGlyphName)?.GetComponent<Text>();
            _progressLabel = FindStage(FocusName + "/" + ProgressName + "/" + ProgressLabelName)?.GetComponent<Text>();
            _progressGroup = FindStage(FocusName + "/" + ProgressName)?.GetComponent<CanvasGroup>();
            _progressFill = FindStage(FocusName + "/" + ProgressName + "/" + ProgressTrackName + "/" + ProgressFillName) as RectTransform;
            _startButton = FindStage(ButtonRowName + "/" + StartButtonName)?.GetComponent<Button>();
            _thoughtButton = FindStage(ButtonRowName + "/" + ThoughtButtonName)?.GetComponent<Button>();
            _backButton = FindStage(BackButtonName)?.GetComponent<Button>();
            _ringImage = FindStage(FocusName + "/" + RingSlotName + "/" + RingImageName)?.GetComponent<Image>();
            _summaryOverlay = transform.Find(SummaryOverlayName) as RectTransform;
            _summaryOkButton = transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryOkButtonName)
                ?.GetComponent<Button>();
            _summaryHighscoreButton = transform.Find(
                    SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryHighscoreButtonName)
                ?.GetComponent<Button>();
            _summaryScrimButton = transform.Find(SummaryOverlayName + "/" + SummaryScrimName)?.GetComponent<Button>();
            _summaryLevel = transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryLevelName)
                ?.GetComponent<Text>();
            _summaryBest = transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryBestName)
                ?.GetComponent<Text>();
            _summaryDuration = transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryDurationName)
                ?.GetComponent<Text>();
        }

        void WireButtons()
        {
            Wire(_startButton, OnStartClicked);
            Wire(_thoughtButton, OnThoughtClicked);
            Wire(_backButton, OnBackClicked);
            Wire(_summaryOkButton, OnSummaryOkClicked);
            Wire(_summaryHighscoreButton, OnSummaryHighscoreClicked);
            Wire(_summaryScrimButton, OnSummaryOkClicked);
        }

        void UnwireButtons()
        {
            Unwire(_startButton, OnStartClicked);
            Unwire(_thoughtButton, OnThoughtClicked);
            Unwire(_backButton, OnBackClicked);
            Unwire(_summaryOkButton, OnSummaryOkClicked);
            Unwire(_summaryHighscoreButton, OnSummaryHighscoreClicked);
            Unwire(_summaryScrimButton, OnSummaryOkClicked);
        }

        void ApplyCopy()
        {
            SetLabel(FindStage(FocusName + "/" + LevelLabelName), AppStrings.LevelLabel, 40);
            SetLabel(FindStage(ButtonRowName + "/" + StartButtonName + "/Label"), AppStrings.Start, 40);
            SetLabel(FindStage(ButtonRowName + "/" + ThoughtButtonName + "/Label"), AppStrings.Thought, 40);
            SetLabel(FindStage(BackButtonName + "/Label"), AppStrings.Back, 36);
            SetLabel(
                transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryTitleName),
                AppStrings.SessionSummaryTitle,
                44);
            SetLabel(
                transform.Find(SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryOkButtonName + "/Label"),
                AppStrings.Ok,
                40);
            SetLabel(
                transform.Find(
                    SummaryOverlayName + "/" + SummaryCardName + "/" + SummaryHighscoreButtonName + "/Label"),
                AppStrings.Highscore,
                40);
            SetLabel(
                FindStage(ButtonRowName + "/" + StartButtonName + "/ContentDescription"),
                AppStrings.StartContentDescription,
                1);
            SetLabel(
                FindStage(ButtonRowName + "/" + ThoughtButtonName + "/ContentDescription"),
                AppStrings.ThoughtContentDescription,
                1);
        }

        void ApplyRingSprite()
        {
            if (_ringImage == null)
            {
                CacheChildren();
            }

            ApplyRingImage(_ringImage, ResolveRingSprite(ringSprite));
        }

        static Sprite ResolveRingSprite(Sprite provided)
        {
            if (provided != null)
            {
                return provided;
            }

            return Resources.Load<Sprite>(TrainingLayout.RingResourceName);
        }

        static void ApplyRingImage(Image image, Sprite ring)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = ring;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = ring != null ? Color.white : Color.clear;
        }

        void ApplyState()
        {
            if (_levelGlyph == null)
            {
                CacheChildren();
            }

            var state = _session != null ? _session.State : new GameSessionState();
            var running = state.Phase == GamePhase.Running;
            if (_levelGlyph != null)
            {
                _levelGlyph.text = running
                    ? state.Level.ToString(CultureInfo.InvariantCulture)
                    : AppStrings.LevelIdle;
                _levelGlyph.color = AppPalette.CalmBlue;
            }

            if (_progressGroup != null)
            {
                _progressGroup.alpha = running ? 1f : 0f;
                _progressGroup.interactable = false;
                _progressGroup.blocksRaycasts = false;
            }

            if (_progressFill != null)
            {
                var fraction = running ? state.ProgressFraction : 0f;
                _progressFill.anchorMin = Vector2.zero;
                _progressFill.anchorMax = new Vector2(fraction, 1f);
                _progressFill.offsetMin = Vector2.zero;
                _progressFill.offsetMax = Vector2.zero;
            }

            if (_progressLabel != null)
            {
                _progressLabel.text = AppStrings.FormatLevelProgress(
                    state.ElapsedSecAtLevel,
                    state.RequiredSecAtLevel);
            }

            if (_startButton != null)
            {
                _startButton.interactable = !running && state.SessionSummary == null;
            }

            if (_thoughtButton != null)
            {
                _thoughtButton.interactable = running;
            }

            ApplySummary(state);
        }

        void ApplySummary(GameSessionState state)
        {
            if (_summaryOverlay == null)
            {
                CacheChildren();
            }

            var summary = state.SessionSummary;
            if (_summaryOverlay != null)
            {
                _summaryOverlay.gameObject.SetActive(summary.HasValue);
                if (summary.HasValue)
                {
                    _summaryOverlay.SetAsLastSibling();
                }
            }

            if (!summary.HasValue)
            {
                return;
            }

            var payload = summary.Value;
            if (_summaryLevel != null)
            {
                _summaryLevel.text = AppStrings.FormatSessionLevelReached(payload.LevelReached);
            }

            if (_summaryBest != null)
            {
                _summaryBest.text = AppStrings.FormatSessionBestToday(payload.BestToday);
            }

            if (_summaryDuration != null)
            {
                _summaryDuration.text = AppStrings.FormatSessionAttemptDuration(
                    payload.TotalSeconds / 60,
                    payload.TotalSeconds % 60);
            }
        }

        void OnStartClicked() => _session?.Start();

        void OnThoughtClicked() => _session?.Thought();

        void OnSummaryOkClicked() => _session?.DismissSessionSummary();

        void OnSummaryHighscoreClicked() => _session?.OpenHighScores();

        void OnBackClicked()
        {
            if (_session != null)
            {
                _session.LeaveTraining();
                return;
            }

            NavigateBackToMenu?.Invoke();
        }

        void OnSessionNavigateBackToMenu() => NavigateBackToMenu?.Invoke();

        void OnSessionNavigateToHighScores() => NavigateToHighScores?.Invoke();

        Transform FindStage(string path)
        {
            var staged = transform.Find(AppHost.ContentRootName + "/" + path);
            return staged != null ? staged : transform.Find(path);
        }

        static void EnsureBack(RectTransform training)
        {
            var back = CreateButton(
                training,
                BackButtonName,
                TrainingLayout.BackWidth,
                TrainingLayout.BackHeight,
                AppPalette.CalmSurfaceLight,
                AppStrings.Back,
                36,
                AppPalette.CalmBlue);
            back.anchorMin = back.anchorMax = new Vector2(0f, 1f);
            back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = new Vector2(24f, -24f);
            back.sizeDelta = new Vector2(TrainingLayout.BackWidth, TrainingLayout.BackHeight);
            var layout = back.GetComponent<LayoutElement>();
            if (layout != null)
            {
                DestroyComponent(layout);
            }

            Flatten(back);
        }

        static void EnsureFocus(RectTransform training, Sprite ring)
        {
            var focus = FindOrCreate(training, FocusName);
            focus.anchorMin = new Vector2(0.5f, 0.5f);
            focus.anchorMax = new Vector2(0.5f, 0.5f);
            focus.pivot = new Vector2(0.5f, 0.5f);
            focus.sizeDelta = new Vector2(TrainingLayout.FocusWidth, TrainingLayout.FocusHeight);
            focus.anchoredPosition = new Vector2(0f, 36f);
            Flatten(focus);
            var column = GetOrAdd<VerticalLayoutGroup>(focus);
            column.spacing = 16f;
            column.childAlignment = TextAnchor.MiddleCenter;
            column.childControlWidth = true;
            column.childControlHeight = false;
            column.childForceExpandWidth = false;
            column.childForceExpandHeight = false;

            var label = FindOrCreate(focus, LevelLabelName);
            SetLayoutSize(label, TrainingLayout.FocusWidth, TrainingLayout.LevelLabelHeight);
            Flatten(label);
            ApplyText(label, AppStrings.LevelLabel, 40, new Color(AppPalette.OnBackground.r, AppPalette.OnBackground.g, AppPalette.OnBackground.b, 0.8f), TextAnchor.MiddleCenter, false);

            var ringSlot = FindOrCreate(focus, RingSlotName);
            SetLayoutSize(ringSlot, TrainingLayout.RingSize, TrainingLayout.RingSize);
            Flatten(ringSlot);

            var ringImage = FindOrCreate(ringSlot, RingImageName);
            ringImage.anchorMin = Vector2.zero;
            ringImage.anchorMax = Vector2.one;
            ringImage.offsetMin = Vector2.zero;
            ringImage.offsetMax = Vector2.zero;
            Flatten(ringImage);
            ApplyRingImage(GetOrAdd<Image>(ringImage), ring);

            var glyph = FindOrCreate(ringSlot, LevelGlyphName);
            glyph.anchorMin = glyph.anchorMax = new Vector2(0.5f, 0.5f);
            glyph.pivot = new Vector2(0.5f, 0.5f);
            glyph.sizeDelta = new Vector2(TrainingLayout.GlyphWidth, TrainingLayout.GlyphHeight);
            glyph.anchoredPosition = TrainingLayout.LevelGlyphAnchoredPosition();
            Flatten(glyph);
            ApplyText(
                glyph,
                AppStrings.LevelIdle,
                TrainingLayout.GlyphFontSize,
                AppPalette.CalmBlue,
                TextAnchor.MiddleCenter,
                false);

            EnsureProgress(focus);

            label.SetSiblingIndex(0);
            ringSlot.SetSiblingIndex(1);
            focus.Find(ProgressName)?.SetSiblingIndex(2);
        }

        static void EnsureProgress(RectTransform focus)
        {
            var progress = FindOrCreate(focus, ProgressName);
            SetLayoutSize(progress, TrainingLayout.ProgressWidth, 72f);
            Flatten(progress);
            var group = GetOrAdd<CanvasGroup>(progress);
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            var column = GetOrAdd<VerticalLayoutGroup>(progress);
            column.spacing = 12f;
            column.childAlignment = TextAnchor.MiddleCenter;
            column.childControlWidth = true;
            column.childControlHeight = false;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            column.padding = new RectOffset(0, 0, 8, 0);

            var track = FindOrCreate(progress, ProgressTrackName);
            SetLayoutSize(track, TrainingLayout.ProgressWidth, TrainingLayout.ProgressHeight);
            Flatten(track);
            var trackImage = GetOrAdd<Image>(track);
            trackImage.color = new Color(AppPalette.CalmBlue.r, AppPalette.CalmBlue.g, AppPalette.CalmBlue.b, 0.22f);
            trackImage.raycastTarget = false;

            var fill = FindOrCreate(track, ProgressFillName);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            Flatten(fill);
            var fillImage = GetOrAdd<Image>(fill);
            fillImage.color = AppPalette.CalmBlue;
            fillImage.raycastTarget = false;

            var caption = FindOrCreate(progress, ProgressLabelName);
            SetLayoutSize(caption, TrainingLayout.ProgressWidth, 36f);
            Flatten(caption);
            ApplyText(
                caption,
                AppStrings.FormatLevelProgress(0, 0),
                32,
                new Color(AppPalette.OnBackground.r, AppPalette.OnBackground.g, AppPalette.OnBackground.b, 0.7f),
                TextAnchor.MiddleCenter,
                false);

            track.SetSiblingIndex(0);
            caption.SetSiblingIndex(1);
        }

        static void EnsureButtons(RectTransform training)
        {
            var row = FindOrCreate(training, ButtonRowName);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0f);
            row.pivot = new Vector2(0.5f, 0f);
            row.sizeDelta = new Vector2(
                TrainingLayout.ActionWidth + TrainingLayout.ActionSpacing + TrainingLayout.ActionWidth,
                TrainingLayout.ActionHeight);
            row.anchoredPosition = new Vector2(0f, 28f);
            Flatten(row);
            var layout = GetOrAdd<HorizontalLayoutGroup>(row);
            layout.spacing = TrainingLayout.ActionSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var start = CreateButton(
                row,
                StartButtonName,
                TrainingLayout.ActionWidth,
                TrainingLayout.ActionHeight,
                AppPalette.CalmBlue,
                AppStrings.Start,
                40,
                AppPalette.CalmSurfaceLight);
            AddContentDescription(start, AppStrings.StartContentDescription);
            ApplyDisabledTint(start.GetComponent<Button>(), AppPalette.CalmBlue);

            var thought = CreateButton(
                row,
                ThoughtButtonName,
                TrainingLayout.ActionWidth,
                TrainingLayout.ActionHeight,
                AppPalette.CalmGreen,
                AppStrings.Thought,
                40,
                AppPalette.CalmSurfaceLight);
            AddContentDescription(thought, AppStrings.ThoughtContentDescription);
            ApplyDisabledTint(thought.GetComponent<Button>(), AppPalette.CalmGreen);

            start.SetSiblingIndex(0);
            thought.SetSiblingIndex(1);
        }

        static void EnsureSummary(RectTransform training)
        {
            var overlay = FindOrCreate(training, SummaryOverlayName);
            StretchFull(overlay);
            Flatten(overlay);
            overlay.gameObject.SetActive(false);

            var scrim = FindOrCreate(overlay, SummaryScrimName);
            StretchFull(scrim);
            Flatten(scrim);
            var scrimImage = GetOrAdd<Image>(scrim);
            scrimImage.color = new Color(0f, 0f, 0f, 0.35f);
            scrimImage.raycastTarget = true;
            var scrimButton = GetOrAdd<Button>(scrim);
            scrimButton.targetGraphic = scrimImage;
            scrimButton.transition = Selectable.Transition.None;

            var card = FindOrCreate(overlay, SummaryCardName);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(SummaryCardWidth, SummaryCardHeight);
            card.anchoredPosition = Vector2.zero;
            Flatten(card);
            var cardImage = GetOrAdd<Image>(card);
            cardImage.color = AppPalette.CalmSurfaceLight;
            cardImage.raycastTarget = true;

            var title = FindOrCreate(card, SummaryTitleName);
            title.anchorMin = new Vector2(0f, 1f);
            title.anchorMax = new Vector2(1f, 1f);
            title.pivot = new Vector2(0.5f, 1f);
            title.sizeDelta = new Vector2(-64f, 64f);
            title.anchoredPosition = new Vector2(0f, -28f);
            Flatten(title);
            ApplyText(title, AppStrings.SessionSummaryTitle, 44, AppPalette.OnBackground, TextAnchor.MiddleLeft, false);

            var level = FindOrCreate(card, SummaryLevelName);
            PlaceSummaryLine(level, -108f);
            ApplyText(level, AppStrings.FormatSessionLevelReached(0), 36, AppPalette.OnBackground, TextAnchor.MiddleLeft, false);

            var best = FindOrCreate(card, SummaryBestName);
            PlaceSummaryLine(best, -160f);
            ApplyText(best, AppStrings.FormatSessionBestToday(0), 36, AppPalette.OnBackground, TextAnchor.MiddleLeft, false);

            var duration = FindOrCreate(card, SummaryDurationName);
            PlaceSummaryLine(duration, -212f);
            ApplyText(
                duration,
                AppStrings.FormatSessionAttemptDuration(0, 0),
                36,
                AppPalette.OnBackground,
                TextAnchor.MiddleLeft,
                false);

            var highscore = CreateButton(
                card,
                SummaryHighscoreButtonName,
                SummaryButtonWidth,
                SummaryButtonHeight,
                AppPalette.CalmBackgroundLight,
                AppStrings.Highscore,
                40,
                AppPalette.CalmBlue);
            PlaceSummaryButton(highscore, new Vector2(32f, 24f), new Vector2(0f, 0f));

            var ok = CreateButton(
                card,
                SummaryOkButtonName,
                SummaryButtonWidth,
                SummaryButtonHeight,
                AppPalette.CalmBlue,
                AppStrings.Ok,
                40,
                AppPalette.CalmSurfaceLight);
            PlaceSummaryButton(ok, new Vector2(-32f, 24f), new Vector2(1f, 0f));

            scrim.SetSiblingIndex(0);
            card.SetSiblingIndex(1);
            overlay.SetAsLastSibling();
        }

        static void PlaceSummaryLine(RectTransform line, float anchoredY)
        {
            line.anchorMin = new Vector2(0f, 1f);
            line.anchorMax = new Vector2(1f, 1f);
            line.pivot = new Vector2(0.5f, 1f);
            line.sizeDelta = new Vector2(-64f, 44f);
            line.anchoredPosition = new Vector2(0f, anchoredY);
            Flatten(line);
        }

        static void PlaceSummaryButton(RectTransform button, Vector2 anchoredPosition, Vector2 pivot)
        {
            button.anchorMin = button.anchorMax = pivot;
            button.pivot = pivot;
            button.anchoredPosition = anchoredPosition;
            button.sizeDelta = new Vector2(SummaryButtonWidth, SummaryButtonHeight);
            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                DestroyComponent(layout);
            }

            Flatten(button);
        }

        static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Flatten(rect);
        }

        static void AddContentDescription(RectTransform button, string value)
        {
            var description = FindOrCreate(button, "ContentDescription");
            description.anchorMin = description.anchorMax = new Vector2(0.5f, 0.5f);
            description.sizeDelta = Vector2.zero;
            Flatten(description);
            ApplyText(description, value, 1, Color.clear, TextAnchor.MiddleCenter, false);
        }

        static void ApplyDisabledTint(Button button, Color background)
        {
            if (button == null)
            {
                return;
            }

            var colors = button.colors;
            colors.disabledColor = new Color(background.r, background.g, background.b, 0.38f);
            button.colors = colors;
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
            rect.sizeDelta = new Vector2(width, height);
            var layout = GetOrAdd<LayoutElement>(rect);
            layout.minWidth = width;
            layout.minHeight = height;
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
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

        static void DestroyComponent(UnityEngine.Object target)
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
