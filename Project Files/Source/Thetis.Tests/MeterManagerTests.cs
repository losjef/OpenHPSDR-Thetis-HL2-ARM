using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thetis;

namespace Thetis.Tests
{
    [TestClass]
    public class MeterManagerTests
    {
        private ConcurrentDictionary<string, string>? _originalCatVariables;
        private FieldInfo? _catVariablesField;

        [TestInitialize]
        public void Setup()
        {
            _catVariablesField = typeof(MeterManager).GetField("_cat_variables", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(_catVariablesField, "_cat_variables field not found.");

            // Save the original dictionary
            _originalCatVariables = _catVariablesField.GetValue(null) as ConcurrentDictionary<string, string>;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Restore the original dictionary
            if (_catVariablesField != null)
            {
                _catVariablesField.SetValue(null, _originalCatVariables);
            }
        }

        [TestMethod]
        public void CatVariables_ReturnsKeysFromDictionary()
        {
            var dict = new ConcurrentDictionary<string, string>();
            dict.TryAdd("var1", "value1");
            dict.TryAdd("var2", "value2");
            dict.TryAdd("var3", "value3");

            _catVariablesField!.SetValue(null, dict);

            // Act
            List<string> result = MeterManager.CatVariables();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains("var1"));
            Assert.IsTrue(result.Contains("var2"));
            Assert.IsTrue(result.Contains("var3"));
        }

        [TestMethod]
        public void CatVariables_ReturnsEmptyList_WhenDictionaryIsEmpty()
        {
            var dict = new ConcurrentDictionary<string, string>();
            _catVariablesField!.SetValue(null, dict);

            // Act
            List<string> result = MeterManager.CatVariables();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
