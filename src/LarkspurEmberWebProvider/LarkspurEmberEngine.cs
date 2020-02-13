using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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

        private static LarkspurEmberEngine _instance;
        public static LarkspurEmberEngine SingleInstance => _instance;
        private LarkspurEmberTree _emberTree;

        //private static Timer _checkPoolCodecsTimer; // Statisk klassvariabel för att undvika att GC slänger timern.

        //private CcmListener _ccmListener;
        //private CodecControlListener _codecControlHubListener;

        public bool EmberTreeState = false;

        //public bool CcmListenerConnectionState()
        //{
        //    return _ccmListener.GetCurrentConnectionState();
        //}

        //public bool CodecControlListenerConnectionState()
        //{
        //    return _codecControlHubListener.GetCurrentConnectionState();
        //}

        public bool EmberTreeInitiatedState()
        {
            return SingleInstance.EmberTreeState;
        }

        public LarkspurEmberEngine()
        {
            _instance = this;

            // Initiate EmBER+ tree
            InitEmberTree().ContinueWith(task =>
            {
                //// Periodic Codec Pool status check
                //var checkPoolCodecsInterval = TimeSpan.FromSeconds(ApplicationSettings.CheckPoolCodecsInterval).TotalMilliseconds;
                //_checkPoolCodecsTimer = new Timer(checkPoolCodecsInterval);
                //_checkPoolCodecsTimer.Elapsed += CheckPoolCodecStatus;
                //_checkPoolCodecsTimer.Enabled = true;

                //// CCM Codec status listener
                //_ccmListener = new CcmListener();
                //_ccmListener.CodecStatusChanged += OnCodecStatusChanged;

                //// Codec Control status listeners
                //Task.Delay(100).ContinueWith(async t =>
                //{
                //    _codecControlHubListener = new CodecControlListener();
                //    await _codecControlHubListener.StartSignalrConnectionAsync();
                //    _codecControlHubListener.AudioStatusChanged += OnAudioStatusChanged;
                //    _emberTree.GetSipAddressListToSubscribeForAudioStatus()
                //        .ForEach(async subscription =>
                //        {
                //            await _codecControlHubListener.Subscribe(subscription);
                //        });
                //});
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

                    // Read codec status
                    //var codecStatusList = await CcmService.GetCodecStatusListAsync();
                    //if (codecStatusList == null)
                    //{
                    //    throw new Exception("No result when getting codec statuses from CCM, paused initiation of EmBER+ tree");
                    //}
                    //codecStatusList = codecStatusList.OrderBy(a => a.SipAddress).ToList();
                    //log.Debug("Found {0} codec statuses from CCM", codecStatusList.Count);

                    // Get saved values
                    //List<ParameterInfo> persistedParameters =
                    //    FileHelper.ReadJsonFile<List<ParameterInfo>>(ApplicationSettings.PersistenceFile);

                    // Initiate EmBER+ tree
                    //_emberTree = new LarkspurEmberTree(ApplicationSettings.EmberPort, config, persistedParameters, codecStatusList);
                    //var config = new Configuration();
                    _emberTree = new LarkspurEmberTree(9003);
                    //_emberTree.CodecSlotChanged += EmberTree_PublishCodecSlotUpdate;
                    //_emberTree.Restart += Restart;
                    //_emberTree.StudioInfoChanged += EmberTree_StudioInfoChanged;
                    //_emberTree.TxChanged += EmberTree_OnTxChanged();
                    //_emberTree.TreeChanged += EmberTree_OnTreeChanged();
                    //_emberTree.ConnectedToChanged += _emberTree_ConnectedToChanged;

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
                    //throw;
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

        public async Task ReloadWebGuiUrls()
        {
            //await _emberTree.ReloadUrls();
        }

        public void TeardownEmberTree()
        {
            log.Info("Tearing down current EmBER+ tree");

            if (_emberTree != null)
            {
                //_emberTree.Restart -= Restart;
                //_emberTree.CodecSlotChanged -= EmberTree_PublishCodecSlotUpdate;
                //_emberTree.StudioInfoChanged -= EmberTree_StudioInfoChanged;
                //_emberTree.TxChanged -= EmberTree_OnTxChanged();
                //_emberTree.TreeChanged -= EmberTree_OnTreeChanged();
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
