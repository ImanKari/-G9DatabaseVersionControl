using System;
using System.Diagnostics;
using G9DatabaseVersionControlCore.DataType.AjaxDataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType;
using G9DatabaseVersionControlCore.Enums;
using G9DatabaseVersionControlCore.SqlServer;
using G9DatabaseVersionControlWebMVCSampleApp.Models;
using Microsoft.AspNetCore.Mvc;
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
                            Success = true
                        });

                    return JsonConvert.SerializeObject(new G9DtTaskAnswer
                    {
                        Success = false, NeedShowMessage = true,
                        Message = "The fields entered for connecting to the database are incorrect."
                    });
                    break;
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return "Okay";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}