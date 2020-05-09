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
using EmberPlusProviderClassLib;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LarkspurEmberWebProvider
{
    public class Program
    {
        //private static LarkspurEmberEngine _emberEngine;

        public static void Main(string[] args)
        {

            //var isService = !(Debugger.IsAttached || args.Contains("--console"));
            //var builder = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray());

            //if (isService)
            //{
            //    var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            //    var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            //    builder.UseContentRoot(pathToContentRoot);
            //}

            //var host = builder.Build();

            //if (isService)
            //{
            //    host.RunAsService();
            //}
            //else
            //{
            //    host.Run();
            //}


            try
            {
                //_emberEngine = new LarkspurEmberEngine();

                CreateHostBuilder(args).Build().Run();

                

                //Console.WriteLine("Ruby Ember+ Dummy v{0}.{1} (GlowDTD v{2} - EmBER v{3})",
                //    typeof(Program).Assembly.GetName().Version.Major,
                //    typeof(Program).Assembly.GetName().Version.Minor,
                //    EmberVersion.GlowDtdVersion,
                //    EmberVersion.EmberEncodingVersion);
                //Console.WriteLine("\nPress Enter to quit...");
                //Console.ReadLine();



            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in Program: {ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Set up the EmBER+ provider
                    services.AddHostedService<LarkspurEmberEngine>();

                    // Set up Kestrel (OWIN) hosting server
                    services.Configure<KestrelServerOptions>(context.Configuration.GetSection("Kestrel"));
                })
                .ConfigureWebHostDefaults((webBuilder) =>
                {
                    // This one configures the web hosting
                    //webBuilder.UseUrls("http://+:5050");
                    //webBuilder.ConfigureKestrel(serverOptions =>
                    //{
                    //    // Set properties and call methods on options
                    //});
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService(); // INFO: Makes it possible to run as a Windows Service
    }
}