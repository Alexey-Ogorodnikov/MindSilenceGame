using MindSilence.XR;
using NUnit.Framework;
using UnityEngine;

namespace MindSilence.Tests.EditMode
{
    public sealed class InputModalityGateTests
    {
        GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("InputRoot");
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [Test]
        public void Apply_TogglesPokeAndRayHosts()
        {
            var leftPoke = Child(UiInteractionRig.LeftPokeName);
            var rightRay = Child(UiInteractionRig.RightRayName);
            var leftHandRay = Child(UiInteractionRig.LeftHandRayName);
            var gate = _root.AddComponent<InputModalityGate>();

            gate.Apply(new InputModalityDecision(
                leftPoke: false,
                rightPoke: false,
                leftRay: false,
                rightRay: true,
                leftHandVisual: false,
                rightHandVisual: false,
                leftControllerVisual: false,
                rightControllerVisual: true));

            Assert.IsFalse(leftPoke.activeSelf);
            Assert.IsTrue(rightRay.activeSelf);
            Assert.IsFalse(leftHandRay.activeSelf);

            gate.Apply(new InputModalityDecision(
                leftPoke: true,
                rightPoke: false,
                leftRay: false,
                rightRay: false,
                leftHandVisual: true,
                rightHandVisual: false,
                leftControllerVisual: false,
                rightControllerVisual: false));

            Assert.IsTrue(leftPoke.activeSelf);
            Assert.IsTrue(leftHandRay.activeSelf);
            Assert.IsFalse(rightRay.activeSelf);
        }

        [Test]
        public void Apply_HidesHintedHandMeshWhenConfidenceLost()
        {
            var hand = Child("LeftHandAnchor");
            var mesh = new GameObject("OVRHandMesh");
            mesh.transform.SetParent(hand.transform, false);
            mesh.AddComponent<MeshFilter>();
            var renderer = mesh.AddComponent<MeshRenderer>();
            renderer.enabled = true;
            var gate = _root.AddComponent<InputModalityGate>();

            gate.Apply(new InputModalityDecision(
                leftPoke: false,
                rightPoke: false,
                leftRay: false,
                rightRay: false,
                leftHandVisual: false,
                rightHandVisual: false,
                leftControllerVisual: false,
                rightControllerVisual: false));

            Assert.IsFalse(renderer.enabled);
        }

        GameObject Child(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            return go;
        }
    }
}
