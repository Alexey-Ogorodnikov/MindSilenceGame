using System.IO;
using MindSilence.Data;
using MindSilence.Domain;
using MindSilence.XR;
using UnityEngine;
using UnityEngine.UI;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Post-splash host: one XR scene, menu / training / highscore from AppNavigator flags.
    /// Child panels send effects; this host calls navigator methods.
    /// One JsonGameProgressRepository per process; GameSession lives only while training is shown
    /// and is destroyed when highscore is shown, then created Idle again on LeaveHighScores.
    /// </summary>
    public sealed class AppHost : MonoBehaviour
    {
        public const string MenuStubName = "MenuStub";
        public const string TrainingStubName = "TrainingStub";
        public const string HighScoresStubName = "HighScoresStub";
        public const float PanelWidth = 2400f;
        public const float PanelHeight = 1800f;
        public const float ContentWidth = 1400f;
        public const float ContentHeight = 1700f;
        public const string ContentRootName = "ContentRoot";

        public static IGameProgressRepository TestRepository;
        public static ITimeSource TestTime;

        AppNavigator _navigator;
        SplashController _splash;
        MenuStub _menu;
        TrainingStub _training;
        HighScoresStub _highScores;
        IGameProgressRepository _progress;
        ITimeSource _time;
        GameSession _session;
        LifecycleBridge _lifecycle;
        HapticBridge _haptic;

        public AppNavigator Navigator => _navigator;

        public MenuStub Menu => _menu;

        public TrainingStub Training => _training;

        public HighScoresStub HighScores => _highScores;

        public GameSession Session => _session;

        public ITimeSource TimeSource => _time;

        public IGameProgressRepository Progress => _progress;

        void Awake()
        {
            _navigator = new AppNavigator();
            _progress = TestRepository ?? new JsonGameProgressRepository(
                Path.Combine(Application.persistentDataPath, JsonGameProgressRepository.FileName));
            _time = TestTime ?? new UnityTimeSource();
            _lifecycle = GetComponent<LifecycleBridge>() ?? gameObject.AddComponent<LifecycleBridge>();
            _haptic = GetComponent<HapticBridge>() ?? gameObject.AddComponent<HapticBridge>();
            _lifecycle.ApplicationPaused += OnApplicationPaused;
            BuildHierarchy(GetComponent<RectTransform>());
            CacheChildren();
            Wire();
            var splashHost = GetComponent<SplashHost>();
            _splash = splashHost != null ? splashHost.Controller : null;
            if (_splash != null)
            {
                _splash.Changed += ApplyState;
            }

            _navigator.Changed += ApplyState;
            ApplyState();
            if (Application.isPlaying)
            {
                var canvas = GetComponent<Canvas>();
                UiInteractionRig.Ensure(canvas);
                InputModalityGate.Ensure(canvas);
            }
        }

        void Update()
        {
            if (_time is UnityTimeSource unityTime)
            {
                unityTime.NotifyAdvanced();
            }
        }

        void OnDestroy()
        {
            Unwire();
            if (_lifecycle != null)
            {
                _lifecycle.ApplicationPaused -= OnApplicationPaused;
            }

            if (_splash != null)
            {
                _splash.Changed -= ApplyState;
            }

            if (_navigator != null)
            {
                _navigator.Changed -= ApplyState;
            }

            DisposeSession();
            KeepAwakeBridge.SetEnabled(false);
        }

        public static void BuildHierarchy(RectTransform panel)
        {
            if (panel == null)
            {
                throw new System.ArgumentNullException(nameof(panel));
            }

            var overlay = panel.Find(SplashHost.OverlayName);
            var menu = FindOrCreate(panel, MenuStubName);
            GetOrAdd<MenuStub>(menu.gameObject);
            Stretch(menu);
            MenuStub.BuildHierarchy(menu);

            var training = FindOrCreate(panel, TrainingStubName);
            GetOrAdd<TrainingStub>(training.gameObject);
            Stretch(training);
            TrainingStub.BuildHierarchy(training, ring: null);

            var highScores = FindOrCreate(panel, HighScoresStubName);
            GetOrAdd<HighScoresStub>(highScores.gameObject);
            Stretch(highScores);
            HighScoresStub.BuildHierarchy(highScores);

            menu.SetSiblingIndex(FirstStubSibling(panel));
            training.SetSiblingIndex(menu.GetSiblingIndex() + 1);
            highScores.SetSiblingIndex(training.GetSiblingIndex() + 1);
            if (overlay != null)
            {
                overlay.SetAsLastSibling();
            }

            var surface = panel.Find("PanelSurface")?.GetComponent<Image>();
            if (surface != null)
            {
                surface.color = AppPalette.CalmBackgroundLight;
            }
        }

        void CacheChildren()
        {
            _menu = transform.Find(MenuStubName)?.GetComponent<MenuStub>();
            _training = transform.Find(TrainingStubName)?.GetComponent<TrainingStub>();
            _highScores = transform.Find(HighScoresStubName)?.GetComponent<HighScoresStub>();
        }

        void Wire()
        {
            if (_menu != null)
            {
                _menu.NavigateToTraining += OnNavigateToTraining;
            }

            if (_training != null)
            {
                _training.NavigateToHighScores += OnNavigateToHighScores;
                _training.NavigateBackToMenu += OnNavigateBackToMenu;
            }

            if (_highScores != null)
            {
                _highScores.NavigateBack += OnNavigateBack;
            }
        }

        void Unwire()
        {
            if (_menu != null)
            {
                _menu.NavigateToTraining -= OnNavigateToTraining;
            }

            if (_training != null)
            {
                _training.NavigateToHighScores -= OnNavigateToHighScores;
                _training.NavigateBackToMenu -= OnNavigateBackToMenu;
            }

            if (_highScores != null)
            {
                _highScores.NavigateBack -= OnNavigateBack;
            }
        }

        void OnNavigateToTraining() => _navigator.OpenTraining();

        void OnNavigateToHighScores() => _navigator.OpenHighScores();

        void OnNavigateBackToMenu() => _navigator.LeaveTraining();

        void OnNavigateBack() => _navigator.LeaveHighScores();

        void OnApplicationPaused(bool paused)
        {
            GetComponent<InputModalityGate>()?.NotifyPaused(paused);
            if (_session == null)
            {
                return;
            }

            if (paused)
            {
                _session.AppBackgrounded();
            }
            else
            {
                _session.AppForegrounded();
            }
        }

        void OnKeepAwake(bool enabled) => KeepAwakeBridge.SetEnabled(enabled);

        void OnHaptic() => _haptic?.PlayThought();

        void ApplyState()
        {
            var showSplash = _splash != null && _splash.ShowBrandedSplash;
            var showMenu = !showSplash && _navigator.ShowMenu;
            var showTraining = !showSplash && _navigator.ShowTraining;
            var showHighScores = !showSplash && _navigator.ShowHighScores;
            if (_menu != null)
            {
                _menu.gameObject.SetActive(showMenu);
            }

            if (_training != null)
            {
                _training.gameObject.SetActive(showTraining);
            }

            if (_highScores != null)
            {
                _highScores.gameObject.SetActive(showHighScores);
                if (showHighScores)
                {
                    _highScores.Bind(_progress);
                }
            }

            SyncSession(showTraining);
        }

        void SyncSession(bool showTraining)
        {
            if (showTraining)
            {
                if (_session != null)
                {
                    return;
                }

                _session = new GameSession(_progress, _time);
                _session.KeepAwake += OnKeepAwake;
                _session.HapticOnThought += OnHaptic;
                _training?.Bind(_session);
                return;
            }

            DisposeSession();
        }

        void DisposeSession()
        {
            if (_session == null)
            {
                return;
            }

            _training?.Unbind();
            _session.KeepAwake -= OnKeepAwake;
            _session.HapticOnThought -= OnHaptic;
            _session.Dispose();
            _session = null;
            KeepAwakeBridge.SetEnabled(false);
        }

        static int FirstStubSibling(RectTransform panel)
        {
            var overlay = panel.Find(SplashHost.OverlayName);
            if (overlay != null)
            {
                return Mathf.Max(0, overlay.GetSiblingIndex());
            }

            return panel.childCount;
        }

        public static RectTransform EnsureContentRoot(RectTransform panel)
        {
            if (panel == null)
            {
                throw new System.ArgumentNullException(nameof(panel));
            }

            var existing = panel.Find(ContentRootName) as RectTransform;
            RectTransform root;
            if (existing != null)
            {
                root = existing;
            }
            else
            {
                var go = new GameObject(ContentRootName, typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(panel, false);
                root = go.GetComponent<RectTransform>();
            }

            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(ContentWidth, ContentHeight);
            root.localScale = Vector3.one;
            root.localPosition = new Vector3(root.localPosition.x, root.localPosition.y, 0f);
            return root;
        }

        public static void MoveNamedTo(Transform from, Transform to, params string[] names)
        {
            if (from == null || to == null || names == null)
            {
                return;
            }

            foreach (var name in names)
            {
                var child = from.Find(name);
                if (child != null && child.parent == from)
                {
                    child.SetParent(to, false);
                }
            }
        }

        static T GetOrAdd<T>(GameObject host) where T : Component
        {
            var found = host.GetComponent<T>();
            if (found == null)
            {
                found = host.AddComponent<T>();
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
    }
}
