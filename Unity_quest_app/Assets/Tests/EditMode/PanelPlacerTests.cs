using MindSilence.XR;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class PanelPlacerTests
    {
        [Test]
        public void Defaults_SitInFrontOfSeatedUser()
        {
            Assert.GreaterOrEqual(PanelPlacer.DistanceMetersDefault, 1.2f);
            Assert.LessOrEqual(PanelPlacer.DistanceMetersDefault, 1.5f);
            Assert.Greater(PanelPlacer.BelowEyesMetersDefault, 0f);
        }
    }
}
