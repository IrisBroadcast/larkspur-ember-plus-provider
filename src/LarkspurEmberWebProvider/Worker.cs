using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LarkspurEmberWebProvider
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        //private static IDisposable _server;
        //private static LarkspurEmberEngine _emberEngine;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            //_emberEngine = new LarkspurEmberEngine();

            // Configure Web API Self hosting
            //_server = WebApp.Start<Startup>(ApplicationSettings.WebApiUrl);
            //log.Info("SignalR and WebApi started on URL " + ApplicationSettings.WebApiUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            //if (_server != null)
            //{
            //    _server?.Dispose();
            //    _server = null;
            //}

            //if (_emberEngine != null)
            //{
            //    _emberEngine.TeardownEmberTree();
            //    _emberEngine = null;
            //}

        }
    }
}
