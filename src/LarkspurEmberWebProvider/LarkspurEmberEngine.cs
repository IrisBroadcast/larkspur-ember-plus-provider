using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.Model.Parameters;
using NLog;
using Timer = System.Timers.Timer;
using EmberPlusProviderClassLib.Helpers;
using LarkspurEmberWebProvider.Models;

namespace LarkspurEmberWebProvider
{
    public class LarkspurEmberEngine
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static LarkspurEmberEngine SingleInstance { get; private set; }
        private EmberPlusProvider _emberTree;

        //private static Timer _checkPoolCodecsTimer;
        //// Statisk klassvariabel för att undvika att GC slänger timern.

        public bool EmberTreeState = false;

        public bool EmberTreeInitiatedState()
        {
            return SingleInstance.EmberTreeState;
        }

        public LarkspurEmberEngine()
        {
            SingleInstance = this;

            // Initiate EmBER+ tree
            InitEmberTree().ContinueWith(task =>
            {

            });
        }

        public async Task InitEmberTree()
        {
            bool done = false;
            while (!done)
            {
                try
                {
                    log.Info("Initializing EmBER+ tree");

                    //var config = await BackendService.GetConfiguration();

                    // Initiate EmBER+ tree
                    _emberTree = new EmberPlusProvider(9003, "Larkspur", "Larkspur");
                    _emberTree.CreateIdentityNode(RootIdentifiers.Identity, "Larkspur EmBER+ Provider", "IRIS Broadcast", "0.0.1");

                    // Started
                    EmberTreeState = true;
                    log.Info("EmBER+ tree initiated");
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

        public void Restart()
        {
            TeardownEmberTree();
            Task.Delay(2000).ContinueWith(async t =>
            {
                await InitEmberTree();
            });
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
    }
}
