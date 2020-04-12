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