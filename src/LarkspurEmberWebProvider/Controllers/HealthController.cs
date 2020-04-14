using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using LarkspurEmberWebProvider.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LarkspurEmberWebProvider.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationSettings _appSettings;

        [HttpGet]
        [Route("/start")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var obj = new
                {
                    ApplicationName = "Larkspur EmBER+ Web Provider",//_appSettings.Name,
                    ServiceName = "",
                    StartTime = Process.GetCurrentProcess().StartTime.ToShortDateString() + " " +
                                Process.GetCurrentProcess().StartTime.ToShortTimeString(),
                    Server = "Localhost", //_appSettings.Server,
                    Environment = "Localhost", //_appSettings.Environment,
                    Version = "0.0.1", //_appSettings.Version,
                    LogLevel = "Info",//LogLevelManager.GetCurrentLevel().Name,
                    LogPath = "Logfolder", //_appSettings.LogFolder,
                    Status = new
                    {
                        ReleaseDate = "Release", //_appSettings.ReleaseDate
                    }
                };

                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(obj));
                return new ContentResult()
                {
                    Content = stringPayload,
                    ContentType = "application/json"
                };
            }
            catch (ArgumentNullException)
            {
                return new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new { error = "Could not get health status" }),
                    ContentType = "application/json"
                };
            }
        }

        [HttpGet]
        [Route("/health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                var obj = new
                {
                    ApplicationName = "Larkspur EmBER+ Web Provider",//_appSettings.Name,
                    ServiceName = "",
                    StartTime = Process.GetCurrentProcess().StartTime.ToShortDateString() + " " +
                                Process.GetCurrentProcess().StartTime.ToShortTimeString(),
                    Server = "Localhost", //_appSettings.Server,
                    Environment = "Localhost", //_appSettings.Environment,
                    Version = "0.0.1", //_appSettings.Version,
                    LogLevel = "Info",//LogLevelManager.GetCurrentLevel().Name,
                    LogPath = "Logfolder", //_appSettings.LogFolder,
                    Status = new
                    {
                        ReleaseDate = "Release", //_appSettings.ReleaseDate
                    }
                };

                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(obj));
                return new ContentResult()
                {
                    Content = stringPayload,
                    ContentType = "application/json"
                };
            }
            catch (ArgumentNullException)
            {
                return new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new { error = "Could not get health status" }),
                    ContentType = "application/json"
                };
            }
        }
    }
}
