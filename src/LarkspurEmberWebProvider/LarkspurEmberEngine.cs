using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EmberPlusProviderClassLib.Model.Parameters;
using Newtonsoft.Json;
using NGEmberProvider.Lib.Communication;
using NGEmberProvider.Lib.EmberTree;
using NGEmberProvider.Lib.Helpers;
using NGEmberProvider.Lib.Models;
using NGEmberProvider.Lib.Models.Configuration;
using NGEmberProvider.Service.WebApi;
using NLog;
using Timer = System.Timers.Timer;
using EmberPlusProviderClassLib.Helpers;

namespace LarkspurEmberWebProvider
{
    public class LarkspurEmberEngine
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static LarkspurEmberEngine _instance;
        public static LarkspurEmberEngine SingleInstance => _instance;
        private LarkspurEmberTree _emberTree;

        private static Timer _checkPoolCodecsTimer; // Statisk klassvariabel för att undvika att GC slänger timern.

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

        //private void CheckPoolCodecStatus(object sender, ElapsedEventArgs e)
        //{
        //    _checkPoolCodecsTimer.Enabled = false;
        //    Task.Run(async () =>
        //    {
        //        try
        //        {
        //            log.Debug("Performing periodic pool codec check");
        //            var codecStatusList = await CcmService.GetCodecStatusListAsync();
        //            _emberTree.UpdatePoolCodecNodes(codecStatusList);

        //        }
        //        catch (Exception ex)
        //        {
        //            log.Warn(ex, "Exception when performing periodic pool codec check");
        //        }
        //        finally
        //        {
        //            _checkPoolCodecsTimer.Enabled = true;
        //        }
        //    });
        //}

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
                    _emberTree = new LarkspurEmberTree(ApplicationSettings.EmberPort, config);
                    _emberTree.CodecSlotChanged += EmberTree_PublishCodecSlotUpdate;
                    _emberTree.Restart += Restart;
                    _emberTree.StudioInfoChanged += EmberTree_StudioInfoChanged;
                    _emberTree.TxChanged += EmberTree_OnTxChanged();
                    _emberTree.TreeChanged += EmberTree_OnTreeChanged();
                    _emberTree.ConnectedToChanged += _emberTree_ConnectedToChanged;

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

