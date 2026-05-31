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

        [TestMethod]
        public void TestSafeSetBoundsBandGainTransitions()
        {
            Setup setup = new Setup(null);

            // Access the private field nud160M using Reflection
            var fieldInfo = typeof(Setup).GetField("nud160M", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo, "Private field nud160M should exist on Setup class");
            
            var nud160M = (System.Windows.Forms.NumericUpDown)fieldInfo.GetValue(setup);
            Assert.IsNotNull(nud160M, "nud160M instance should not be null");

            // Access SafeSetBounds private method on Setup using Reflection
            var safeSetBoundsMethod = typeof(Setup).GetMethod("SafeSetBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(safeSetBoundsMethod, "Private method SafeSetBounds should exist on Setup class");

            // 1. Initial State: Non-HL2 default bounds (Minimum 38.8, Maximum 100, Value e.g. 50)
            safeSetBoundsMethod.Invoke(setup, new object[] { nud160M, 38.8m, 100m, 1, 0.1m });
            Assert.AreEqual(38.8m, nud160M.Minimum, "Minimum should be set to 38.8");
            Assert.AreEqual(100m, nud160M.Maximum, "Maximum should be set to 100");

            // Set value to 50
            nud160M.Value = 50m;
            Assert.AreEqual(50m, nud160M.Value, "Value should be set to 50");

            // 2. Transition to HERMESLITE: Minimum 0, Maximum 100, Value should remain 50
            safeSetBoundsMethod.Invoke(setup, new object[] { nud160M, 0m, 100m, 0, 1m });
            Assert.AreEqual(0m, nud160M.Minimum, "HERMESLITE minimum should be 0");
            Assert.AreEqual(50m, nud160M.Value, "Value should remain 50 during transition");

            // Change Value to 10 (which is < 38.8, valid for HERMESLITE)
            nud160M.Value = 10m;
            Assert.AreEqual(10m, nud160M.Value, "Value should be updated to 10");

            // 3. Transition back to Non-HL2 defaults: Minimum 38.8.
            // Under unsafe direct assignment this would crash (value 10 < new minimum 38.8).
            // SafeSetBounds must clamp the Value first, then update Minimum.
            safeSetBoundsMethod.Invoke(setup, new object[] { nud160M, 38.8m, 100m, 1, 0.1m });
            Assert.AreEqual(38.8m, nud160M.Minimum, "Minimum should safely transition back to 38.8");
            Assert.AreEqual(38.8m, nud160M.Value, "Value 10 should be safely clamped to the new minimum 38.8");
        }

        [TestMethod]
        public void TestSetupRadioModelTransitionToHermesLite()
        {
            // Initialize ChannelMaster memory structures first
            cmaster.CMCreateCMaster();

            // Initialize MeterManager arrays
            var initAntennaArraysMethod = typeof(MeterManager).GetMethod("initAntennaArrays", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (initAntennaArraysMethod != null)
            {
                initAntennaArraysMethod.Invoke(null, null);
            }

            // 1. Instantiate Console using FormatterServices to bypass its constructor
            var console = (Console)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Console));
            
            // Initialize Control native window to avoid InvokeRequired NullReferenceException
            var controlNativeWindowType = typeof(System.Windows.Forms.Control).GetNestedType("ControlNativeWindow", System.Reflection.BindingFlags.NonPublic);
            if (controlNativeWindowType != null)
            {
                var ctor = controlNativeWindowType.GetConstructor(
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new Type[] { typeof(System.Windows.Forms.Control) },
                    null);
                if (ctor != null)
                {
                    var windowInstance = ctor.Invoke(new object[] { console });
                    var windowField = typeof(System.Windows.Forms.Control).GetField("_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (windowField != null)
                    {
                        windowField.SetValue(console, windowInstance);
                    }
                }
            }

            // Set console property/field on static classes
            Audio.console = console;
            
            // Instantiate and set mock Radio/DSP objects
            var radio = (Radio)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Radio));
            console.radio = radio;
            
            var dsp_rx = new RadioDSPRX[2][];
            dsp_rx[0] = new RadioDSPRX[2];
            dsp_rx[0][0] = (RadioDSPRX)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RadioDSPRX));
            dsp_rx[0][1] = (RadioDSPRX)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RadioDSPRX));
            dsp_rx[1] = new RadioDSPRX[2];
            dsp_rx[1][0] = (RadioDSPRX)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RadioDSPRX));
            dsp_rx[1][1] = (RadioDSPRX)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RadioDSPRX));
            
            var dsp_rxField = typeof(Radio).GetField("dsp_rx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dsp_rxField != null)
                dsp_rxField.SetValue(radio, dsp_rx);

            var dsp_tx = new RadioDSPTX[1];
            dsp_tx[0] = (RadioDSPTX)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RadioDSPTX));
            var dsp_txField = typeof(Radio).GetField("dsp_tx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dsp_txField != null)
                dsp_txField.SetValue(radio, dsp_tx);

            // Instantiate and set mock XVTRForm object
            var xvtrForm = (XVTRForm)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(XVTRForm));
            console.XVTRForm = xvtrForm;
            
            var enabledField = typeof(XVTRForm).GetField("enabled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enabledField != null)
            {
                var enabledArr = new CheckBoxTS[16];
                for (int i = 0; i < 16; i++)
                {
                    enabledArr[i] = (CheckBoxTS)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(CheckBoxTS));
                }
                enabledField.SetValue(xvtrForm, enabledArr);
            }

            var psform = (PSForm)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(PSForm));
            console.psform = psform;

            // Initialize Control native window on psform to avoid InvokeRequired NullReferenceException
            if (controlNativeWindowType != null)
            {
                var ctor = controlNativeWindowType.GetConstructor(
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new Type[] { typeof(System.Windows.Forms.Control) },
                    null);
                if (ctor != null)
                {
                    var windowInstance = ctor.Invoke(new object[] { psform });
                    var windowField = typeof(System.Windows.Forms.Control).GetField("_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (windowField != null)
                    {
                        windowField.SetValue(psform, windowInstance);
                    }
                }
            }

            InitializeControlFields(psform);
            
            // 2. Initialize arrays on console
            int modelLast = (int)HPSDRModel.LAST;
            int bandLast = (int)Band.LAST;
            
            var rx1_preamp_by_bandField = typeof(Console).GetField("rx1_preamp_by_band", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rx1_preamp_by_bandField != null)
                rx1_preamp_by_bandField.SetValue(console, new PreampMode[bandLast]);
            
            var rx2_preamp_by_bandField = typeof(Console).GetField("rx2_preamp_by_band", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rx2_preamp_by_bandField != null)
                rx2_preamp_by_bandField.SetValue(console, new PreampMode[bandLast]);
            
            var rx_meter_cal_offset_by_radioField = typeof(Console).GetField("rx_meter_cal_offset_by_radio", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rx_meter_cal_offset_by_radioField != null)
                rx_meter_cal_offset_by_radioField.SetValue(console, new float[modelLast]);
            
            var rx_display_cal_offset_by_radioField = typeof(Console).GetField("rx_display_cal_offset_by_radio", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rx_display_cal_offset_by_radioField != null)
                rx_display_cal_offset_by_radioField.SetValue(console, new float[modelLast]);

            var rx1_preamp_offsetField = typeof(Console).GetField("rx1_preamp_offset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rx1_preamp_offsetField != null)
                rx1_preamp_offsetField.SetValue(console, new float[(int)PreampMode.LAST]);

            var rx2_preamp_offsetField = typeof(Console).GetField("rx2_preamp_offset", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (rx2_preamp_offsetField != null)
                rx2_preamp_offsetField.SetValue(console, new float[(int)PreampMode.LAST]);

            var rx1_step_attenuator_by_bandField = typeof(Console).GetField("rx1_step_attenuator_by_band", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rx1_step_attenuator_by_bandField != null)
                rx1_step_attenuator_by_bandField.SetValue(console, new int[bandLast]);
            
            var rx2_step_attenuator_by_bandField = typeof(Console).GetField("rx2_step_attenuator_by_band", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rx2_step_attenuator_by_bandField != null)
                rx2_step_attenuator_by_bandField.SetValue(console, new int[bandLast]);

            var tx_step_attenuator_by_bandField = typeof(Console).GetField("tx_step_attenuator_by_band", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tx_step_attenuator_by_bandField != null)
                tx_step_attenuator_by_bandField.SetValue(console, new int[bandLast]);

            var _from_preampmodeField = typeof(Console).GetField("_from_preampmode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (_from_preampmodeField != null)
                _from_preampmodeField.SetValue(console, new bool[2]);

            var _from_attenuatordataField = typeof(Console).GetField("_from_attenuatordata", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (_from_attenuatordataField != null)
                _from_attenuatordataField.SetValue(console, new bool[2]);

            var _general_settingsField = typeof(Console).GetField("_general_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (_general_settingsField != null)
                _general_settingsField.SetValue(console, new System.Collections.Generic.Dictionary<OtherButtonId, bool>[2]);

            var m_objSetupFormLockerField = typeof(Console).GetField("m_objSetupFormLocker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (m_objSetupFormLockerField != null)
                m_objSetupFormLockerField.SetValue(console, new object());

            var _findPeakLockField = typeof(Console).GetField("_findPeakLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (_findPeakLockField != null)
                _findPeakLockField.SetValue(console, new object());

            var separatorField = typeof(Console).GetField("separator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (separatorField != null)
                separatorField.SetValue(console, ".");

            // Initialize Andromeda antenna arrays
            var AntennaArrayByBandField = typeof(Console).GetField("AntennaArrayByBand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (AntennaArrayByBandField != null)
                AntennaArrayByBandField.SetValue(console, new int[12] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            var RXAntennaArrayByBandField = typeof(Console).GetField("RXAntennaArrayByBand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (RXAntennaArrayByBandField != null)
                RXAntennaArrayByBandField.SetValue(console, new int[12] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            var RXAuxAntennaArrayByBandField = typeof(Console).GetField("RXAuxAntennaArrayByBand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (RXAuxAntennaArrayByBandField != null)
                RXAuxAntennaArrayByBandField.SetValue(console, new bool[12]);

            var RXAntennaNameArrayByBandField = typeof(Console).GetField("RXAntennaNameArrayByBand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (RXAntennaNameArrayByBandField != null)
                RXAntennaNameArrayByBandField.SetValue(console, new string[12] { "-", "-", "-", "-", "-", "-", "-", "-", "-", "-", "-", "-" });

            var on_off_preamp_settingsField = typeof(Console).GetField("on_off_preamp_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (on_off_preamp_settingsField != null)
                on_off_preamp_settingsField.SetValue(console, new string[] { "on", "off" });
            
            var anan100d_preamp_settingsField = typeof(Console).GetField("anan100d_preamp_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (anan100d_preamp_settingsField != null)
                anan100d_preamp_settingsField.SetValue(console, new string[] { "0dB", "-10dB", "-20dB", "-30dB" });
                
            var alex_preamp_settingsField = typeof(Console).GetField("alex_preamp_settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (alex_preamp_settingsField != null)
                alex_preamp_settingsField.SetValue(console, new string[] { "-10db", "-20db", "-30db", "-40db", "-50db" });
            
            // 3. Initialize Control fields recursively on console
            InitializeControlFields(console);
            
            // Add a default item to comboMeterTXMode so that Insert(1, ...) does not throw ArgumentOutOfRangeException
            var comboMeterTXModeField = typeof(Console).GetField("comboMeterTXMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (comboMeterTXModeField != null)
            {
                var comboMeterTXMode = (System.Windows.Forms.ComboBox)comboMeterTXModeField.GetValue(console);
                if (comboMeterTXMode != null)
                {
                    comboMeterTXMode.Items.Add("Off");
                }
            }
            
            // 4. Instantiate Setup form with null Console (assign it via reflection afterwards to avoid set_Owner NRE)
            Setup setup = new Setup(null);
            
            // Set fields on Setup that might be referenced
            var setupConsoleField = typeof(Setup).GetField("console", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (setupConsoleField != null)
                setupConsoleField.SetValue(setup, console);
            
            var m_frmSetupFormField = typeof(Console).GetField("m_frmSetupForm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (m_frmSetupFormField != null)
                m_frmSetupFormField.SetValue(console, setup);

            // Populate comboRadioModel
            var comboField = typeof(Setup).GetField("comboRadioModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(comboField, "Private field comboRadioModel should exist on Setup class");
            var comboRadioModel = (System.Windows.Forms.ComboBox)comboField.GetValue(setup);
            Assert.IsNotNull(comboRadioModel, "comboRadioModel instance should not be null");

            comboRadioModel.Items.Clear();
            comboRadioModel.Items.Add("HERMES");
            comboRadioModel.Items.Add("Hermes Lite");
            comboRadioModel.Text = "HERMES";
            
            // Trigger setupADCRadioButtons so controls are bound
            var setupADCRadioButtonsMethod = typeof(Setup).GetMethod("setupADCRadioButtions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (setupADCRadioButtonsMethod != null)
                setupADCRadioButtonsMethod.Invoke(setup, null);
            
            // Disable 'initializing' flag so IndexChanged executes
            var setupInitializingField = typeof(Setup).GetField("initializing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (setupInitializingField != null)
                setupInitializingField.SetValue(setup, false);
            
            // Get references to the NumericUpDown controls to verify bounds and values
            var udTXTunePowerField = typeof(Setup).GetField("udTXTunePower", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nud160MField = typeof(Setup).GetField("nud160M", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var udATTOnTXField = typeof(Setup).GetField("udATTOnTX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(udTXTunePowerField);
            Assert.IsNotNull(nud160MField);
            Assert.IsNotNull(udATTOnTXField);

            var udTXTunePower = (System.Windows.Forms.NumericUpDown)udTXTunePowerField.GetValue(setup);
            var nud160M = (System.Windows.Forms.NumericUpDown)nud160MField.GetValue(setup);
            var udATTOnTX = (System.Windows.Forms.NumericUpDown)udATTOnTXField.GetValue(setup);

            Assert.IsNotNull(udTXTunePower);
            Assert.IsNotNull(nud160M);
            Assert.IsNotNull(udATTOnTX);

            // Set initial values while in HERMES model
            udTXTunePower.Value = 50m;
            udATTOnTX.Value = 10m;
            nud160M.Value = 50m;

            // Trigger the transition to Hermes Lite
            try
            {
                comboRadioModel.Text = "Hermes Lite";
            }
            catch (Exception ex)
            {
                Assert.Fail($"Transition to Hermes Lite failed with exception: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            }

            // Verify Hermes Lite states and clamped values
            Assert.AreEqual(HPSDRModel.HERMESLITE, HardwareSpecific.Model);
            Assert.AreEqual(-16.5m, udTXTunePower.Minimum);
            Assert.AreEqual(0m, udTXTunePower.Maximum);
            Assert.AreEqual(0m, udTXTunePower.Value); // 50m clamped to new max 0m
            
            Assert.AreEqual(-28m, udATTOnTX.Minimum);
            Assert.AreEqual(10m, udATTOnTX.Value); // 10m is within [-28m, 31m]

            Assert.AreEqual(0m, nud160M.Minimum);
            Assert.AreEqual(100m, nud160M.Maximum);
            Assert.AreEqual(50m, nud160M.Value);

            // Now, set values in Hermes Lite that would be out of bounds in HERMES
            nud160M.Value = 10m; // Under HERMES, minimum is 38.8m
            udTXTunePower.Value = -10m; // Under HERMES, minimum is 0m

            // Trigger transition back to HERMES
            try
            {
                comboRadioModel.Text = "HERMES";
            }
            catch (Exception ex)
            {
                Assert.Fail($"Transition back to HERMES failed with exception: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            }

            // Verify HERMES states and clamped values
            Assert.AreEqual(HPSDRModel.HERMES, HardwareSpecific.Model);
            Assert.AreEqual(0m, udTXTunePower.Minimum);
            Assert.AreEqual(100m, udTXTunePower.Maximum);
            Assert.AreEqual(0m, udTXTunePower.Value); // -10m clamped to new min 0m

            Assert.AreEqual(0m, udATTOnTX.Minimum);
            Assert.AreEqual(10m, udATTOnTX.Value); // 10m is within [0m, 31m]

            Assert.AreEqual(38.8m, nud160M.Minimum);
            Assert.AreEqual(100m, nud160M.Maximum);
            Assert.AreEqual(38.8m, nud160M.Value); // 10m clamped to new min 38.8m
        }

        [TestMethod]
        public void TestUninitializedControlInvokeRequired()
        {
            var uninitCtrl = (System.Windows.Forms.Control)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(System.Windows.Forms.Control));
            
            var controlNativeWindowType = typeof(System.Windows.Forms.Control).GetNestedType("ControlNativeWindow", System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(controlNativeWindowType, "ControlNativeWindow type should exist");
            
            var ctor = controlNativeWindowType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new Type[] { typeof(System.Windows.Forms.Control) },
                null);
            Assert.IsNotNull(ctor, "ControlNativeWindow constructor should exist");
            
            var windowInstance = ctor.Invoke(new object[] { uninitCtrl });
            var windowField = typeof(System.Windows.Forms.Control).GetField("_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(windowField, "_window field should exist");
            windowField.SetValue(uninitCtrl, windowInstance);
            
            try
            {
                bool req = uninitCtrl.InvokeRequired;
                System.Console.WriteLine($"InvokeRequired returned: {req}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"InvokeRequired crashed: {ex.ToString()}");
            }
        }
        
        private static void InitializeControlFields(object obj)
        {
            if (obj == null) return;
            var type = obj.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(System.Windows.Forms.Control)) || field.FieldType.IsSubclassOf(typeof(System.Windows.Forms.ToolStripItem)))
                {
                    if (field.GetValue(obj) == null)
                    {
                        try
                        {
                            var control = Activator.CreateInstance(field.FieldType);
                            field.SetValue(obj, control);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}

