using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class XrUiButtonReadersTests
    {
        [Test]
        public void Names_IncludeSelectAndUiPress()
        {
            Assert.IsTrue(System.Array.IndexOf(XrUiButtonReaders.Names, "selectInput") >= 0);
            Assert.IsTrue(System.Array.IndexOf(XrUiButtonReaders.Names, "uiPressInput") >= 0);
        }

        [Test]
        public void TryQueue_TwoArgSignature()
        {
            var reader = new TwoArgReader();
            Assert.IsTrue(XrUiButtonReaders.TryQueue(reader, true));
            Assert.IsTrue(reader.Performed);
            Assert.AreEqual(1f, reader.Value);
            Assert.IsTrue(XrUiButtonReaders.TryQueue(reader, false));
            Assert.IsFalse(reader.Performed);
            Assert.AreEqual(0f, reader.Value);
        }

        [Test]
        public void SetManualMode_ParsesManualValue()
        {
            var reader = new ModeReader();
            Assert.IsTrue(XrUiButtonReaders.SetManualMode(reader));
            Assert.AreEqual(ModeReader.InputSourceMode.ManualValue, reader.inputSourceMode);
        }

        sealed class TwoArgReader
        {
            public bool Performed;
            public float Value;

            public void QueueManualState(bool performed, float value)
            {
                Performed = performed;
                Value = value;
            }
        }

        sealed class ModeReader
        {
            public enum InputSourceMode
            {
                InputActionReference,
                ManualValue,
            }

            public InputSourceMode inputSourceMode { get; set; }
        }
    }
}