        private void _emberTree_ConnectedToChanged(string identifierPath, string sipAddress)
        {
            Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(sipAddress))
                {
                    await _codecControlHubListener.Subscribe(new AudioStatusSubscription()
                    { IdentifierPath = identifierPath, SipAddress = sipAddress });
                }
                else
                {
                    await _codecControlHubListener.Unsubscribe(identifierPath);
                }
            });
        }

        private EventHandler EmberTree_OnTxChanged()
        {
            return EventHandlerHelper.ThrottledEventHandler((sender, e) => { EmberHub.TxUpdate(); }, 50);
        }

        private EventHandler EmberTree_OnTreeChanged()
        {
            return EventHandlerHelper.ThrottledEventHandler((sender, e) => { SaveTree(); }, 2000);
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
            await _emberTree.ReloadUrls();
        }

        public void TeardownEmberTree()
        {
            log.Info("Tearing down current EmBER+ tree");

            if (_emberTree != null)
            {
                _emberTree.Restart -= Restart;
                _emberTree.CodecSlotChanged -= EmberTree_PublishCodecSlotUpdate;
                _emberTree.StudioInfoChanged -= EmberTree_StudioInfoChanged;
                _emberTree.TxChanged -= EmberTree_OnTxChanged();
                _emberTree.TreeChanged -= EmberTree_OnTreeChanged();
                _emberTree.Dispose();
                _emberTree = null;
                EmberTreeState = false;
            }
            else
            {
                log.Warn("EmBER+ tree was already null, so no need to tear it down");
            }
        }

        private void EmberTree_StudioInfoChanged(string studio)
        {
            try
            {
                StudioInfoLight studioInfo = GetStudioInfo(studio);
                log.Info("StudioInfoChanged event for {0}", studioInfo);
                EmberHub.StudioInfoUpdate(studioInfo);
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when updating studio info");
            }
        }

        private void OnCodecStatusChanged(CodecStatus codecStatus)
        {
            try
            {
                if (_emberTree.HasCodecWithSipAddress(codecStatus.SipAddress))
                {
                    log.Info("Received codec status. Sip address={0}, state={1}", codecStatus.SipAddress, codecStatus.State);
                    _emberTree.UpdatePoolCodecs(codecStatus);
                    _emberTree.UpdateStudioCodecSlots(codecStatus);
                }
                else
                {
                    log.Debug("Ignoring received codec status for sip address {0} because not part of tree ", codecStatus.SipAddress);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when updating codec status OnCodecStatusChanged");
            }
        }

        private void OnAudioStatusChanged(List<AudioStatusSubscription> subscriptions, AudioStatusResponse audioStatus)
        {
            try
            {
                log.Trace($"AudioStatus parsing subscriptions, count: '{subscriptions.Count}'");
                // Should this be checked if its interesting
                foreach (var subscription in subscriptions)
                {
                    // TODO: This is done way to much, is there some error here?
                    log.Trace($"AudioStatus received for sub: {subscription.IdentifierPath}");
                    _emberTree.UpdateInputGainAndEnabled(subscription.IdentifierPath, audioStatus);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when updating audio status");
            }
        }

        void EmberTree_PublishCodecSlotUpdate(SlotInfo slotInfo)
        {
            EmberHub.CodecSlotUpdate(slotInfo);
        }

        public void SetMaintenanceMode(string studioId, bool inMaintenance)
        {
            if (_emberTree != null)
            {
                _emberTree.SetMaintenanceMode(studioId, inMaintenance);
            }
            else
            {
                log.Warn("Can't set maintenance mode, there is no EmBER+ tree available");
            }
        }

        public static SlotInfo GetSlotInfo(string studio, string slot) { return SingleInstance._emberTree.GetSlotInfo(studio, slot); }
        public static SlotInfo GetSlotInfoByStudioIdentifier(string studioIdentifier, string slot) { return SingleInstance._emberTree.GetSlotInfoByStudioIdentifier(studioIdentifier, slot); }
        public static string GetStudioSlotSipAddress(string studio, string slot) { return SingleInstance._emberTree.GetStudioSlotSipAddres(studio, slot); }
        public static List<SlotInfo> GetStudioSlots(string studio) { return SingleInstance._emberTree.GetStudioSlots(studio); }
        public static StudioInfoLight GetStudioInfo(string studio) { return SingleInstance._emberTree.GetStudioInfo(studio); }
        public static List<StudioInfoLight> GetStudioInfoList() { return SingleInstance._emberTree.GetStudioInfoList(); }
        public static List<StudioStates> GetStudioStatesList() { return SingleInstance._emberTree.GetStudioStatesList(); }
        public static List<TxInfo> GetTxInfoList() { return SingleInstance._emberTree.GetTxInfos(); }
        public static List<TxSourceInfo> GetTxInfoForStudio(string studio) { return SingleInstance._emberTree.GetTxInfoForStudio(studio); }
        public static void RequestRestart() { SingleInstance.Restart(); }
        public static async Task<List<string>> CheckAvailableStudiosAsync() { return await SingleInstance._emberTree.CheckAvailableStudiosAsync(); }
        public static void SetMaintenance(string studioId, bool inMaintenance) { SingleInstance.SetMaintenanceMode(studioId, inMaintenance); }
        public static async Task ReloadUrlsAsync() { await SingleInstance.ReloadWebGuiUrls(); }

        private void SaveTree()
        {
            var persistenceFile = ApplicationSettings.PersistenceFile;
            log.Info("Saving EmBER+ tree to: \"{0}\"", persistenceFile);

            try
            {
                using (StreamWriter writer = new StreamWriter(persistenceFile))
                {
                    IList<ParameterBase> writeableParameters = _emberTree?.GetWritableParameters();
                    var parameterList = writeableParameters?.Select(p => new ParameterInfo(p)).ToList() ?? new List<ParameterInfo>();
                    var json = JsonConvert.SerializeObject(parameterList);
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not save persistent EmBER+ tree to: {0}", persistenceFile);
            }
        }
    }
}
