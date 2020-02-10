using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LarkspurEmberWebProvider
{
    public class Program
    {
        private static LarkspurEmberEngine _emberEngine;

        public static void Main(string[] args)
        {
            _emberEngine = new LarkspurEmberEngine();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseWindowsService(); // INFO: Makes it possible to run as a Windows Service
    }
}
