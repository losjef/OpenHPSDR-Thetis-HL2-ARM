using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thetis;

namespace Thetis.Tests
{
    [TestClass]
    public class WdspTests
    {
        [DllImport("wdsp.dll", EntryPoint = "GetWDSPVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWDSPVersion();

        [DllImport("ChannelMaster.dll", EntryPoint = "GetCMVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCMVersion();

        [TestMethod]
        public void TestGetWdspVersion()
        {
            try
            {
                int version = GetWDSPVersion();
                System.Console.WriteLine($"WDSP Version: {version}");
                Assert.AreEqual(129, version, "WDSP version should be 129");
            }
            catch (DllNotFoundException ex)
            {
                Assert.Fail($"Failed to load wdsp.dll or one of its dependencies (e.g. libfftw3-3.dll). Error: {ex.Message}");
            }
        }

        [TestMethod]
        public void TestGetCmVersion()
        {
            try
            {
                int version = GetCMVersion();
                System.Console.WriteLine($"ChannelMaster Version: {version}");
                Assert.IsTrue(version > 0, "ChannelMaster version should be greater than 0");
            }
            catch (DllNotFoundException ex)
            {
                Assert.Fail($"Failed to load ChannelMaster.dll. Error: {ex.Message}");
            }
        }

        [TestMethod]
        public void TestCmLoadRouterAllHermesLite()
        {
            try
            {
                // Initialize the channel master memory structures first to avoid AccessViolationException
                cmaster.CMCreateCMaster();

                // Set necessary static fields/properties for HPSDRModel.HERMESLITE using Reflection
                // to avoid calling the property setters which trigger uninitialized NetworkIO calls.
                var modelField = typeof(HardwareSpecific).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                Assert.IsNotNull(modelField, "Private field _model should exist on HardwareSpecific class");
                modelField.SetValue(null, HPSDRModel.HERMESLITE);

                var hwField = typeof(HardwareSpecific).GetField("_hardware", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                Assert.IsNotNull(hwField, "Private field _hardware should exist on HardwareSpecific class");
                hwField.SetValue(null, HPSDRHW.HermesLite);

                NetworkIO.CurrentRadioProtocol = RadioProtocol.ETH;

                // Load router configuration for HERMESLITE - this performs P/Invoke calls
                cmaster.CMLoadRouterAll(HPSDRModel.HERMESLITE);
            }
            catch (Exception ex)
            {
                Assert.Fail($"CMLoadRouterAll for HERMESLITE failed with exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [TestMethod]
        public void TestSetupFixedTunePowerClamping()
        {
            // Instantiate Setup using null for console to avoid running complex constructors or setting Owner on uninitialized Form
            Setup setup = new Setup(null);

            // Set the private 'initializing' field to true to prevent event handlers from executing console logic
            var initializingField = typeof(Setup).GetField("initializing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(initializingField, "Private field initializing should exist on Setup class");
            initializingField.SetValue(setup, true);

            // Access the private field udTXTunePower using Reflection
            var fieldInfo = typeof(Setup).GetField("udTXTunePower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo, "Private field udTXTunePower should exist on Setup class");
            
            var udTXTunePower = (System.Windows.Forms.NumericUpDown)fieldInfo.GetValue(setup);
            Assert.IsNotNull(udTXTunePower, "udTXTunePower instance should not be null");

            // Access private fields of HardwareSpecific using Reflection to bypass property setter P/Invokes
            var modelField = typeof(HardwareSpecific).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(modelField, "Private field _model should exist on HardwareSpecific class");

            var hwField = typeof(HardwareSpecific).GetField("_hardware", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(hwField, "Private field _hardware should exist on HardwareSpecific class");

            // Test Case 1: Standard model (e.g. HERMES)
            modelField.SetValue(null, HPSDRModel.HERMES);
            hwField.SetValue(null, HPSDRHW.Hermes);
            udTXTunePower.Minimum = 0;
            udTXTunePower.Maximum = 100;

            // Set FixedTunePower to a standard value
            setup.FixedTunePower = 50;
            Assert.AreEqual(50, setup.FixedTunePower, "FixedTunePower should return the assigned value when within bounds");

            // Set FixedTunePower to a value exceeding maximum (100) -> should clamp to 100
            setup.FixedTunePower = 150;
            Assert.AreEqual(100, (int)udTXTunePower.Value, "udTXTunePower.Value should be clamped to 100");
            Assert.AreEqual(100, setup.FixedTunePower, "FixedTunePower getter should return the clamped maximum");

            // Set FixedTunePower to a value below minimum (0) -> should clamp to 0
            setup.FixedTunePower = -10;
            Assert.AreEqual(0, (int)udTXTunePower.Value, "udTXTunePower.Value should be clamped to 0");
            Assert.AreEqual(0, setup.FixedTunePower, "FixedTunePower getter should return the clamped minimum");

            // Test Case 2: HERMESLITE model
            modelField.SetValue(null, HPSDRModel.HERMESLITE);
            hwField.SetValue(null, HPSDRHW.HermesLite);
            udTXTunePower.Minimum = -16.5m;
            udTXTunePower.Maximum = 0;

            // In HERMESLITE, targetVal = (value/3 - 33)/2. 
            // If value = 90 -> (90/3 - 33)/2 = (30 - 33)/2 = -1.5. This is within [-16.5, 0].
            setup.FixedTunePower = 90;
            Assert.AreEqual(-1.5m, udTXTunePower.Value, "udTXTunePower.Value should be set to -1.5 for input 90");

            // If value = 150 -> (150/3 - 33)/2 = 8.5 -> exceeds maximum (0) -> should clamp to 0
            setup.FixedTunePower = 150;
            Assert.AreEqual(0m, udTXTunePower.Value, "udTXTunePower.Value should clamp to 0 for input 150");

            // If value = -30 -> (-30/3 - 33)/2 = -21.5 -> below minimum (-16.5) -> should clamp to -16.5
            setup.FixedTunePower = -30;
            Assert.AreEqual(-16.5m, udTXTunePower.Value, "udTXTunePower.Value should clamp to -16.5 for input -30");
        }
    }
}

