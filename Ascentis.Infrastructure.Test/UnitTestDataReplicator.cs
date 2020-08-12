﻿using System.Data.SqlClient;
using Ascentis.Infrastructure.DataPipeline.SourceAdapter.Sql.SqlClient;
using Ascentis.Infrastructure.DataReplicator.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestAsyncDisposer
{
    [TestClass]
    public class UnitTestDataReplicator
    {
        [TestMethod]
        public void TestReadMetadata()
        {
            using var replicator = new SQLiteDataReplicator(
                "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                "Data Source=inmemorydb;mode=memory;cache=shared;journal_mode=WAL;") {ParallelismLevel = 2};
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.AddSourceTable("", "SELECT @@VERSION AS 'SQL Server Version'");
            replicator.Prepare<SqlCommand, SqlConnection>();
            Assert.AreEqual(6, replicator.ColumnMetadataLists.Length);
            foreach (var metaList in replicator.ColumnMetadataLists)
            {
                Assert.AreNotEqual(0, metaList.Count);
                Assert.AreEqual(typeof(string), metaList[0].DataType);
            }
            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestBasicReplicate()
        {
            using var replicator = new SQLiteDataReplicator(
                "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SITES", "SELECT * FROM SITES");
            replicator.AddSourceTable("TIME", "SELECT TOP 10000 * FROM TIME");
            replicator.AddSourceTable("A_TIMESHEET", "SELECT TOP 10000 * FROM A_TIMESHEET");
            replicator.AddSourceTable("A_SCHEDULE", "SELECT TOP 10000 * FROM A_SCHEDULE");
            replicator.AddSourceTable("PM_DIST", "SELECT TOP 10000 * FROM PM_DIST");
            replicator.AddSourceTable("PM_LOG", "SELECT TOP 10000 * FROM PM_LOG");
            replicator.AddSourceTable("AUDITLOG", "SELECT TOP 10000 * FROM AUDITLOG");
            replicator.AddSourceTable("APPROVPR", "SELECT TOP 10000 * FROM APPROVPR");
            replicator.ForceDropTable = true;
            replicator.Prepare<SqlCommand, SqlConnection>();
            replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestReplicateMultipleRounds()
        {
            using var replicator = new SQLiteDataReplicator(
                    "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                    "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;Pooling=True;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SITES", "SELECT * FROM SITES");
            replicator.AddSourceTable("TIME", "SELECT TOP 1000 * FROM TIME");
            replicator.AddSourceTable("A_TIMESHEET", "SELECT TOP 1000 * FROM A_TIMESHEET");
            replicator.AddSourceTable("A_SCHEDULE", "SELECT TOP 1000 * FROM A_SCHEDULE");
            replicator.AddSourceTable("PM_DIST", "SELECT TOP 1000 * FROM PM_DIST", new []
            {
                "CREATE INDEX PM_DIST_CEMPID ON PM_DIST(CEMPID)"
            });
            replicator.AddSourceTable("PM_LOG", "SELECT TOP 1000 * FROM PM_LOG");
            replicator.AddSourceTable("AUDITLOG", "SELECT TOP 1000 * FROM AUDITLOG");
            replicator.AddSourceTable("APPROVPR", "SELECT TOP 1000 * FROM APPROVPR");
            replicator.Prepare<SqlCommand, SqlConnection>();
            for (var i = 0; i < 40; i++)
                replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            replicator.UnPrepare();
        }

        [TestMethod]
        public void TestBasicReplicateWithoutTransactions()
        {
            using var replicator = new SQLiteDataReplicator(
                    "Server=vm-pc-sql02;Database=NEU14270_200509_Seba;Trusted_Connection=True;",
                    "Data Source=inmemorydb;mode=memory;cache=shared;synchronous=Off;")
            { ParallelismLevel = 2 };
            replicator.AddSourceTable("SITES", "SELECT * FROM SITES");
            replicator.UseTransaction = false;
            replicator.Prepare<SqlCommand, SqlConnection>();
            replicator.Replicate<SqlClientSourceAdapter>(1000, 1);
            replicator.UnPrepare();
        }
    }
}
