using System;
using System.IO;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thetis;

namespace Thetis.Tests
{
    [TestClass]
    public class DatabaseTests
    {
        private string tempDbFile;

        [TestInitialize]
        public void Setup()
        {
            // Use a temporary file for the database test
            tempDbFile = Path.Combine(Path.GetTempPath(), $"thetis_test_db_{Guid.NewGuid()}.xml");
            DB.FileName = tempDbFile;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempDbFile))
            {
                try
                {
                    File.Delete(tempDbFile);
                }
                catch { }
            }
        }

        [TestMethod]
        public void TestDatabaseInitAndVerifyTables()
        {
            // Initialize database
            bool initResult = DB.Init();
            Assert.IsTrue(initResult, "DB.Init() should succeed for a new database file");

            // Verify core tables are created
            Assert.IsNotNull(DB.ds, "DataSet should be initialized");
            Assert.IsTrue(DB.ds.Tables.Contains("BandText"), "BandText table should exist");
            Assert.IsTrue(DB.ds.Tables.Contains("Memory"), "Memory table should exist");
            Assert.IsTrue(DB.ds.Tables.Contains("TXProfile"), "TXProfile table should exist");
            Assert.IsTrue(DB.ds.Tables.Contains("BandStack2Entries"), "BandStack2Entries table should exist");

            // Verify that we can write the database to disk
            DB.WriteDB();
            Assert.IsTrue(File.Exists(tempDbFile), "Database file should be written to disk");

            // Read it back and verify it loads
            bool reInitResult = DB.Init();
            Assert.IsTrue(reInitResult, "DB.Init() should succeed when loading an existing database file");
        }
    }
}
