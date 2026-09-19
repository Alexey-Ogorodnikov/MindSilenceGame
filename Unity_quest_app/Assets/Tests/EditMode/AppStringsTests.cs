using MindSilence.Presentation;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class AppStringsTests
    {
        static readonly string[] ExpectedKeys =
        {
            "app_name",
            "level_label",
            "start",
            "thought",
            "level_progress",
            "level_idle",
            "start_content_description",
            "thought_content_description",
            "session_summary_title",
            "session_level_reached",
            "session_best_today",
            "session_attempt_duration",
            "ok",
            "highscore",
            "high_scores_title",
            "back",
            "high_scores_empty",
            "daily_attempts",
            "daily_total_time",
            "daily_best_level",
            "menu_training",
            "menu_how_to_train_cd",
            "how_to_train_title",
            "how_to_train_body",
        };

        [Test]
        public void Table_ContainsHandbookKeysOneToOne()
        {
            Assert.AreEqual(ExpectedKeys.Length, AppStrings.Table.Count);
            CollectionAssert.AreEquivalent(ExpectedKeys, AppStrings.Table.Keys);
        }

        [Test]
        public void Copy_MatchesEnglishKotlinStrings()
        {
            Assert.AreEqual("Mind Silence", AppStrings.Get("app_name"));
            Assert.AreEqual("Level", AppStrings.Get("level_label"));
            Assert.AreEqual("Start", AppStrings.Get("start"));
            Assert.AreEqual("Thought", AppStrings.Get("thought"));
            Assert.AreEqual("{0} / {1} s", AppStrings.Get("level_progress"));
            Assert.AreEqual("—", AppStrings.Get("level_idle"));
            Assert.AreEqual("Start session", AppStrings.Get("start_content_description"));
            Assert.AreEqual("Log a thought and end the session", AppStrings.Get("thought_content_description"));
            Assert.AreEqual("Session complete", AppStrings.Get("session_summary_title"));
            Assert.AreEqual("Level reached: {0}", AppStrings.Get("session_level_reached"));
            Assert.AreEqual("Best today: {0}", AppStrings.Get("session_best_today"));
            Assert.AreEqual("Time in attempt: {0} min {1} s", AppStrings.Get("session_attempt_duration"));
            Assert.AreEqual("OK", AppStrings.Get("ok"));
            Assert.AreEqual("Highscore", AppStrings.Get("highscore"));
            Assert.AreEqual("Highscore", AppStrings.Get("high_scores_title"));
            Assert.AreEqual("Back", AppStrings.Get("back"));
            Assert.AreEqual("No records yet", AppStrings.Get("high_scores_empty"));
            Assert.AreEqual("Attempts: {0}", AppStrings.Get("daily_attempts"));
            Assert.AreEqual("Total: {0} min {1} s", AppStrings.Get("daily_total_time"));
            Assert.AreEqual("Best level: {0}", AppStrings.Get("daily_best_level"));
            Assert.AreEqual("Mind Silence Training", AppStrings.Get("menu_training"));
            Assert.AreEqual("How to train", AppStrings.Get("menu_how_to_train_cd"));
            Assert.AreEqual("How to train", AppStrings.Get("how_to_train_title"));
        }

        [Test]
        public void HowToTrainBody_MatchesKotlinStringsXml()
        {
            const string expected =
                "Sit comfortably and tap Start.\n\n" +
                "Watch the mind. While there is silence — do nothing. The level rises on its own: each next one lasts twice as long.\n\n" +
                "As soon as a thought appears — tap Thought. The session will end, and you will see the level you reached.\n\n" +
                "The goal is to stay in silence as long as possible.\n\n" +
                "Practice for just 5 minutes a day. With regular practice, you may feel calmer and more balanced, while your thoughts become quieter, clearer, and more peaceful.";
            Assert.AreEqual(expected, AppStrings.HowToTrainBody);
            Assert.AreEqual(expected, AppStrings.Get("how_to_train_body"));
        }

        [Test]
        public void Format_UsesCsharpPlaceholders()
        {
            Assert.AreEqual("2 / 4 s", AppStrings.FormatLevelProgress(2, 4));
            Assert.AreEqual("Level reached: 3", AppStrings.FormatSessionLevelReached(3));
            Assert.AreEqual("Best today: 4", AppStrings.FormatSessionBestToday(4));
            Assert.AreEqual("Time in attempt: 1 min 12 s", AppStrings.FormatSessionAttemptDuration(1, 12));
            Assert.AreEqual("Attempts: 2", AppStrings.FormatDailyAttempts(2));
            Assert.AreEqual("Total: 0 min 90 s", AppStrings.FormatDailyTotalTime(0, 90));
            Assert.AreEqual("Best level: 5", AppStrings.FormatDailyBestLevel(5));
        }

        [Test]
        public void FormatDate_UsesEnglishDayMonthYear()
        {
            Assert.AreEqual("5 September 2026", AppStrings.FormatDate(new System.DateTime(2026, 9, 5)));
            Assert.AreEqual("1 January 2026", AppStrings.FormatDate(new System.DateTime(2026, 1, 1)));
        }

        [Test]
        public void Table_HasNoRussianLocaleKeys()
        {
            foreach (var key in AppStrings.Table.Keys)
            {
                StringAssert.DoesNotContain("ru", key);
            }
        }
    }
}
