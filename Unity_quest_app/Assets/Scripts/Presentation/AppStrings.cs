using System;
using System.Collections.Generic;
using System.Globalization;

namespace MindSilence.Presentation
{
    /// <summary>
    /// English UI copy. Keys match handbook/unity-quest.md 1:1 with Kotlin strings.xml.
    /// Presenters must use this table; do not hardcode copy.
    /// </summary>
    public static class AppStrings
    {
        public const string AppName = "Mind Silence";
        public const string LevelLabel = "Level";
        public const string Start = "Start";
        public const string Thought = "Thought";
        public const string LevelProgress = "{0} / {1} s";
        public const string LevelIdle = "—";
        public const string StartContentDescription = "Start session";
        public const string ThoughtContentDescription = "Log a thought and end the session";
        public const string SessionSummaryTitle = "Session complete";
        public const string SessionLevelReached = "Level reached: {0}";
        public const string SessionBestToday = "Best today: {0}";
        public const string SessionAttemptDuration = "Time in attempt: {0} min {1} s";
        public const string Ok = "OK";
        public const string Highscore = "Highscore";
        public const string HighScoresTitle = "Highscore";
        public const string Back = "Back";
        public const string HighScoresEmpty = "No records yet";
        public const string DailyAttempts = "Attempts: {0}";
        public const string DailyTotalTime = "Total: {0} min {1} s";
        public const string DailyBestLevel = "Best level: {0}";
        public const string MenuTraining = "Mind Silence Training";
        public const string MenuHowToTrainCd = "How to train";
        public const string HowToTrainTitle = "How to train";
        public const string HowToTrainBody =
            "Sit comfortably and tap Start.\n\n" +
            "Watch the mind. While there is silence — do nothing. The level rises on its own: each next one lasts twice as long.\n\n" +
            "As soon as a thought appears — tap Thought. The session will end, and you will see the level you reached.\n\n" +
            "The goal is to stay in silence as long as possible.\n\n" +
            "Practice for just 5 minutes a day. With regular practice, you may feel calmer and more balanced, while your thoughts become quieter, clearer, and more peaceful.";

        public static readonly IReadOnlyDictionary<string, string> Table = new Dictionary<string, string>
        {
            ["app_name"] = AppName,
            ["level_label"] = LevelLabel,
            ["start"] = Start,
            ["thought"] = Thought,
            ["level_progress"] = LevelProgress,
            ["level_idle"] = LevelIdle,
            ["start_content_description"] = StartContentDescription,
            ["thought_content_description"] = ThoughtContentDescription,
            ["session_summary_title"] = SessionSummaryTitle,
            ["session_level_reached"] = SessionLevelReached,
            ["session_best_today"] = SessionBestToday,
            ["session_attempt_duration"] = SessionAttemptDuration,
            ["ok"] = Ok,
            ["highscore"] = Highscore,
            ["high_scores_title"] = HighScoresTitle,
            ["back"] = Back,
            ["high_scores_empty"] = HighScoresEmpty,
            ["daily_attempts"] = DailyAttempts,
            ["daily_total_time"] = DailyTotalTime,
            ["daily_best_level"] = DailyBestLevel,
            ["menu_training"] = MenuTraining,
            ["menu_how_to_train_cd"] = MenuHowToTrainCd,
            ["how_to_train_title"] = HowToTrainTitle,
            ["how_to_train_body"] = HowToTrainBody,
        };

        public static string Get(string key) => Table[key];

        public static string FormatLevelProgress(int elapsed, int required) =>
            Format(LevelProgress, elapsed, required);

        public static string FormatSessionLevelReached(int level) =>
            Format(SessionLevelReached, level);

        public static string FormatSessionBestToday(int level) =>
            Format(SessionBestToday, level);

        public static string FormatSessionAttemptDuration(int minutes, int seconds) =>
            Format(SessionAttemptDuration, minutes, seconds);

        public static string FormatDailyAttempts(int attempts) =>
            Format(DailyAttempts, attempts);

        public static string FormatDailyTotalTime(int minutes, int seconds) =>
            Format(DailyTotalTime, minutes, seconds);

        public static string FormatDailyBestLevel(int level) =>
            Format(DailyBestLevel, level);

        public static string FormatDate(DateTime date) =>
            date.ToString("d MMMM yyyy", English);

        static string Format(string template, params object[] args) =>
            string.Format(CultureInfo.InvariantCulture, template, args);

        static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
    }
}
