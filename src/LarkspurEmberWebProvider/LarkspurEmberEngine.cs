using System;
using System.Threading;
using System.Threading.Tasks;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.Helpers;
using LarkspurEmberWebProvider.Hubs;
using NLog;
using LarkspurEmberWebProvider.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Timer = System.Timers.Timer;

namespace LarkspurEmberWebProvider
{
    public class LarkspurEmberEngine : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<LarkspurHub, ILarkspurHub> _websocketHub;

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static LarkspurEmberEngine SingleInstance { get; private set; }
        private static EmberPlusProvider _emberTree;

        //private static Timer _checkPoolCodecsTimer;
        //// Statisk klassvariabel för att undvika att GC slänger timern.

        public bool EmberTreeState = false;

        public bool EmberTreeInitiatedState()
        {
            return SingleInstance.EmberTreeState;
        }

        public event EventHandler TreeChanged;

        public LarkspurEmberEngine(
            IServiceProvider serviceProvider,
            IHubContext<LarkspurHub, ILarkspurHub> websocketHub)
        {
            _serviceProvider = serviceProvider;
            _websocketHub = websocketHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SingleInstance = this;
            log.Info("Execute async running Larkspur EmbBER+ Engine");

            // Initiate EmBER+ tree
            InitEmberTree().ContinueWith(task =>
            {
                // TODO: do some essentials, like checking up on static data
            });
            
            try
            {
                while (!stoppingToken.IsCancellationRequested) // Keep the thread alive
                {
                    // TODO: add an infinite delay?
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                log.Warn("EmBER+ Provider background service terminated.");
                TeardownEmberTree();
            }
        }

        public async Task InitEmberTree()
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    log.Info("Initializing EmBER+ tree");
                    Console.WriteLine("Initializing EmBER+ tree");

                    //var config = await BackendService.GetConfiguration();

                    // Initiate EmBER+ tree
                    _emberTree = new EmberPlusProvider(9003, "Larkspur", "Larkspur");
                    _emberTree.TreeChanged += EmberTree_OnTreeChangedAsync();
                    _emberTree.CreateIdentityNode(RootIdentifiers.Identity, "Larkspur EmBER+ Provider", "IRIS Broadcast", "0.0.1");

                    _emberTree.InitializeAllNodes(RootIdentifiers.Utilities);

                    // Started
                    EmberTreeState = true;
                    log.Info("EmBER+ tree initiated");
                    Console.WriteLine("EmBER+ tree initiated");
                    done = true;
                }
                catch (Exception ex)
                {
                    EmberTreeState = false;
                    log.Error(ex, "Exception when initializing EmBER+ tree");
                    Thread.Sleep(2000);
                }
            }
        }

        public void TeardownEmberTree()
        {
            log.Info("Tearing down current EmBER+ tree");

            if (_emberTree != null)
            {
                _emberTree.Dispose();
                _emberTree = null;
                EmberTreeState = false;
            }
            else
            {
                log.Warn("EmBER+ tree was already null, so no need to tear it down");
            }
        }

        /// <summary>
        /// EmBER+ tree events on any changes, use this to persist data or similar.
        /// </summary>
        private EventHandler EmberTree_OnTreeChangedAsync()
        {
            return EventHandlerHelper.ThrottledEventHandler((sender, e) =>
            {
                // TODO: Persist tree
                _websocketHub.Clients.All.ChangesInEmberTree("Hi!!!!!222");
                Debug.WriteLine("You should save the tree");
            }, 200);
        }

        /// <summary>
        /// Methods
        /// </summary>
        public void Restart()
        {
            TeardownEmberTree();
            Task.Delay(2000).ContinueWith(async t =>
            {
                await InitEmberTree();
            });
        }

        public void Engine_SetGpio()
        {
            Debug.WriteLine("Setting GPIO...");
        }

    }
}
