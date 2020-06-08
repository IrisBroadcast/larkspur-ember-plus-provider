#region copyright
/*
 * Larkspur Ember Plus Provider
 *
 * Copyright (c) 2020 Roger Sandholm & Fredrik Bergholtz, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
