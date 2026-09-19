using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using MindSilence.Domain;

namespace MindSilence.Data
{
    public sealed class JsonGameProgressRepository : IGameProgressRepository
    {
        public const string FileName = "game_progress.json";

        private readonly string _filePath;
        private readonly Func<DateTime> _today;

        public JsonGameProgressRepository(string filePath, Func<DateTime> today = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            _filePath = filePath;
            _today = today ?? (() => DateTime.Now.Date);
        }

        public int RecordSession(int levelReached, int totalSeconds)
        {
            var statsByDate = LoadStats();
            var today = _today().Date;
            DailyStats updated;
            if (!statsByDate.TryGetValue(today, out var current))
            {
                updated = new DailyStats(today, attempts: 1, totalSeconds, levelReached);
            }
            else
            {
                var bestLevel = current.BestLevel > levelReached ? current.BestLevel : levelReached;
                updated = new DailyStats(
                    today,
                    current.Attempts + 1,
                    current.TotalSeconds + totalSeconds,
                    bestLevel);
            }

            statsByDate[today] = updated;
            SaveStats(statsByDate);
            return updated.BestLevel;
        }

        public IReadOnlyList<DailyStats> GetDailyStats()
        {
            var stats = new List<DailyStats>(LoadStats().Values);
            stats.Sort((left, right) => right.Date.CompareTo(left.Date));
            return stats;
        }

        private Dictionary<DateTime, DailyStats> LoadStats()
        {
            if (!File.Exists(_filePath))
                return new Dictionary<DateTime, DailyStats>();

            try
            {
                var json = File.ReadAllText(_filePath, Encoding.UTF8);
                return ParseArray(json);
            }
            catch (Exception)
            {
                return new Dictionary<DateTime, DailyStats>();
            }
        }

        private void SaveStats(Dictionary<DateTime, DailyStats> statsByDate)
        {
            var ordered = new List<DailyStats>(statsByDate.Values);
            ordered.Sort((left, right) => left.Date.CompareTo(right.Date));
            WriteAtomically(ToJson(ordered));
        }

        private void WriteAtomically(string json)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            if (!File.Exists(_filePath))
            {
                File.Move(tempPath, _filePath);
                return;
            }

            try
            {
                File.Replace(tempPath, _filePath, destinationBackupFileName: null);
            }
            catch (PlatformNotSupportedException)
            {
                File.Delete(_filePath);
                File.Move(tempPath, _filePath);
            }
        }

        private static string ToJson(IReadOnlyList<DailyStats> orderedOldestFirst)
        {
            var builder = new StringBuilder();
            builder.Append('[');
            for (var index = 0; index < orderedOldestFirst.Count; index++)
            {
                if (index > 0)
                    builder.Append(',');

                var stat = orderedOldestFirst[index];
                builder.Append("{\"date\":\"");
                builder.Append(stat.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                builder.Append("\",\"attempts\":");
                builder.Append(stat.Attempts.ToString(CultureInfo.InvariantCulture));
                builder.Append(",\"totalSeconds\":");
                builder.Append(stat.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                builder.Append(",\"bestLevel\":");
                builder.Append(stat.BestLevel.ToString(CultureInfo.InvariantCulture));
                builder.Append('}');
            }

            builder.Append(']');
            return builder.ToString();
        }

        private static Dictionary<DateTime, DailyStats> ParseArray(string json)
        {
            if (!string.IsNullOrEmpty(json) && json[0] == '\uFEFF')
                json = json.Substring(1);

            var trimmed = json.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '[' || trimmed[trimmed.Length - 1] != ']')
                throw new FormatException("Expected a JSON array.");

            var result = new Dictionary<DateTime, DailyStats>();
            var index = 1;
            var end = trimmed.Length - 1;
            SkipWhitespace(trimmed, ref index, end);
            if (index >= end)
                return result;

            while (index < end)
            {
                SkipWhitespace(trimmed, ref index, end);
                if (index >= end)
                    break;

                var stat = ReadObject(trimmed, ref index, end);
                result[stat.Date] = stat;
                SkipWhitespace(trimmed, ref index, end);
                if (index >= end)
                    break;
                if (trimmed[index] != ',')
                    throw new FormatException("Unexpected trailing content.");
                index++;
            }

            return result;
        }

        private static DailyStats ReadObject(string json, ref int index, int end)
        {
            if (index >= end || json[index] != '{')
                throw new FormatException("Expected a JSON object.");
            index++;

            string date = null;
            int? attempts = null;
            int? totalSeconds = null;
            int? bestLevel = null;

            while (index < end)
            {
                SkipWhitespace(json, ref index, end);
                if (index < end && json[index] == '}')
                {
                    index++;
                    break;
                }

                var key = ReadString(json, ref index, end);
                SkipWhitespace(json, ref index, end);
                if (index >= end || json[index] != ':')
                    throw new FormatException("Expected ':'.");
                index++;
                SkipWhitespace(json, ref index, end);

                if (key == "date")
                    date = ReadString(json, ref index, end);
                else if (key == "attempts")
                    attempts = ReadInt(json, ref index, end);
                else if (key == "totalSeconds")
                    totalSeconds = ReadInt(json, ref index, end);
                else if (key == "bestLevel")
                    bestLevel = ReadInt(json, ref index, end);
                else
                    SkipValue(json, ref index, end);

                SkipWhitespace(json, ref index, end);
                if (index < end && json[index] == ',')
                {
                    index++;
                    continue;
                }

                if (index < end && json[index] == '}')
                {
                    index++;
                    break;
                }

                throw new FormatException("Malformed JSON object.");
            }

            if (date == null || !attempts.HasValue || !totalSeconds.HasValue || !bestLevel.HasValue)
                throw new FormatException("Missing daily stats field.");

            var parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return new DailyStats(parsedDate, attempts.Value, totalSeconds.Value, bestLevel.Value);
        }

        private static string ReadString(string json, ref int index, int end)
        {
            if (index >= end || json[index] != '"')
                throw new FormatException("Expected a JSON string.");
            index++;

            var start = index;
            while (index < end && json[index] != '"')
            {
                if (json[index] == '\\')
                    throw new FormatException("Escapes are not used in daily stats JSON.");
                index++;
            }

            if (index >= end)
                throw new FormatException("Unterminated JSON string.");

            var value = json.Substring(start, index - start);
            index++;
            return value;
        }

        private static int ReadInt(string json, ref int index, int end)
        {
            var start = index;
            if (index < end && json[index] == '-')
                index++;

            var digits = 0;
            while (index < end && json[index] >= '0' && json[index] <= '9')
            {
                index++;
                digits++;
            }

            if (digits == 0)
                throw new FormatException("Expected an integer.");

            return int.Parse(json.Substring(start, index - start), CultureInfo.InvariantCulture);
        }

        private static void SkipValue(string json, ref int index, int end)
        {
            if (index >= end)
                throw new FormatException("Unexpected end of JSON.");

            if (json[index] == '"')
            {
                ReadString(json, ref index, end);
                return;
            }

            if (json[index] == '-' || (json[index] >= '0' && json[index] <= '9'))
            {
                ReadInt(json, ref index, end);
                return;
            }

            throw new FormatException("Unsupported JSON value.");
        }

        private static void SkipWhitespace(string json, ref int index, int end)
        {
            while (index < end && char.IsWhiteSpace(json[index]))
                index++;
        }
    }
}
