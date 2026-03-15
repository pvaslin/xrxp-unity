using NUnit.Framework;
using UnityEditor;
using XRXP;
using XRXP.Editor;

namespace XRXP.Editor.Tests
{
    [TestFixture]
    public class XRXPMenuTests
    {
        private const string TestConfigPath = "Assets/XRXPConfig/XRXPConfig.asset";

        [TearDown]
        public void TearDown()
        {
            // Clean up test config if created
            if (AssetDatabase.LoadAssetAtPath<XRXPConfig>(TestConfigPath) != null)
            {
                AssetDatabase.DeleteAsset("Assets/XRXPConfig");
            }
        }

        [Test]
        public void SetupConfig_CreatesConfigAsset()
        {
            // Act - Using reflection to call private method
            var method = typeof(XRXPMenu).GetMethod("SetupConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method.Invoke(null, null);

            // Assert
            var config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(TestConfigPath);
            Assert.IsNotNull(config);
            Assert.IsInstanceOf<XRXPConfig>(config);
        }

        [Test]
        public void SetupConfig_CreatesConfigWithDefaultValues()
        {
            // Act
            var method = typeof(XRXPMenu).GetMethod("SetupConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method.Invoke(null, null);

            // Assert
            var config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(TestConfigPath);
            Assert.IsFalse(config.LocalStorageMode);
            Assert.IsTrue(config.OnlineMode);
            Assert.IsTrue(config.BackUpStorageMode);
            Assert.AreEqual("Experimentation", config.ExperimentID);
        }
    }

    [TestFixture]
    public class XRXPConfigEditorTests
    {
        [Test]
        public void XRXPConfig_IsScriptableObject()
        {
            var config = ScriptableObject.CreateInstance<XRXPConfig>();
            
            Assert.IsInstanceOf<UnityEngine.ScriptableObject>(config);
        }

        [Test]
        public void XRXPConfig_DefaultValues_AreSet()
        {
            var config = ScriptableObject.CreateInstance<XRXPConfig>();
            
            Assert.IsFalse(config.LocalStorageMode);
            Assert.IsTrue(config.OnlineMode);
            Assert.IsTrue(config.BackUpStorageMode);
            Assert.AreEqual("Experimentation", config.ExperimentID);
            Assert.IsNull(config.AuthorizationToken);
            Assert.IsNull(config.WebSocketServer);
            Assert.AreEqual("https://...", config.RESTServer);
            Assert.IsNull(config.FileServer);
        }
    }
}