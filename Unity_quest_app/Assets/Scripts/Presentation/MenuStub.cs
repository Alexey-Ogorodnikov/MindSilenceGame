using System;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Menu panel: Training + (i) and How to train overlay. Does not reference AppNavigator.
    /// </summary>
    public sealed class MenuStub : MonoBehaviour
    {
        public const string RowName = "MenuRow";
        public const string InfoSpacerName = "MenuInfoSpacer";
        public const string TrainingButtonName = "MenuTrainingButton";
        public const string InfoButtonName = "MenuInfoButton";
        public const string OverlayName = "HowToTrainOverlay";
        public const string ScrimName = "HowToTrainScrim";
        public const string CardName = "HowToTrainCard";
        public const string TitleName = "HowToTrainTitle";
        public const string BodyName = "HowToTrainBody";
        public const string OkButtonName = "HowToTrainOk";
        public const string FooterName = "HowToTrainFooter";
        public const string FooterSpacerName = "HowToTrainFooterSpacer";
        public const string InfoGlyphName = "MenuInfoGlyph";

        public const float InfoButtonSize = 120f;
        public const float RowSpacing = 50f;
        public const float TrainingWidth = 520f;
        public const float TrainingHeight = 120f;
        public const float DialogCardWidth = 1000f;
        public const float DialogTitleHeight = 64f;
        public const float DialogPadding = 32f;
        public const float DialogTopPadding = 28f;
        public const float DialogBottomPadding = 24f;
        public const float DialogSpacing = 16f;
        public const float OkButtonWidth = 250f;
        public const float OkButtonHeight = 100f;

        MenuController _controller;
        RectTransform _overlay;
        Button _trainingButton;
        Button _infoButton;
        Button _okButton;
        Button _scrimButton;

        public MenuController Controller
        {
            get
            {
                EnsureController();
                return _controller;
            }
        }

        public event Action NavigateToTraining;

        public void EmitNavigateToTraining() => Controller.OpenTraining();

        public void EmitOpenHowToTrain() => Controller.OpenHowToTrain();

        public void EmitDismissHowToTrain() => Controller.DismissHowToTrain();

        void Awake()
        {
            EnsureController();
            BindUi();
        }

        void OnDestroy()
        {
            UnbindUi();
            if (_controller == null)
            {
                return;
            }

            _controller.Changed -= ApplyHowToTrainVisibility;
            _controller.NavigateToTraining -= OnControllerNavigateToTraining;
            _controller = null;
        }

        public static void BuildHierarchy(RectTransform menu)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            DestroyNamed(menu, "MenuStubLabel");
            EnsureRow(menu);
            EnsureOverlay(menu);
            var stub = menu.GetComponent<MenuStub>();
            stub?.BindUi();
        }

        void EnsureController()
        {
            if (_controller != null)
            {
                return;
            }

            _controller = new MenuController();
            _controller.Changed += ApplyHowToTrainVisibility;
            _controller.NavigateToTraining += OnControllerNavigateToTraining;
        }

        void BindUi()
        {
            CacheChildren();
            WireButtons();
            ApplyCopy();
            ApplyHowToTrainVisibility();
        }

        void CacheChildren()
        {
            _overlay = transform.Find(OverlayName) as RectTransform;
            _trainingButton = transform.Find(RowName + "/" + TrainingButtonName)?.GetComponent<Button>();
            _infoButton = transform.Find(RowName + "/" + InfoButtonName)?.GetComponent<Button>();
            _okButton = transform.Find(OverlayName + "/" + CardName + "/" + FooterName + "/" + OkButtonName)
                ?.GetComponent<Button>();
            _scrimButton = transform.Find(OverlayName + "/" + ScrimName)?.GetComponent<Button>();
        }

        void WireButtons()
        {
            Wire(_trainingButton, OnTrainingClicked);
            Wire(_infoButton, OnInfoClicked);
            Wire(_okButton, OnDismissClicked);
            Wire(_scrimButton, OnDismissClicked);
        }

        void UnbindUi()
        {
            Unwire(_trainingButton, OnTrainingClicked);
            Unwire(_infoButton, OnInfoClicked);
            Unwire(_okButton, OnDismissClicked);
            Unwire(_scrimButton, OnDismissClicked);
        }

        void ApplyCopy()
        {
            SetLabel(transform.Find(RowName + "/" + TrainingButtonName + "/Label"), AppStrings.MenuTraining, 40);
            SetLabel(transform.Find(RowName + "/" + InfoButtonName + "/" + InfoGlyphName), "i", 56);
            SetLabel(transform.Find(RowName + "/" + InfoButtonName + "/ContentDescription"), AppStrings.MenuHowToTrainCd, 1);
            SetLabel(transform.Find(OverlayName + "/" + CardName + "/" + TitleName), AppStrings.HowToTrainTitle, 44);
            SetLabel(transform.Find(OverlayName + "/" + CardName + "/" + BodyName), AppStrings.HowToTrainBody, 32);
            SetLabel(
                transform.Find(OverlayName + "/" + CardName + "/" + FooterName + "/" + OkButtonName + "/Label"),
                AppStrings.Ok,
                40);
            if (_infoButton != null)
            {
                _infoButton.gameObject.name = InfoButtonName;
            }
        }

        void ApplyHowToTrainVisibility()
        {
            if (_overlay == null)
            {
                CacheChildren();
            }

            if (_overlay != null)
            {
                _overlay.gameObject.SetActive(_controller != null && _controller.ShowHowToTrain);
                if (_overlay.gameObject.activeSelf)
                {
                    _overlay.SetAsLastSibling();
                    var card = _overlay.Find(CardName) as RectTransform;
                    if (card != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(card);
                    }
                }
            }
        }

        void OnTrainingClicked() => Controller.OpenTraining();

        void OnInfoClicked() => Controller.OpenHowToTrain();

        void OnDismissClicked() => Controller.DismissHowToTrain();

        void OnControllerNavigateToTraining() => NavigateToTraining?.Invoke();

        static void EnsureRow(RectTransform menu)
        {
            var row = FindOrCreate(menu, RowName);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 0.5f);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(
                InfoButtonSize + RowSpacing + TrainingWidth + RowSpacing + InfoButtonSize,
                TrainingHeight);
            row.anchoredPosition = Vector2.zero;
            Flatten(row);
            var layout = row.GetComponent<HorizontalLayoutGroup>() ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 0, 0);

            var spacer = FindOrCreate(row, InfoSpacerName);
            SetLayoutSize(spacer, InfoButtonSize, InfoButtonSize);
            Flatten(spacer);

            var training = CreateButton(
                row,
                TrainingButtonName,
                TrainingWidth,
                TrainingHeight,
                AppPalette.CalmBlue,
                AppStrings.MenuTraining,
                40,
                AppPalette.CalmSurfaceLight);

            var info = CreateButton(
                row,
                InfoButtonName,
                InfoButtonSize,
                InfoButtonSize,
                AppPalette.CalmSurfaceLight,
                string.Empty,
                56,
                AppPalette.CalmBlue);
            var glyph = info.Find("Label");
            if (glyph != null)
            {
                glyph.name = InfoGlyphName;
                var glyphText = glyph.GetComponent<Text>();
                if (glyphText != null)
                {
                    glyphText.text = "i";
                    glyphText.fontStyle = FontStyle.Bold;
                    glyphText.color = AppPalette.CalmBlue;
                }
            }

            var description = FindOrCreate(info, "ContentDescription");
            description.anchorMin = description.anchorMax = new Vector2(0.5f, 0.5f);
            description.sizeDelta = Vector2.zero;
            Flatten(description);
            ApplyText(description, AppStrings.MenuHowToTrainCd, 1, Color.clear, TextAnchor.MiddleCenter, false);

            spacer.SetSiblingIndex(0);
            training.SetSiblingIndex(1);
            info.SetSiblingIndex(2);
        }

        static void EnsureOverlay(RectTransform menu)
        {
            var overlay = FindOrCreate(menu, OverlayName);
            Stretch(overlay);
            Flatten(overlay);
            overlay.gameObject.SetActive(false);

            var scrim = FindOrCreate(overlay, ScrimName);
            Stretch(scrim);
            Flatten(scrim);
            var scrimImage = scrim.GetComponent<Image>() ?? scrim.gameObject.AddComponent<Image>();
            scrimImage.color = new Color(0f, 0f, 0f, 0.35f);
            scrimImage.raycastTarget = true;
            var scrimButton = scrim.GetComponent<Button>() ?? scrim.gameObject.AddComponent<Button>();
            scrimButton.targetGraphic = scrimImage;
            scrimButton.transition = Selectable.Transition.None;

            var card = FindOrCreate(overlay, CardName);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(DialogCardWidth, 0f);
            Flatten(card);
            var cardImage = card.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
            cardImage.color = AppPalette.CalmSurfaceLight;
            cardImage.raycastTarget = true;

            var cardLayout = card.GetComponent<VerticalLayoutGroup>() ?? card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(
                (int)DialogPadding,
                (int)DialogPadding,
                (int)DialogTopPadding,
                (int)DialogBottomPadding);
            cardLayout.spacing = DialogSpacing;
            cardLayout.childAlignment = TextAnchor.UpperLeft;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var cardFitter = card.GetComponent<ContentSizeFitter>() ?? card.gameObject.AddComponent<ContentSizeFitter>();
            cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var cardSize = card.GetComponent<LayoutElement>() ?? card.gameObject.AddComponent<LayoutElement>();
            cardSize.minWidth = DialogCardWidth;
            cardSize.preferredWidth = DialogCardWidth;
            cardSize.flexibleWidth = 0f;

            var title = FindOrCreate(card, TitleName);
            Flatten(title);
            ApplyText(title, AppStrings.HowToTrainTitle, 44, AppPalette.OnBackground, TextAnchor.MiddleLeft, false);
            SetFixedHeight(title, DialogTitleHeight);

            var body = FindOrCreate(card, BodyName);
            Flatten(body);
            ApplyText(body, AppStrings.HowToTrainBody, 32, AppPalette.OnBackground, TextAnchor.UpperLeft, true);
            var innerWidth = DialogCardWidth - (DialogPadding * 2f);
            var bodyText = body.GetComponent<Text>();
            var bodyHeight = WrappedTextHeight(bodyText, innerWidth);
            if (bodyHeight < (bodyText != null ? bodyText.fontSize * 2f : 64f))
            {
                bodyHeight = 32f * 8f;
            }

            SetFixedHeight(body, bodyHeight);

            var existingOkOnCard = card.Find(OkButtonName);
            var footer = FindOrCreate(card, FooterName);
            Flatten(footer);
            if (existingOkOnCard != null && existingOkOnCard.parent == card)
            {
                existingOkOnCard.SetParent(footer, false);
            }

            var footerLayout = footer.GetComponent<HorizontalLayoutGroup>() ?? footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.padding = new RectOffset(0, 0, 0, 0);
            footerLayout.spacing = 0f;
            footerLayout.childAlignment = TextAnchor.MiddleRight;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = false;
            SetFixedHeight(footer, OkButtonHeight);

            var spacer = FindOrCreate(footer, FooterSpacerName);
            Flatten(spacer);
            var spacerLayout = spacer.GetComponent<LayoutElement>() ?? spacer.gameObject.AddComponent<LayoutElement>();
            spacerLayout.minWidth = 0f;
            spacerLayout.preferredWidth = 0f;
            spacerLayout.flexibleWidth = 1f;
            spacerLayout.minHeight = 0f;
            spacerLayout.preferredHeight = 1f;
            spacerLayout.flexibleHeight = 0f;

            CreateButton(
                footer,
                OkButtonName,
                OkButtonWidth,
                OkButtonHeight,
                AppPalette.CalmBlue,
                AppStrings.Ok,
                40,
                AppPalette.CalmSurfaceLight);
            var ok = footer.Find(OkButtonName) as RectTransform;
            Flatten(ok);

            spacer.SetSiblingIndex(0);
            if (ok != null)
            {
                ok.SetSiblingIndex(1);
            }

            title.SetSiblingIndex(0);
            body.SetSiblingIndex(1);
            footer.SetSiblingIndex(2);
            scrim.SetSiblingIndex(0);
            card.SetSiblingIndex(1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);
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
            var image = buttonRect.GetComponent<Image>() ?? buttonRect.gameObject.AddComponent<Image>();
            image.color = background;
            image.raycastTarget = true;
            var button = buttonRect.GetComponent<Button>() ?? buttonRect.gameObject.AddComponent<Button>();
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
            var text = rect.GetComponent<Text>() ?? rect.gameObject.AddComponent<Text>();
            text.font = BuiltinFont();
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            text.verticalOverflow = wrap ? VerticalWrapMode.Overflow : VerticalWrapMode.Overflow;
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

        static void SetFixedHeight(RectTransform rect, float height)
        {
            var layout = rect.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0f;
            layout.flexibleWidth = 1f;
        }

        static float WrappedTextHeight(Text text, float width)
        {
            if (text == null || string.IsNullOrEmpty(text.text) || width <= 0f)
            {
                return 0f;
            }

            var settings = text.GetGenerationSettings(new Vector2(width, 0f));
            settings.generateOutOfBounds = true;
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            return Mathf.Max(
                text.fontSize,
                text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, settings));
        }

        static void SetLayoutSize(RectTransform rect, float width, float height)
        {
            rect.sizeDelta = new Vector2(width, height);
            var layout = rect.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
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

        static void DestroyNamed(Transform parent, string name)
        {
            var found = parent.Find(name);
            if (found == null)
            {
                return;
            }

            DestroyComponent(found.gameObject);
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

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Flatten(rect);
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
