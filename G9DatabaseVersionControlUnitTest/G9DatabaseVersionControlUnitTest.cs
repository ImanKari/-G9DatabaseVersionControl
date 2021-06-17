using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using G9DatabaseVersionControlCore;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using G9DatabaseVersionControlCore.SqlServer;
using NUnit.Framework;

namespace G9DatabaseVersionControlUnitTest
{
    public class G9DatabaseVersionControlUnitTest
    {
        private readonly string _databaseName = "MDMWaterSmartMeter";

        private readonly string _dataSource = @"GAM3R-IK\MSSQLSERVER2019";
        private readonly string _password = "Pass1234";
        private readonly string _userId = "sa";
        private IList<string> _projectNames;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Order(1)]
        public void TestSmallLoggerWithoutInitialize()
        {
            Parallel.For(1, 100, counter =>
            {
                if (counter % 4 == 0)
                    new Exception($"Test Exception: {counter}",
                            new Exception($"Test Inner Exception: {counter}"))
                        .G9SmallLogException($"Test Exception Additional Message: {counter}");
                if (counter % 3 == 0)
                    $"Test Error: {counter}".G9SmallLogError();
                if (counter % 2 == 0)
                    $"Test Warning: {counter}".G9SmallLogWarning();
                else
                    $"Test Information: {counter}".G9SmallLogInformation();
            });
            Assert.Pass();
        }

        [Test]
        [Order(2)]
        public void TestSmallLoggerWitheInitialize()
        {
            G9CSmallLogger.Initialize(Environment.CurrentDirectory, "CustomLogPath",
                $"LogFile-{DateTime.Now:HH-mm-ss.fff}");
            Parallel.For(1, 100, counter =>
            {
                if (counter % 4 == 0)
                    new Exception($"Test Exception: {counter}",
                            new Exception($"Test Inner Exception: {counter}"))
                        .G9SmallLogException($"Test Exception Additional Message: {counter}");
                if (counter % 3 == 0)
                    $"Test Error: {counter}".G9SmallLogError();
                if (counter % 2 == 0)
                    $"Test Warning: {counter}".G9SmallLogWarning();
                else
                    $"Test Information: {counter}".G9SmallLogInformation();
            });
            Assert.Pass();
        }

        [Test]
        [Order(3)]
        public void TestGetProjectName()
        {
            _projectNames = G9CDatabaseVersionControl.GetTotalProjectNames();
            Assert.True(_projectNames.Count == 4 && _projectNames.All(s => !string.IsNullOrEmpty(s)));
        }

        [Test]
        [Order(4)]
        public void TestGetUpdateFiles()
        {
            var dbVersionControlForSqlServer = new G9CDatabaseVersionControlCoreSQLServer(_dataSource, _userId,
                _password, "Project2", _databaseName, "G9TM");
            // From Base
            var projects = dbVersionControlForSqlServer.GetUpdateFiles();
            Assert.True(projects.Count > 0 && projects.All(s => !string.IsNullOrEmpty(s.UpdateFileName)));
        }

        [Test]
        [Order(5)]
        public void TestGetUpdateFolders()
        {
            var dbVersionControlForSqlServer = new G9CDatabaseVersionControlCoreSQLServer(_dataSource, _userId,
                _password, "Project2", _databaseName, "G9TM");
            // From Base
            var projects =
                dbVersionControlForSqlServer.GetUpdateFolders();
            Assert.True(projects.Count > 0 && projects.All(s => !string.IsNullOrEmpty(s.FolderName)));
        }

        [Test]
        [Order(6)]
        public void TestResetDatabase()
        {
            var dbVersionControlForSqlServer = new G9CDatabaseVersionControlCoreSQLServer(_dataSource, _userId,
                _password, "Project1", _databaseName, "G9TM");
            dbVersionControlForSqlServer.RemoveTablesOfDatabaseVersionControlFromDatabase();
            Assert.Pass();
        }

        [Test]
        [Order(7)]
        public void TestStartUpdate()
        {
            var dbVersionControlForSqlServer = new G9CDatabaseVersionControlCoreSQLServer(_dataSource, _userId,
                _password, "Project1", _databaseName, "G9TM");
            // From Base
            dbVersionControlForSqlServer.StartUpdate();
            Assert.Pass();
        }
    }
}