using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                Assert.IsGreaterThan(0, version, "ChannelMaster version should be greater than 0");
            }
            catch (DllNotFoundException ex)
            {
                Assert.Fail($"Failed to load ChannelMaster.dll. Error: {ex.Message}");
            }
        }
    }
}
