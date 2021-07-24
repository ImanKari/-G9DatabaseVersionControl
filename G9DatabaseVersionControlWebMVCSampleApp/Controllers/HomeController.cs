using G9DatabaseVersionControlCore;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType;
using G9DatabaseVersionControlCore.SqlServer;
using G9DatabaseVersionControlWebMVCSampleApp.EfCoreTest;
using G9DatabaseVersionControlWebMVCSampleApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace G9DatabaseVersionControlWebMVCSampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            #region ### Map 4 project ###

            // 1- Base database not exist
            var map1 =
                new G9DtMap(
                    "Project3",
                    "Project3",
                    new G9DtMapDatabaseScriptRequirements(true, true),
                    productVersionFunc: GetProjectVersion);


            // 2- Base database with backup
            var map2 =
                new G9DtMap(
                    "G9DatabaseVersionControlWebMVCSampleApp",
                    "Version",
                    @"BaseDatabaseBackup/Test.BAK",
                    new G9DtMapDatabaseScriptRequirements(true, true));


            // 3- Base database with func
            var map3 =
                new G9DtMap(
                    "Project2",
                    "Project2",
                    (customDbName, customDbPath, connectionString) =>
                    {
                        try
                        {
                            // For example, in this case you can use EFCore to create a database
                            var dbContext = new G9CEfCoreDbContext();
                            dbContext.Database.SetConnectionString(connectionString.ToString());
                            if (dbContext.Database.EnsureCreated())
                                return new G9DtTaskResult();
                            return new G9DtTaskResult("Database already existed!");
                        }
                        catch (Exception ex)
                        {
                            return new G9DtTaskResult(ex.Message);
                        }
                    },
                    new G9DtMapDatabaseScriptRequirements(true, true));


            // 4- Base database with script
            var map4 =
                new G9DtMap(
                    "Project1",
                    "Project1",
                    (customDbName, customDbPath) =>
                    {
                        return $@"CREATE DATABASE [{customDbName}]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Test', FILENAME = N'{customDbPath}\{customDbName}.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Test_log', FILENAME = N'{customDbPath}\{customDbName}.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT";
                    },
                    new G9DtMapDatabaseScriptRequirements(true, true, needToSaveUpdatedScriptData: true, isRequiredToRemoveGoPhrase: true),

                    // Custom params:
                    @"DatabaseUpdateFiles\",
                    false,
                    false,
                    "dbo",
                    Encoding.UTF8,
                    GetProjectVersion,
                    new G9DtCustomTask("Test",
                        "At first, check to exist schema with name Test, if not exist then create it",
                        (dbName, actionExecuteQueryWithoutResult, funcExecuteQueryWithResult) =>
                        {
                            // Custom func
                            try
                            {
                                if (!funcExecuteQueryWithResult("SELECT 1 FROM sys.schemas WHERE name = 'Test'").Any())
                                    actionExecuteQueryWithoutResult("CREATE SCHEMA [Test]");
                                return new G9DtTaskResult();
                            }
                            catch (Exception ex)
                            {
                                return new G9DtTaskResult(ex.Message);
                            }
                        }),
                    new G9DtCustomTask("Test2",
                        "At first, check to exist schema with name Test2, if not exist then create it\nAt first, check to exist schema with name Test2, if not exist then create it\nAt first, check to exist schema with name Test2, if not exist then create it\nAt first, check to exist schema with name Test2, if not exist then create it\nAt first, check to exist schema with name Test2, if not exist then create it\nAt first, check to exist schema with name Test2, if not exist then create it",
                        (dbName, actionExecuteQueryWithoutResult, funcExecuteQueryWithResult) =>
                        {
                            // Custom func
                            try
                            {
                                if (!funcExecuteQueryWithResult("SELECT 1 FROM sys.schemas WHERE name = 'Test2'").Any())
                                    actionExecuteQueryWithoutResult("CREATE SCHEMA [Test2]");
                                return new G9DtTaskResult();
                            }
                            catch (Exception ex)
                            {
                                return new G9DtTaskResult(ex.Message);
                            }
                        }));


            G9CDatabaseVersionControl.MapProjects(map1, map2, map3, map4);

            #endregion
        }

        public IActionResult Index()
        {
            return View();
        }

        private string GetProjectVersion()
        {
            return
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NETCOREAPP)
                string.IsNullOrEmpty(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                    ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
                    : Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#elif (NETSTANDARD1_6 || NETSTANDARD1_5)
        string.IsNullOrEmpty(Assembly.GetEntryAssembly().GetName().Version.ToString())
                ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
                : Assembly.GetEntryAssembly().GetName().Version.ToString();
#else
                string.IsNullOrEmpty(Assembly.Load(new AssemblyName(nameof(HomeController))).GetName()
                    .Version
                    .ToString())
                    ? Assembly.Load(new AssemblyName(nameof(HomeController)))?.GetName()?.Version
                          .ToString() ??
                      "0.0.0.0"
                    : Assembly.Load(new AssemblyName(nameof(HomeController))).GetName().Version.ToString();
#endif
        }

        [HttpPost]
        public string G9DatabaseVersionControlHandler(string data)
        {
            var requestPacket = JsonConvert.DeserializeObject<G9DtTaskRequest>(data);
            return G9CDatabaseVersionControlCoreSQLServer.HandleTaskRequest(requestPacket);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}