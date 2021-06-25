using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using G9DatabaseVersionControlCore;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType;
using G9DatabaseVersionControlCore.Enums;
using G9DatabaseVersionControlCore.SqlServer;
using G9DatabaseVersionControlWebMVCSampleApp.EfCoreTest;
using G9DatabaseVersionControlWebMVCSampleApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace G9DatabaseVersionControlWebMVCSampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string G9DatabaseVersionControlHandler(string data)
        {
            #region ### Map 4 project for test ###

            G9CDatabaseVersionControl.MapProjects(
                // 1- Base database with backup
                new G9DtMap(
                    "G9DatabaseVersionControlUnitTest",
                    "G9DatabaseVersionControlUnitTest",
                    @"BaseDatabaseBackup/Test.BAK",
                    new G9DtMapDatabaseScriptRequirements(true, true),
                    // Custom params:
                    @"DatabaseUpdateFiles/",
                    false,
                    false,
                    "G9Schema",
                    Encoding.UTF8,
                    () => "9.6.3.1",
                    (oldDbName, newDbName, actionExecuteQueryWithoutResult, funcExecuteQueryWithResult) =>
                        // Func for convert and transfer old data from an old database to a new one
                        new G9DtTaskResult()),
                // 2- Base database with script
                new G9DtMap(
                    "Project1",
                    "Project1",
                    (customDbName, customDbPath) => $@"CREATE DATABASE [{customDbName}]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Test', FILENAME = N'{customDbPath ?? @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER2019\MSSQL\DATA"}\{customDbName}.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Test_log', FILENAME = N'{customDbPath ?? @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER2019\MSSQL\DATA"}\{customDbName}.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT",
                    new G9DtMapDatabaseScriptRequirements(true, true)),
                // 3- Base database with func
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
                    new G9DtMapDatabaseScriptRequirements(true, true)),
                // 4- Base database not exist
                new G9DtMap(
                    "Project3",
                    "Project3",
                    new G9DtMapDatabaseScriptRequirements(true, true))
            );

            #endregion

            var requestPacket = JsonConvert.DeserializeObject<G9DtTaskRequest>(data);
            switch (requestPacket.TaskRequest)
            {
                case G9ETaskRequest.EnterConnectionString:
                    var checkConnectionString =
                        JsonConvert.DeserializeObject<G9DtConnectionString>(requestPacket.JsonData);
                    if (G9CDatabaseVersionControlCoreSQLServer.CheckConnectionString(checkConnectionString.DataSource,
                        checkConnectionString.UserId,
                        checkConnectionString.Password))
                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true,
                            Data = JsonConvert.SerializeObject(G9CDatabaseVersionControl.GetAssignedMaps().Select(s =>
                                new
                                {
                                    s.ProjectName,
                                    s.DatabaseName,
                                    ExistBaseDatabase = s.BaseDatabaseType != G9EBaseDatabaseType.NotSet,
                                    ExistConvert = s.ConvertFromOldDbToNewDbFunc != null
                                }
                            ).ToArray())
                        });

                    return JsonConvert.SerializeObject(new G9DtTaskAnswer
                    {
                        Success = false, NeedShowMessage = true,
                        Message = "The fields entered for connecting to the database are incorrect."
                    });
                case G9ETaskRequest.CheckExistDatabase:
                    var dbExist = JsonConvert.DeserializeObject<G9DtCheckExistDatabase>(requestPacket.JsonData);
                    if (G9CDatabaseVersionControlCoreSQLServer.CheckDatabaseExist(dbExist.DatabaseName,
                        dbExist.DataSource, dbExist.UserId, dbExist.Password))
                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true
                        });

                    return JsonConvert.SerializeObject(new G9DtTaskAnswer
                    {
                        Success = false, NeedShowMessage = true, Message = "There is no database with this name."
                    });
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}