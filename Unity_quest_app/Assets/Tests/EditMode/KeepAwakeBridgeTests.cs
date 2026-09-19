using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class KeepAwakeBridgeTests
    {
        [TearDown]
        public void TearDown()
        {
            KeepAwakeBridge.SetEnabled(false);
        }

        [Test]
        public void Enabled_SetsNeverSleep()
        {
            KeepAwakeBridge.SetEnabled(true);

            Assert.AreEqual(SleepTimeout.NeverSleep, Screen.sleepTimeout);
        }

        [Test]
        public void Disabled_RestoresSystemSetting()
        {
            KeepAwakeBridge.SetEnabled(true);

            KeepAwakeBridge.SetEnabled(false);

            Assert.AreEqual(SleepTimeout.SystemSetting, Screen.sleepTimeout);
        }
    }
}
