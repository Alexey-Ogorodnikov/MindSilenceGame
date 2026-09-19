using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class SideloadManifestTests
    {
        static string ManifestPath =>
            Path.Combine(Application.dataPath, "Plugins", "Android", "AndroidManifest.xml");

        static string PackagesManifestPath =>
            Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        [Test]
        public void PathA_RemovesInternetAndNetworkState()
        {
            var xml = File.ReadAllText(ManifestPath);
            Assert.IsFalse(HasBarePermission(xml, "android.permission.INTERNET"));
            Assert.IsFalse(HasBarePermission(xml, "android.permission.ACCESS_NETWORK_STATE"));
            Assert.IsTrue(xml.Contains("tools:node=\"remove\""));
            Assert.IsTrue(xml.Contains("android.permission.INTERNET"));
        }

        [Test]
        public void PathA_HasNoCameraOrMicrophone()
        {
            var xml = File.ReadAllText(ManifestPath);
            Assert.IsFalse(xml.Contains("android.permission.CAMERA"));
            Assert.IsFalse(xml.Contains("android.permission.RECORD_AUDIO"));
            Assert.IsFalse(xml.Contains("horizonos.permission.HEADSET_CAMERA"));
        }

        [Test]
        public void PathA_DeclaresHandTrackingAndNoBackup()
        {
            var xml = File.ReadAllText(ManifestPath);
            Assert.IsTrue(xml.Contains("oculus.software.handtracking"));
            Assert.IsTrue(xml.Contains("com.oculus.permission.HAND_TRACKING"));
            Assert.IsTrue(xml.Contains("android:allowBackup=\"false\""));
        }

        [Test]
        public void PathA_DoesNotReferencePlatformSdk()
        {
            var packages = File.ReadAllText(PackagesManifestPath);
            Assert.IsFalse(packages.Contains("com.meta.xr.sdk.platform"));
            Assert.IsFalse(packages.Contains("com.meta.xr.sdk.voice"));
        }

        static bool HasBarePermission(string xml, string permission)
        {
            foreach (Match match in Regex.Matches(xml, "<uses-permission[^/]*/>"))
            {
                if (match.Value.IndexOf(permission, System.StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                if (match.Value.IndexOf("tools:node=\"remove\"", System.StringComparison.Ordinal) < 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
