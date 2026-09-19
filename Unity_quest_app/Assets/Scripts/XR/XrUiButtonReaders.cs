using System;
using UnityEngine;

namespace MindSilence.XR
{
    /// <summary>
    /// XRI 3 UI clicks use <c>uiPressInput</c>, not <c>selectInput</c>.
    /// Both readers must be Manual and receive the same press.
    /// </summary>
    public static class XrUiButtonReaders
    {
        public static readonly string[] Names =
        {
            "selectInput",
            "m_SelectInput",
            "uiPressInput",
            "m_UIPressInput",
        };

        public static int ConfigureManual(Component interactor)
        {
            if (interactor == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var name in Names)
            {
                var reader = GetMember(interactor, name);
                if (reader != null && SetManualMode(reader))
                {
                    count++;
                }
            }

            return count;
        }

        public static int Queue(Component interactor, bool pressed)
        {
            if (interactor == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var name in Names)
            {
                var reader = GetMember(interactor, name);
                if (reader != null && TryQueue(reader, pressed))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool SetManualMode(object reader)
        {
            if (reader == null)
            {
                return false;
            }

            var modeProperty = reader.GetType().GetProperty("inputSourceMode")
                ?? reader.GetType().GetProperty("InputSourceMode");
            if (modeProperty == null || !modeProperty.PropertyType.IsEnum)
            {
                return false;
            }

            try
            {
                modeProperty.SetValue(reader, Enum.Parse(modeProperty.PropertyType, "ManualValue"));
                return true;
            }
            catch (ArgumentException)
            {
                try
                {
                    modeProperty.SetValue(reader, Enum.Parse(modeProperty.PropertyType, "Manual"));
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
        }

        public static bool TryQueue(object reader, bool pressed)
        {
            if (reader == null)
            {
                return false;
            }

            var type = reader.GetType();
            var two = type.GetMethod("QueueManualState", new[] { typeof(bool), typeof(float) });
            if (two != null)
            {
                two.Invoke(reader, new object[] { pressed, pressed ? 1f : 0f });
                return true;
            }

            var four = type.GetMethod(
                "QueueManualState",
                new[] { typeof(bool), typeof(float), typeof(bool), typeof(bool) });
            if (four != null)
            {
                four.Invoke(reader, new object[] { pressed, pressed ? 1f : 0f, pressed, !pressed });
                return true;
            }

            return false;
        }

        public static object GetMember(object target, string name)
        {
            if (target == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            var property = target.GetType().GetProperty(name);
            if (property != null)
            {
                return property.GetValue(target);
            }

            var field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(target);
        }
    }
}
