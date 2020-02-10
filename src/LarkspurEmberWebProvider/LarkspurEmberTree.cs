using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmberLib.Glow;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Helpers;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using LarkspurEmberWebProvider.Models;
using NLog;

namespace LarkspurEmberWebProvider
{
    //public class LarkspurEmberTree : EmberPlusProviderClassLib.EmberTree
    public class LarkspurEmberTree : EmberPlusProviderClassLib.EmberTreeProvider
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        #region Members
        public const string RootIdentifier = "NG Ember Provider";
        private static readonly string TreePathForTxPools = $"{RootIdentifier}/TxPool";
        private static readonly string TreePathForStudios = $"{RootIdentifier}/Studios";

        private static readonly string StudioPattern = "^NG Ember Provider/Studios/(?<studio>[^/]*)(/[^/]*)$";
        private static readonly string StudioParameterPattern = "^NG Ember Provider/Studios/(?<studio>[^/]*)/{0}$";
        private static readonly string SlotParameterPattern = "^NG Ember Provider/Studios/(?<studio>[^/]*)/CodecSlots/(?<slot>[^/]*)/{0}$";
        private static readonly string TxSourceParameterPattern = "^NG Ember Provider/TxPool/(?<txPool>[^/]*)/Sources/(?<source>[^/]*)/(?<parameter>[^/]*)$";
        private static readonly string TxTaParameterPattern = "^NG Ember Provider/TxPool/(?<txPool>[^/]*)/Ta$";
        private static readonly string TxNationalFeedParameterPattern = "^NG Ember Provider/TxPool/(?<txPool>[^/]*)/Nationalfeed$";

        public event Action Restart;      // Triggas d� EmberProvidern ska startas om.
        public event EventHandler TreeChanged;  // Triggas d� n�got v�rde i Embertr�det �ndrats.

        //public delegate void SlotUpdateEventHandler(SlotInfo e);
        //public event SlotUpdateEventHandler CodecSlotChanged;      // Triggas d� en kodarslot �ndrats i Embertr�det.

        public delegate void StudioInfoUpdateEventHandler(string studio);
        public event StudioInfoUpdateEventHandler StudioInfoChanged; // Triggas d� n�gon studio-parameter �ndrats

        public delegate void ConnectedToUpdateEventHandler(string identifierPath, string sipAddress);
        public event ConnectedToUpdateEventHandler ConnectedToChanged; // Triggas d� ConnectedToSipId under CodecSlot �ndrats (dvs d� ett samtal startat eller avslutats).

        public event EventHandler TxChanged;            // Triggas d� Tx.TA eller StudioId/Prepare/OnAir p� TxSource �ndrats

        private readonly List<string> _poolCodecSipAddresses;

        private readonly List<string> _parametersToTriggerTxEvent = new List<string>
        {
            TxSourceIdentifiers.StudioId.ToString(),
            TxSourceIdentifiers.Prepare.ToString(),
            TxSourceIdentifiers.OnAir.ToString()
        };

        private Configuration _config;
        #endregion

        #region Initiation
        /// <summary>
        /// Initiates the application EmBER+ tree
        /// </summary>
        /// <param name="port"></param>
        /// <param name="config"></param>
        //        public LarkspurEmberTree(int port, Configuration config, List<ParameterInfo> persistedParameters, IList<CodecStatus> codecStatusList)
        //            : base(port, "NG Ember Provider", "Next Generation Ember Plus Provider")
        public LarkspurEmberTree(int port, Configuration config)
            : base(port)
        {
            _config = config;

            // Identity node
            //Provider.CreateIdentityNode(0, "NGEmberProvider", "Sveriges Radio", ApplicationSettings.Version);

            // Utility node
            CreateUtilitiesNode();

            // Codec pool list
            /*var codecPoolListNode = Provider.AddChildNode(RootIdentifiers.CodecPools);
            config.CodecPools?.Each((codecPool, index) =>
            {
                try
                {
                    CreateCodecPool(codecPoolListNode, index, codecPool);
                    log.Info("Creating CreateCodecPoolNode with identifier " + codecPool.Name);
                }
                catch (Exception e)
                {
                    log.Error("Error in creating CreateCodecPoolNode, " + e.Message);
                }
            });
            _poolCodecSipAddresses = GetCodecPoolUnitNodes().Select(u => u.GetStringParameterValue(PoolCodecNodeIdentifiers.SipId)).ToList();

            // TX's lists
            var txPoolNode = Provider.AddChildNode(RootIdentifiers.TxPool);
            config.TxPool?.OrderBy(t => t.NodeIdentifier).Each((tx, index) =>
            {
                try
                {
                    CreateTxNode(txPoolNode, index, tx);
                    log.Info("Creating TxNode with identifier " + tx.NodeIdentifier);
                }
                catch (Exception e)
                {
                    log.Error("Error in creating TxNode, " + e.Message);
                }
            });

            // Studios
            var studiosNode = Provider.AddChildNode(RootIdentifiers.Studios);
            config.Studios?.OrderBy(s => s.NodeIdentifier).Each((studio, index) =>
            {
                try
                {
                    CreateStudioNode(studiosNode, index, studio);
                    log.Info("Creating StudiosNode with identifier " + studio.NodeIdentifier);
                }
                catch (Exception e)
                {
                    log.Error("Error in creating StudiosNode, " + e.Message);
                }
            });

            // Regions
            var regionsNode = Provider.AddChildNode(RootIdentifiers.Regions);
            config.Regions?.OrderBy(r => r.Identifier).Each((region, index) =>
            {
                try
                {
                    CreateRegionNode(regionsNode, index, region);
                    log.Info("Creating RegionNode with identifier " + region.Identifier);
                }
                catch (Exception e)
                {
                    log.Error("Error in creating RegionsNode, " + e.Message);
                }
            });*/

            // Everything went well
            log.Debug("Ember provider created and listening on port {0}", port);

            // Update nodes created with loaded parameters
            //RestoreParameters(persistedParameters);

            // Re:evaluate node values
            //UpdatePoolCodecNodes(codecStatusList);
            //UpdateStudioCodecSlots(codecStatusList);
            //UpdatePoolCodecsOwner();
            //UpdatePoolTxSourceNodes();

            Provider.Dispatcher.GlowRootReady += OnEmberTreeChanged;
        }
        #endregion

        /*
        #region Add function methods
        public async Task<GlowValue[]> RestartFunction(GlowValue[] args)
        {
            try
            {
                string magicString = args[0].String;
                if (magicString == "restart")
                {
                    await Task.Delay(1000).ContinueWith(t =>
                    {
                        log.Info("Ember provider restarting");
                        Restart?.Invoke();
                    });

                    return new[] { Function.CreateArgumentValue(true) };
                }
                else
                {
                    return new[] { Function.CreateArgumentValue(false) };
                }
            }
            catch (Exception)
            {
                return new[] { Function.CreateArgumentValue(false) };
            }
        }

        public async Task<GlowValue[]> ReloadWebGuiUrlsFunction(GlowValue[] args)
        {
            try
            {
                await ReloadUrls();
                return new[] { Function.CreateArgumentValue(true) };
            }
            catch (Exception)
            {
                return new[] { Function.CreateArgumentValue(false) };
            }
        }

        public async Task ReloadUrls()
        {
            log.Info("Reloading WebGui URLs");
            var config = await BackendService.GetConfiguration();

            foreach (var studioNode in GetStudioNodes())
            {
                Node displayTypesNode = studioNode.GetChildNode(StudioNodeIdentifiers.WebGuiUrls);
                var urls = config.Studios.FirstOrDefault(s => s.NodeIdentifier == studioNode.Identifier)?.WebGuiUrls;
                UpdateDisplayTypeNode(displayTypesNode, urls);
            }
        }
        #endregion
        */

        #region Creation & Initiation methods
        private void CreateUtilitiesNode()
        {
            var utilNode = Provider.AddChildNode(RootIdentifiers.Utilities);
            utilNode.AddStringParameter(UtilitiesIdentifiers.Server, Environment.MachineName);
            utilNode.AddStringParameter(UtilitiesIdentifiers.StartTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Provider.AddChildNode(RootIdentifiers.Studios);
            /*
            // Log level function
            utilNode.AddFunction(
                UtilitiesIdentifiers.LogLevel,
                new[] { Function.CreateBooleanArgument("debug") },
                new[] { Function.CreateBooleanArgument("success") },
                LogLevelFunction);
            
            // Restart function
            utilNode.AddFunction(
                UtilitiesIdentifiers.Restart,
                new[] { Function.CreateStringArgument("magicString (=restart)") },
                new[] { Function.CreateBooleanArgument("success") },
                RestartFunction);
            
            utilNode.AddFunction(
                UtilitiesIdentifiers.ReloadWebGuiUrls,
                null,
                new[] { Function.CreateBooleanArgument("success") },
                ReloadWebGuiUrlsFunction);
            */
        }

        /*
        private void CreateStudioNode(EmberNode parentNode, int index, Studio studio)
        {
            EmberNode studioNode = parentNode.AddSubNode(index, studio.NodeIdentifier);
            studioNode.AddStringParameter(StudioNodeIdentifiers.StudioId, studio.StudioId);
            studioNode.AddStringParameter(StudioNodeIdentifiers.DisplayName, studio.DisplayName);
            studioNode.AddStringParameter(StudioNodeIdentifiers.Region, studio.Region);
            studioNode.AddStringParameter(StudioNodeIdentifiers.ModifyRegion, studio.Region, true, "ModifyRegion");
            studioNode.AddStringParameter(StudioNodeIdentifiers.LoggedInUser, "", true);
            studioNode.AddStringParameter(StudioNodeIdentifiers.LoggedInDisplayName);
            studioNode.AddStringParameter(StudioNodeIdentifiers.Type, studio.Type);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.Remoted, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.InfoServiceOnAir, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.HostMicOn, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.GuestMicOn, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.InMaintenance, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.OnAir, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.Prepare, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.PrivateMode, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.ActiveStudioConnection, true);
            studioNode.AddBooleanParameter(StudioNodeIdentifiers.Recording, true);

            EmberNode displayTypesNode = studioNode.AddSubNode(StudioNodeIdentifiers.WebGuiUrls);
            CreateDisplayTypeNode(displayTypesNode, studio.WebGuiUrls);

            EmberNode codecSlotsNode = studioNode.AddSubNode(StudioNodeIdentifiers.CodecSlots);
            studio.CodecSlots.Each((slot, i) => CreateCodecSlotNode(codecSlotsNode, i, slot));

            EmberNode netIosNode = studioNode.AddSubNode(StudioNodeIdentifiers.NetIos);
            for (int i = 0; i < 4; i++)
            {
                CreateNetIoNode(netIosNode, i);
            }
        }

        private void CreateNetIoNode(EmberNode parentNode, int index)
        {
            EmberNode netIoNode = parentNode.AddSubNode(index, $"netio-{(index + 1):D2}");
            netIoNode.AddStringParameter(NetIoNodeIdentifiers.ConnectedTo, "", true);
        }

        private void CreateCodecPool(EmberNode codecPoolListNode, int poolIndex, CodecPool codecPool)
        {
            var codecPoolNode = codecPoolListNode.AddSubNode(poolIndex, $"CodecPool{poolIndex + 1:000}");
            codecPoolNode.AddStringParameter(CodecPoolNodeIdentifiers.DisplayName, codecPool.Name);
            var unitsNode = codecPoolNode.AddSubNode(CodecPoolNodeIdentifiers.Units);
            codecPool.Units.Each((unit, unitIndex) => { CreatePoolCodecUnitNode(unitsNode, unitIndex, unit); });
        }

        private void CreatePoolCodecUnitNode(EmberNode parentNode, int index, Unit unit)
        {
            EmberNode codecNode = parentNode.AddSubNode(index, unit.Identifier);
            codecNode.AddStringParameter(PoolCodecNodeIdentifiers.SipId, unit.SipId);
            codecNode.AddStringParameter(PoolCodecNodeIdentifiers.DisplayName);
            codecNode.AddStringParameter(PoolCodecNodeIdentifiers.Owner);
            codecNode.AddBooleanParameter(PoolCodecNodeIdentifiers.IsRegistered);
            codecNode.AddBooleanParameter(PoolCodecNodeIdentifiers.IsInCall);
        }

        private void CreateTxNode(EmberNode parentNode, int index, Tx tx)
        {
            var txNode = parentNode.AddSubNode(index, tx.NodeIdentifier);
            txNode.AddStringParameter(TxNodeIdentifiers.Region, tx.RegionName);
            txNode.AddStringParameter(TxNodeIdentifiers.DisplayName, tx.DisplayName);
            txNode.AddBooleanParameter(TxNodeIdentifiers.Ta, true);
            txNode.AddBooleanParameter(TxNodeIdentifiers.Nationalfeed, true);

            // TX Sources
            var sourcesNode = txNode.AddSubNode(TxNodeIdentifiers.Sources);
            tx.Sources.Each((unit, i) =>
            {
                CreateTxSourceNode(sourcesNode, i, unit);
            });
        }

        private void CreateTxSourceNode(EmberNode parentNode, int index, TxSource txSource)
        {
            EmberNode sourceNode = parentNode.AddSubNode(index, txSource.SourceId);
            sourceNode.AddStringParameter(TxSourceIdentifiers.StudioId, txSource.StudioId, true);
            sourceNode.AddStringParameter(TxSourceIdentifiers.StudioDisplayName, "", true);
            sourceNode.AddStringParameter(TxSourceIdentifiers.Prepare, false.ToString(), true);
            sourceNode.AddStringParameter(TxSourceIdentifiers.OnAir, false.ToString(), true);
            sourceNode.AddBooleanParameter(TxSourceIdentifiers.PrepareBool, false);
            sourceNode.AddBooleanParameter(TxSourceIdentifiers.OnAirBool, false);
        }

        private void CreateDisplayTypeNode(EmberNode parentNode, WebGuiUrls displayType)
        {
            parentNode.AddStringParameter(DisplayNodeIdentifiers.PlaylistUrl, displayType.PlaylistUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.JinglesUrl, displayType.JinglesUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Player01Url, displayType.Player01Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Player02Url, displayType.Player02Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Player03Url, displayType.Player03Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.PrelistenUrl, displayType.PrelistenUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Dialer01Url, displayType.Dialer01Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Dialer02Url, displayType.Dialer02Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Dialer03Url, displayType.Dialer03Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.Dialer04Url, displayType.Dialer04Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.RemoteConnectorUrl, displayType.RemoteConnectorUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.StudioSignageUrl, displayType.StudioSignageUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.ReadonlyPlaylistUrl, displayType.ReadonlyPlaylistUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.SidekickScreenUrl, displayType.SidekickScreenUrl);

            parentNode.AddStringParameter(DisplayNodeIdentifiers.DeskPlaylistUrl, displayType.DeskPlaylistUrl);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.NetIo01Url, displayType.NetIo01Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.NetIo02Url, displayType.NetIo02Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.NetIo03Url, displayType.NetIo03Url);
            parentNode.AddStringParameter(DisplayNodeIdentifiers.NetIo04Url, displayType.NetIo04Url);
        }

        private void CreateCodecSlotNode(EmberNode parentNode, int index, CodecSlot codecSlot)
        {
            EmberNode codecNode = parentNode.AddSubNode(index, codecSlot.Identifier);

            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.SipId, codecSlot.DefaultSipId, true);
            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.DisplayName);
            codecNode.AddBooleanParameter(CodecSlotNodeIdentifiers.IsOnAir, true);
            codecNode.AddBooleanParameter(CodecSlotNodeIdentifiers.IsInCall);
            codecNode.AddBooleanParameter(CodecSlotNodeIdentifiers.IsInPhoneCall);
            codecNode.AddBooleanParameter(CodecSlotNodeIdentifiers.IsOutgoingCall);
            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.ConnectedToSipId);
            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayName);
            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.ConnectedToLocation);
            codecNode.AddStringParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayNameAndLocation);
            AddSetGpoFunction(codecNode, CodecSlotNodeIdentifiers.SetGpo0, CodecSlotNodeIdentifiers.SipId, 0);
            AddSetGpoFunction(codecNode, CodecSlotNodeIdentifiers.SetGpo1, CodecSlotNodeIdentifiers.SipId, 1);
            AddSetGpoFunction(codecNode, CodecSlotNodeIdentifiers.ConnectedToSetGpo0, CodecSlotNodeIdentifiers.ConnectedToSipId, 0);
            AddSetGpoFunction(codecNode, CodecSlotNodeIdentifiers.ConnectedToSetGpo1, CodecSlotNodeIdentifiers.ConnectedToSipId, 1);
            AddHangupFunction(codecNode, CodecSlotNodeIdentifiers.Hangup);
            AddConnectedToTurnInputsOnOff(codecNode, CodecSlotNodeIdentifiers.ConnectedToTurnInputsOn, true);
            AddConnectedToTurnInputsOnOff(codecNode, CodecSlotNodeIdentifiers.ConnectedToTurnInputsOff, false);

            EmberNode connectedToInputsNode = codecNode.AddSubNode(CodecSlotNodeIdentifiers.ConnectedToInputs);

            for (int i = 0; i < 5; i++)
            {
                EmberNode inputNode = connectedToInputsNode.AddSubNode(i, $"Input{i + 1:00}");
                inputNode.AddIntegerParameter(CodecSlotInputIdentifiers.InputNumber, false, 0, 99, i);
                inputNode.AddBooleanParameter(CodecSlotInputIdentifiers.IsEnabled);
                inputNode.AddIntegerParameter(CodecSlotInputIdentifiers.CurrentGain, false, -1000, 1000);
                AddRefreshConnectedToInputValuesFunction(inputNode, CodecSlotInputIdentifiers.RefreshValues, i);
                AddConnectedToEnableFunction(inputNode, CodecSlotInputIdentifiers.Enable, i);
                AddConnectedToIncreaseDecreaseFunction(inputNode, CodecSlotInputIdentifiers.IncreaseGain, i, CodecControlService.IncreaseInputGainAsync);
                AddConnectedToIncreaseDecreaseFunction(inputNode, CodecSlotInputIdentifiers.DecreaseGain, i, CodecControlService.DecreaseInputGainAsync);
                inputNode.AddBooleanParameter(CodecSlotInputIdentifiers.ControlThis, true);
            }
        }

        private void CreateRegionNode(EmberNode parentNode, int index, Region region)
        {
            EmberNode regionNode = parentNode.AddSubNode(index, region.Identifier);
            regionNode.AddStringParameter(RegionNodeIdentifiers.Name, region.Name);
        }

        private void AddSetGpoFunction(EmberNode node, ValueType nodeIdentifier, ValueType sipAddressNodeName, int number)
        {
            async Task<GlowValue[]> Func(GlowValue[] args)
            {
                bool success = false;
                string sipAddress = node.GetStringParameterValue(sipAddressNodeName); // H�mtar SIP-adress fr�n node med detta namn

                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("SetGpo can't be invoked. Empty {0}", sipAddressNodeName);
                }
                else
                {
                    bool active = args[0].Boolean;
                    log.Debug("Sending SetGpo to CodecControl. Setting GPO{0} to {1} on {2}", number, active, sipAddress);
                    var response = await CodecControlService.SetGpoAsync(sipAddress, number, active);
                    log.Debug($"Received response for SetGpo from CodecControl. Active={response?.Active}");
                    success = response != null && response.Active == active;
                }
                return new[] { Function.CreateArgumentValue(success) };
            }

            node.AddFunction(nodeIdentifier, new[] { Function.CreateBooleanArgument("enable") }, new[] { Function.CreateBooleanArgument("success") }, Func);
        }

        private void AddHangupFunction(EmberNode node, ValueType nodeIdentifier)
        {
            async Task<GlowValue[]> HangupFunction(GlowValue[] args)
            {
                string sipAddress = node.GetStringParameterValue(CodecSlotNodeIdentifiers.SipId); // H�mtar SIP-adress fr�n node med detta namn
                bool success = false;

                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("Hangup can't be invoked. Empty sip address");
                }
                else
                {
                    log.Debug("Hanging up call on {0}", sipAddress);
                    success = await CodecControlService.HangupAsync(sipAddress);
                }

                return new[] { Function.CreateArgumentValue(success) };
            }

            node.AddFunction(
                nodeIdentifier,
                null,
                new[] { Function.CreateBooleanArgument("success") },
                HangupFunction
            );
        }

        private void AddConnectedToTurnInputsOnOff(EmberNode node, ValueType nodeIdentifier, bool enable)
        {
            async Task<GlowValue[]> TurnInputsOnOffFunction(GlowValue[] args)
            {
                string sipAddress = node.GetStringParameterValue(CodecSlotNodeIdentifiers.ConnectedToSipId); // H�mtar SIP-adress fr�n node med detta namn
                bool success = false;

                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("Function can't be invoked. Empty sip address");
                }
                else
                {
                    // Get inputs where ControlThis is true
                    var inputNodes = node.GetChildNode(CodecSlotNodeIdentifiers.ConnectedToInputs).ChildNodes().ToList();
                    var inputsToControlNodes = inputNodes.Where(c => c.GetBoooleanParameterValue(CodecSlotInputIdentifiers.ControlThis));
                    var inputList = inputsToControlNodes.Select(n => n.Number).ToList();
                    var request = new BatchInputEnableRequest()
                    {
                        SipAddress = sipAddress,
                        InputEnableRequests = inputList.Select(i => new InputEnable() { Input = i, Enabled = enable }).ToList()
                    };

                    log.Debug($"Sending BatchSetInputEnabled to CodecControl: {request}");
                    var response = await CodecControlService.BatchSetInputEnabledAsync(request);

                    if (response != null)
                    {
                        log.Debug($"Received response for BatchSetInputEnabled from CodecControl: {response}");

                        // Update enable on nodes
                        response.Inputs.ForEach(i => inputNodes
                            .First(n => n.Number == i.Input)
                            .UpdateParameter(CodecSlotInputIdentifiers.IsEnabled, i.Enabled));

                        success = true;
                    }
                }

                return new[] { Function.CreateArgumentValue(success) };
            }

            node.AddFunction(
                nodeIdentifier,
                null,
                new[] { Function.CreateBooleanArgument("success") },
                TurnInputsOnOffFunction
            );

        }

        private void AddRefreshConnectedToInputValuesFunction(EmberNode node, ValueType nodeIdentifier, int input)
        {
            async Task<GlowValue[]> RefreshFunction(GlowValue[] args)
            {
                string sipAddress = node.ParentNode().ParentNode().GetStringParameterValue(CodecSlotNodeIdentifiers.ConnectedToSipId);

                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("Function can't be invoked. No SIP address");
                    return Function.CreateResult(false);
                }

                log.Debug("Getting gain for input {0} on {1}", input, sipAddress);

                var response = await CodecControlService.GetInputGainAndEnabledAsync(sipAddress, input);

                if (response == null)
                {
                    log.Warn($"Unable to get input gain and enabled for input {input} on {sipAddress}");
                    return Function.CreateResult(false);
                }

                // Update parameters
                node.UpdateParameter(CodecSlotInputIdentifiers.CurrentGain, response.GainLevel);
                node.UpdateParameter(CodecSlotInputIdentifiers.IsEnabled, response.Enabled);

                return Function.CreateResult(true);
            }

            node.AddFunction(nodeIdentifier,
                null,
                new[] { Function.CreateBooleanArgument("success") },
                RefreshFunction);
        }

        private void AddConnectedToEnableFunction(EmberNode node, ValueType nodeIdentifier, int input)
        {
            async Task<GlowValue[]> EnableFunction(GlowValue[] args)
            {
                string sipAddress = node.ParentNode().ParentNode().GetStringParameterValue(CodecSlotNodeIdentifiers.ConnectedToSipId);
                var result = false;
                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("Function can't be invoked. No SIP address");
                }
                else
                {
                    bool active = args[0].Boolean;
                    log.Debug("Sending SetInputEnable to CodecControl. Setting input enabled to {0} on input {1} on {2}", active, input, sipAddress);
                    InputEnabledResponse setInputEnabledResponse = await CodecControlService.SetInputEnabledAsync(sipAddress, input, active);
                    log.Debug($"Received response for SetInputEnabled from CodecControl. Active={setInputEnabledResponse?.Enabled}");
                    if (setInputEnabledResponse != null && setInputEnabledResponse.Enabled == active)
                    {
                        result = true;
                        node.UpdateParameter(CodecSlotInputIdentifiers.IsEnabled, setInputEnabledResponse.Enabled);
                    }
                }
                return new[] { Function.CreateArgumentValue(result) };
            }

            node.AddFunction(nodeIdentifier,
                new[] { Function.CreateBooleanArgument("enable") },
                new[] { Function.CreateBooleanArgument("success") },
                EnableFunction);
        }

        private void AddConnectedToIncreaseDecreaseFunction(EmberNode node, ValueType nodeIdentifier, int input,
            Func<string, int, Task<InputGainLevelResponse>> gainFunction)
        {
            async Task<GlowValue[]> IncreaseDecreaseGainFunction(GlowValue[] args)
            {
                string sipAddress = node.ParentNode().ParentNode().GetStringParameterValue(CodecSlotNodeIdentifiers.ConnectedToSipId);
                var result = false;
                if (string.IsNullOrEmpty(sipAddress))
                {
                    log.Info("Function can't be invoked. No SIP address");
                }
                else
                {
                    log.Debug("Changing gain on input {0} on {1}", input, sipAddress);
                    var response = await gainFunction(sipAddress, input);
                    if (response != null)
                    {
                        result = true;
                        node.UpdateParameter(CodecSlotInputIdentifiers.CurrentGain, response.GainLevel);
                    }
                }

                return new[] { Function.CreateArgumentValue(result) };
            }

            node.AddFunction(nodeIdentifier,
                null,
                new[] { Function.CreateBooleanArgument("success") },
                IncreaseDecreaseGainFunction);
        }
        */
        #endregion

        #region Update methods for initiation or intervals
        /*
        private void UpdateDisplayTypeNode(Node node, WebGuiUrls webGuiUrls)
        {
            if (webGuiUrls != null)
            {
                node.UpdateParameter(DisplayNodeIdentifiers.PlaylistUrl, webGuiUrls.PlaylistUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.JinglesUrl, webGuiUrls.JinglesUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.Player01Url, webGuiUrls.Player01Url);
                node.UpdateParameter(DisplayNodeIdentifiers.Player02Url, webGuiUrls.Player02Url);
                node.UpdateParameter(DisplayNodeIdentifiers.Player03Url, webGuiUrls.Player03Url);
                node.UpdateParameter(DisplayNodeIdentifiers.PrelistenUrl, webGuiUrls.PrelistenUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.Dialer01Url, webGuiUrls.Dialer01Url);
                node.UpdateParameter(DisplayNodeIdentifiers.Dialer02Url, webGuiUrls.Dialer02Url);
                node.UpdateParameter(DisplayNodeIdentifiers.Dialer03Url, webGuiUrls.Dialer03Url);
                node.UpdateParameter(DisplayNodeIdentifiers.Dialer04Url, webGuiUrls.Dialer04Url);
                node.UpdateParameter(DisplayNodeIdentifiers.RemoteConnectorUrl, webGuiUrls.RemoteConnectorUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.StudioSignageUrl, webGuiUrls.StudioSignageUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.ReadonlyPlaylistUrl, webGuiUrls.ReadonlyPlaylistUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.SidekickScreenUrl, webGuiUrls.SidekickScreenUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.DeskPlaylistUrl, webGuiUrls.DeskPlaylistUrl);
                node.UpdateParameter(DisplayNodeIdentifiers.NetIo01Url, webGuiUrls.NetIo01Url);
                node.UpdateParameter(DisplayNodeIdentifiers.NetIo02Url, webGuiUrls.NetIo02Url);
                node.UpdateParameter(DisplayNodeIdentifiers.NetIo03Url, webGuiUrls.NetIo03Url);
                node.UpdateParameter(DisplayNodeIdentifiers.NetIo04Url, webGuiUrls.NetIo04Url);
            }
        }

        public void UpdatePoolCodecNodes(IList<CodecStatus> codecStatusList)
        {
            List<EmberNode> poolCodecNodes = GetCodecPoolUnitNodes();

            foreach (var poolCodecNode in poolCodecNodes)
            {
                // Match information from CCM
                var sipId = poolCodecNode.GetStringParameterValue(PoolCodecNodeIdentifiers.SipId.ToString());
                var codecStatus = codecStatusList.FirstOrDefault(a => a.SipAddress == sipId);
                UpdatePoolCodecNode(poolCodecNode, codecStatus);
            }
        }

        public void UpdatePoolTxSourceNodes()
        {
            List<EmberNode> poolTxNodes = GetTxPoolUnitNodes();

            // Set the boolean Prepare / OnAir value
            foreach (var poolSourcesNodes in poolTxNodes)
            {
                foreach (var poolTxNode in poolSourcesNodes.ChildNodes())
                {
                    var prepareValue = poolTxNode.GetStringParameterValue(TxSourceIdentifiers.Prepare) == "True" || poolTxNode.GetStringParameterValue(TxSourceIdentifiers.Prepare) == "true";
                    poolTxNode.UpdateParameter(TxSourceIdentifiers.PrepareBool, prepareValue);

                    var onAirValue = poolTxNode.GetStringParameterValue(TxSourceIdentifiers.OnAir) == "True" || poolTxNode.GetStringParameterValue(TxSourceIdentifiers.OnAir) == "true";
                    poolTxNode.UpdateParameter(TxSourceIdentifiers.OnAirBool, onAirValue);
                }
            }
        }

        public void UpdateStudioCodecSlots(IList<CodecStatus> codecStatusList)
        {
            List<EmberNode> codecSlotNodes = GetStudioCodecSlots();

            foreach (var codecSlotNode in codecSlotNodes)
            {
                var sipId = codecSlotNode.GetStringParameterValue(CodecSlotNodeIdentifiers.SipId);
                if (!string.IsNullOrEmpty(sipId))
                {
                    // Allokerad slot.
                    var codecStatus = codecStatusList.FirstOrDefault(a => a.SipAddress == sipId);
                    UpdateCodecSlotNode(codecSlotNode, codecStatus);
                }
            }
        }

        public void UpdateStudioCodecSlots(CodecStatus codecStatus)
        {
            // Uppdatera kodarslottar för studio
            GetStudioCodecNodesWithSipAddress(codecStatus.SipAddress).ForEach(codecNode => UpdateCodecSlotNode(codecNode, codecStatus));
        }

        private void UpdateCodecSlotNode(EmberNode node, CodecStatus codecStatus)
        {
            if (codecStatus != null)
            {
                var isInCall = codecStatus.State == CodecState.InCall;

                node.UpdateParameter(CodecSlotNodeIdentifiers.DisplayName, codecStatus.PresentationName ?? string.Empty);
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsInCall, isInCall);
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsOutgoingCall, codecStatus.IsCallingPart);
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToSipId, codecStatus.ConnectedToSipAddress ?? string.Empty);
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayName, codecStatus.ConnectedToPresentationName ?? string.Empty);
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToLocation, codecStatus.ConnectedToLocation ?? string.Empty);
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayNameAndLocation, codecStatus.ConnectedToDisplayNameAndLocation() ?? string.Empty);

                if (!isInCall)
                {
                    node.UpdateParameter(CodecSlotNodeIdentifiers.IsInPhoneCall, false);
                }
                else
                {
                    // Det tar lite tid f�r kodaren att l�sa in p� r�tt kodningsalgoritm, och fr�gar vi f�r tidigt f�r vi fel resultat.
                    // D�rf�r v�ntar vi lite med att l�sa ut IsInPhoneCall.
                    Task.Delay(1000).ContinueWith(async t =>
                    {
                        var audioMode = await CodecControlService.GetAudioModeAsync(codecStatus.SipAddress);
                        var isInPhoneCall = audioMode != null && audioMode.IsPhoneCall;
                        node.UpdateParameter(CodecSlotNodeIdentifiers.IsInPhoneCall, isInPhoneCall);
                    });
                }
            }
        }

        public void UpdatePoolCodecs(CodecStatus codecStatus)
        {
            GetPoolCodecNodesWithSipAddress(codecStatus.SipAddress).ForEach(codecNode => UpdatePoolCodecNode(codecNode, codecStatus));
        }

        private void UpdatePoolCodecNode(EmberNode node, CodecStatus codecStatus)
        {
            if (codecStatus != null)
            {
                var displayName = !string.IsNullOrEmpty(codecStatus.PresentationName) ? codecStatus.PresentationName : codecStatus.SipAddress;
                node.UpdateParameter(PoolCodecNodeIdentifiers.DisplayName, displayName);
                node.UpdateParameter(PoolCodecNodeIdentifiers.IsRegistered, codecStatus.State != CodecState.NotRegistered);
                node.UpdateParameter(PoolCodecNodeIdentifiers.IsInCall, codecStatus.State == CodecState.InCall);
            }
        }

        public void UpdatePoolCodecsOwner()
        {
            // Allokerade kodarslots
            var allocatedCodecSlots = GetStudioCodecSlots()
                .Where(n => !string.IsNullOrEmpty(n.GetStringParameterValue(CodecSlotNodeIdentifiers.SipId.ToString())))
                .ToList();

            // Uppdatera owner p� pool-kodar-noderna.
            foreach (var poolCodecNode in GetCodecPoolUnitNodes())
            {
                string poolCodecSipId = poolCodecNode.GetStringParameterValue(PoolCodecNodeIdentifiers.SipId.ToString());
                EmberNode allocatedCodecSlot = allocatedCodecSlots.FirstOrDefault(s => s.HasStringParameterWithValue(CodecSlotNodeIdentifiers.SipId, poolCodecSipId));

                string owner = string.Empty;
                if (allocatedCodecSlot != null)
                {
                    var studioNode = allocatedCodecSlot.ParentNode().ParentNode();
                    owner = studioNode.GetStringParameterValue(StudioNodeIdentifiers.DisplayName);
                    if (string.IsNullOrEmpty(owner))
                    {
                        owner = studioNode.GetStringParameterValue(StudioNodeIdentifiers.StudioId);
                    }
                }

                bool changed = poolCodecNode.UpdateParameter(PoolCodecNodeIdentifiers.Owner, owner);
                if (changed)
                {
                    log.Info("Updating pool codec node {0}. Setting Owner to {1}", poolCodecNode.IdentifierPath, owner);
                }
            }
        }

        public void UpdateInputGainAndEnabled(string identifierPath, AudioStatusResponse audioStatus)
        {
            var connectedToInputsNode = GetElement<EmberNode>(identifierPath)?.GetChildNode(CodecSlotNodeIdentifiers.ConnectedToInputs);

            if (connectedToInputsNode != null)
            {
                foreach (var inputStatus in audioStatus.InputStatus)
                {
                    var inputNode = connectedToInputsNode.ChildNodes()
                        .FirstOrDefault(n => n.HasIntegerParameterWithValue(CodecSlotInputIdentifiers.InputNumber, inputStatus.Index));
                    if (inputNode != null)
                    {
                        inputNode.UpdateParameter(CodecSlotInputIdentifiers.IsEnabled, inputStatus.Enabled);
                        inputNode.UpdateParameter(CodecSlotInputIdentifiers.CurrentGain, inputStatus.GainLevel);
                    }
                }
            }
        }
        */
        #endregion

        #region Handlers on change
        /// <summary>
        /// Triggered on all changes to parameter's in EmBER+ tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnEmberTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            try
            {
                ParameterBase changedParameter = e.Root.FirstOrDefault() is GlowQualifiedParameter glowParameter
                        ? GetElement<ParameterBase>(glowParameter.Path)
                        : null;

                if (changedParameter != null)
                {
                    //log.Debug("EmberTree node {0} changed", changedParameter.IdentifierPath);

                    Task.Run(async () =>
                    {
                        /*await HandleSlotSipIdUpdate(changedParameter);
                        await HandleSlotIsOnAirUpdate(changedParameter);
                        await HandleSlotIsInCallUpdate(changedParameter);
                        await HandleSlotConnectedToSipIdUpdate(changedParameter);
                        await HandleLoggedInUserUpdate(changedParameter);
                        await HandleStudioUpdate(changedParameter);
                        await HandleTxUpdate(changedParameter);
                        await HandleModifyRegionUpdate(changedParameter);*/
                        TreeChanged?.Invoke(this, new EventArgs());
                    });
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when handling ember tree change");
            }
        }

        /*
        private async Task HandleSlotSipIdUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(string.Format(SlotParameterPattern, CodecSlotNodeIdentifiers.SipId));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success)
            {
                var slotNode = (Node)parameter.Parent;
                var sipAddress = slotNode.GetStringParameterValue(CodecSlotNodeIdentifiers.SipId);
                log.Debug("Slot SipAddress on node {0} changed to \"{1}\"", slotNode.IdentifierPath, sipAddress);

                if (string.IsNullOrEmpty(sipAddress))
                {
                    // Avallokering. Rensa kodarslot-noden
                    ClearCodecSlot(slotNode);
                }
                else
                {
                    // Allokering
                    var codecStatus = await CcmService.GetCodecStatusAsync(sipAddress);
                    UpdateStudioCodecSlots(codecStatus);
                }

                UpdatePoolCodecsOwner();

                await Task.Delay(100).ContinueWith(t =>
                {
                    var slotInfo = SlotInfo.CreateFromNode(slotNode);
                    CodecSlotChanged?.Invoke(slotInfo);
                });

            }
        }

        private async Task HandleSlotIsOnAirUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(string.Format(SlotParameterPattern, CodecSlotNodeIdentifiers.IsOnAir));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success)
            {
                var slotNode = parameter.Parent as Node;
                await Task.Delay(100).ContinueWith(t =>
                {
                    var slotInfo = SlotInfo.CreateFromNode(slotNode);
                    log.Debug("Slot IsOnAir for {0}/{1} changed to \"{2}\"", slotInfo.StudioNodeIdentifier, slotInfo.Slot, slotInfo.IsOnAir);
                    CodecSlotChanged?.Invoke(slotInfo);
                });
            }
        }

        private async Task HandleSlotIsInCallUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(string.Format(SlotParameterPattern, CodecSlotNodeIdentifiers.IsInCall));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success)
            {
                var slotNode = parameter.Parent as Node;
                await Task.Delay(100).ContinueWith(t =>
                {
                    var slotInfo = SlotInfo.CreateFromNode(slotNode);
                    log.Debug("Slot IsInCall for {0}/{1} changed to \"{2}\"", slotInfo.StudioNodeIdentifier, slotInfo.Slot, slotInfo.IsInCall);
                    CodecSlotChanged?.Invoke(slotInfo);
                });

            }
        }

        private async Task HandleSlotConnectedToSipIdUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(string.Format(SlotParameterPattern, CodecSlotNodeIdentifiers.ConnectedToSipId));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success)
            {
                var stringParameter = parameter as StringParameter;
                var sipAddress = stringParameter.Value;

                var identifierPath = parameter.Parent.IdentifierPath;
                log.Debug($"ConnectedToSipId changed to {sipAddress} on node {identifierPath}");
                ConnectedToChanged?.Invoke(identifierPath, sipAddress);
            }
        }

        private async Task HandleTxUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(TxSourceParameterPattern);
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success)
            {
                var parameterName = match.Groups["parameter"].Value;

                if (_parametersToTriggerTxEvent.Contains(parameterName))
                {
                    // Set the boolean Prepare / OnAir value
                    var stringNode = (StringParameter)parameter;
                    var parameterValue = stringNode.Value == "True" || stringNode.Value == "true";

                    // Check if input string is accepted string value
                    bool isValidParameterValue = stringNode.Value == "True" || stringNode.Value == "true" ||
                                                stringNode.Value == "False" || stringNode.Value == "false";

                    Node txNode = stringNode.Parent as Node;

                    if (parameterName == "OnAir")
                    {
                        txNode.UpdateParameter(TxSourceIdentifiers.OnAirBool, parameterValue);
                        if (!isValidParameterValue)
                        {
                            txNode.UpdateParameter(TxSourceIdentifiers.OnAir, parameterValue.ToString());
                        }
                    }
                    else if (parameterName == "Prepare")
                    {
                        txNode.UpdateParameter(TxSourceIdentifiers.PrepareBool, parameterValue);
                        if (!isValidParameterValue)
                        {
                            txNode.UpdateParameter(TxSourceIdentifiers.Prepare, parameterValue.ToString());
                        }
                    }

                    await Task.Run(() =>
                    {
                        TxChanged?.Invoke(this, new EventArgs());
                    });
                    return;
                }
            }

            // Tx.TA
            var taRegExp = new Regex(TxTaParameterPattern);
            var taMatch = taRegExp.Match(parameter.IdentifierPath);
            if (taMatch.Success)
            {
                await Task.Run(() =>
                {
                    TxChanged?.Invoke(this, new EventArgs());
                });
                return;
            }

            // Nationalfeed
            var nationalFeedRegExp = new Regex(TxNationalFeedParameterPattern);
            var nationalFeedMatch = nationalFeedRegExp.Match(parameter.IdentifierPath);
            if (nationalFeedMatch.Success)
            {
                await Task.Run(() =>
                {
                    TxChanged?.Invoke(this, new EventArgs());
                });
            }
        }

        private async Task HandleStudioUpdate(ParameterBase parameter)
        {
            var regExp = new Regex(StudioPattern);
            var match = regExp.Match(parameter.IdentifierPath);
            if (match.Success)
            {
                var studioNodeIdentifier = match.Groups["studio"].Value;
                log.Debug("HandleStudioInfoUpdate. RegExp match for {0}", parameter.IdentifierPath);

                var studioNode = GetStudioNodes().FirstOrDefault(s => s.Identifier == studioNodeIdentifier);
                var studioId = studioNode?.GetStringParameterValue(StudioNodeIdentifiers.StudioId) ?? string.Empty;
                await Task.Run(() =>
                {
                    StudioInfoChanged?.Invoke(studioId);
                });
            }
        }

        private async Task HandleLoggedInUserUpdate(ParameterBase parameter)
        {
            // Uppdaterar namn p� inloggad anv�ndare d� inloggningsnamnet �ndras.

            var regExp = new Regex(string.Format(StudioParameterPattern, StudioNodeIdentifiers.LoggedInUser));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success && parameter is StringParameter)
            {
                // Get user name
                var stringNode = (StringParameter)parameter;
                var userName = stringNode.Value;

                var displayName = "";
                if (!string.IsNullOrEmpty(userName))
                {
                    displayName = UserHelper.GetDisplayNameFromActiveDirectory(userName);
                    log.Info("User name changed to " + userName + ". Display name: " + displayName);
                }

                // Update display name node
                Node studioNode = parameter.Parent as Node;
                studioNode.UpdateParameter(StudioNodeIdentifiers.LoggedInDisplayName, displayName);
            }
        }

        private async Task HandleModifyRegionUpdate(ParameterBase parameter)
        {
            // Validate input
            var regExp = new Regex(string.Format(StudioParameterPattern, StudioNodeIdentifiers.ModifyRegion));
            var match = regExp.Match(parameter.IdentifierPath);

            if (match.Success && parameter is StringParameter)
            {
                // Get new region
                string modifyRegion = ((StringParameter)parameter).Value;

                if (!string.IsNullOrEmpty(modifyRegion) && _config.Regions.Any(x => x.Name == modifyRegion))
                {
                    // Update region node
                    Node studioNode = parameter.Parent as Node;
                    if (modifyRegion.ToLower() == "default" || modifyRegion.ToLower() == "noregion")
                    {
                        studioNode.UpdateParameter(StudioNodeIdentifiers.Region, ""); // On default region we set an empty region
                    }
                    else
                    {
                        studioNode.UpdateParameter(StudioNodeIdentifiers.Region, modifyRegion);
                    }
                    log.Info("Region changed to " + modifyRegion + ".");
                }
            }
        }
        */
        #endregion

        #region Set methods
        /*
        public void SetMaintenanceMode(string studioId, bool inMaintenance)
        {
            var studioNode = GetStudioNodeByStudioId(studioId);
            studioNode?.UpdateParameter(StudioNodeIdentifiers.InMaintenance, inMaintenance);
        }
        */
        #endregion

        #region Get methods
        /*
        public SlotInfo GetSlotInfo(string studio, string slot)
        {
            var slotNode = GetSlotNode(studio, slot);
            return SlotInfo.CreateFromNode(slotNode);
        }

        public SlotInfo GetSlotInfoByStudioIdentifier(string studioIdentifier, string slot)
        {
            var slotNode = GetSlotNodeByStudioIdentifier(studioIdentifier, slot);
            return SlotInfo.CreateFromNode(slotNode);
        }

        public string GetStudioSlotSipAddres(string studio, string slot)
        {
            var slotNode = GetSlotNode(studio, slot);
            var sipAddress = slotNode?.GetStringParameterValue(CodecSlotNodeIdentifiers.SipId) ?? string.Empty;
            return sipAddress;

        }

        public StudioInfoLight GetStudioInfo(string studio)
        {
            var studioNode = GetStudioNodeByStudioId(studio);
            return studioNode != null ? new StudioInfoLight(studioNode) : null;
        }

        public List<StudioInfoLight> GetStudioInfoList()
        {
            var studioNodes = GetStudioNodes();
            return studioNodes.Select(s => new StudioInfoLight(s)).ToList();
        }

        public List<StudioStates> GetStudioStatesList()
        {
            var studioNodes = GetStudioNodes();
            return studioNodes.Select(s => new StudioStates(s)).ToList();
        }

        public List<SlotInfo> GetStudioSlots(string studio)
        {
            var matchingStudioNode = GetStudioNodeByStudioId(studio);
            var codecSlotNodes = matchingStudioNode?.GetChildNode(StudioNodeIdentifiers.CodecSlots)?.ChildNodes();
            return codecSlotNodes?.Select(SlotInfo.CreateFromNode).ToList() ?? new List<SlotInfo>();
        }

        private Node GetStudioNodeByStudioId(string studioId)
        {
            return GetStudioNodes().FirstOrDefault(s => s.HasStringParameterWithValue(StudioNodeIdentifiers.StudioId, studioId));
        }

        private Node GetSlotNodeByStudioIdentifier(string studioNodeIdentifier, string slot)
        {
            var studioNode = GetStudioNodes().FirstOrDefault(s => s.Identifier == studioNodeIdentifier);
            return GetSlotNodeOfStudioNode(studioNode, slot);
        }

        private Node GetSlotNode(string studioId, string slot)
        {
            var studioNode = GetStudioNodeByStudioId(studioId);
            return GetSlotNodeOfStudioNode(studioNode, slot);
        }

        private Node GetSlotNodeOfStudioNode(Node studioNode, string slot)
        {
            slot = (slot ?? string.Empty).Trim().ToLower();
            var slotNodes = studioNode?.GetChildNode(StudioNodeIdentifiers.CodecSlots)?.ChildNodes();
            return slotNodes?.FirstOrDefault(s => s.Identifier.ToLower() == slot);
        }

        private IEnumerable<Node> GetStudioNodes()
        {
            return GetElement<EmberNode>(TreePathForStudios).ChildNodes();
        }

        public async Task<List<string>> CheckAvailableStudiosAsync()
        {
            // S�tt alla ActiveStudioConnections till false
            GetStudioNodes().ToList().ForEach(s =>
            {
                s.UpdateParameter(StudioNodeIdentifiers.ActiveStudioConnection, false);
            });

            // V�nta p� att VisTool uppdaterar v�rdena
            await Task.Delay(2000);

            return GetAvailableStudios();
        }

        private List<string> GetAvailableStudios()
        {
            var availableStudioNodes = GetStudioNodes()
                .Where(n => n.GetBoooleanParameterValue(StudioNodeIdentifiers.ActiveStudioConnection) == false).ToList();
            return availableStudioNodes.Select(s => s.GetStringParameterValue(StudioNodeIdentifiers.StudioId)).ToList();
        }

        public List<TxInfo> GetTxInfos()
        {
            // Returnerar lista med info om alla TX:ar och vilka studior som �r aktiva p� dem.
            var txNodes = GetElement<EmberNode>(TreePathForTxPools).ChildNodes().ToList();
            var txList = txNodes.Select(TxInfo.CreateFromTxNode).ToList();
            return txList;
        }

        public List<TxSourceInfo> GetTxInfoForStudio(string studio)
        {
            if (string.IsNullOrEmpty(studio))
            {
                return new List<TxSourceInfo>();
            }

            var txNodes = GetElement<EmberNode>(TreePathForTxPools).ChildNodes().ToList();

            var list = new List<TxSourceInfo>();
            foreach (var txNode in txNodes)
            {
                // Hitta f�rsta source med detta studio-id.
                var sources = txNode.GetChildNode(TxNodeIdentifiers.Sources);
                var childNodes = sources.ChildNodes();
                var txSource = childNodes.FirstOrDefault(n => n.HasStringParameterWithValue(TxSourceIdentifiers.StudioId, studio));

                if (txSource != null)
                {
                    var txInfo = TxSourceInfo.CreateFromTxSourceNode(txSource);
                    list.Add(txInfo);
                }
            }
            return list;
        }

        private List<EmberNode> GetCodecPoolUnitNodes()
        {
            var allCodecPools = Provider.ProviderRoot.GetChildNode(RootIdentifiers.CodecPools).Children;
            var allUnitsNodes = allCodecPools.Select(p => p.GetChildNode(CodecPoolNodeIdentifiers.Units));
            return allUnitsNodes.SelectMany(p => p.ChildNodes()).OfType<EmberNode>().ToList();
        }

        private List<EmberNode> GetPoolCodecNodesWithSipAddress(string sipAddress)
        {
            return GetCodecPoolUnitNodes().Where(cn => cn.HasStringParameterWithValue(PoolCodecNodeIdentifiers.SipId, sipAddress)).ToList();
        }

        private List<EmberNode> GetTxPoolUnitNodes()
        {
            // Gets all the Sources in TX Pools
            var allTxPools = Provider.ProviderRoot.GetChildNode(RootIdentifiers.TxPool).Children;
            return allTxPools.SelectMany(p => p.ChildNodes()).OfType<EmberNode>().ToList();
        }

        private List<EmberNode> GetStudioCodecNodesWithSipAddress(string sipAddress)
        {
            return GetStudioCodecSlots().Where(cn => cn.HasStringParameterWithValue(CodecSlotNodeIdentifiers.SipId, sipAddress)).ToList();
        }

        private List<EmberNode> GetStudioCodecSlots()
        {
            var studioCodecSlots = GetStudioNodes()
                .Select(c => c.GetChildNode(StudioNodeIdentifiers.CodecSlots))
                .Where(c => c != null)
                .SelectMany(c => c.Children)
                .OfType<EmberNode>()
                .ToList();
            return studioCodecSlots;
        }

        public List<AudioStatusSubscription> GetSipAddressListToSubscribeForAudioStatus()
        {
            return GetStudioCodecSlots()
                .Select(n => n.Children.FirstOrDefault(c => c.Identifier == CodecSlotNodeIdentifiers.ConnectedToSipId.ToString()))
                .OfType<StringParameter>()
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => new AudioStatusSubscription() { IdentifierPath = p.Parent.IdentifierPath, SipAddress = p.Value })
                .ToList();
        }
        */
        #endregion

        #region General methods
        /*
        public void RestoreParameters(List<ParameterInfo> persistedParameters)
        {
            if (persistedParameters == null)
            {
                log.Info("Persisted parameters not found. Will not restore values");
                return;
            }

            try
            {
                foreach (ParameterBase parameter in GetWritableParameters())
                {
                    var persistedParameter = persistedParameters.FirstOrDefault(p => p.Identifier == parameter.IdentifierPath);
                    if (persistedParameter != null)
                    {
                        log.Debug("Setting persisted value for \"{0}\": {1}", parameter.IdentifierPath, persistedParameter.Value);
                        parameter.SetValue(persistedParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when restoring persisted values");
            }
        }

        public void ClearCodecSlot(Node node)
        {
            if (node != null)
            {
                node.UpdateParameter(CodecSlotNodeIdentifiers.DisplayName, "");
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsOnAir, false);
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsInCall, false);
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsInPhoneCall, false);
                node.UpdateParameter(CodecSlotNodeIdentifiers.IsOutgoingCall, false);
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToSipId, "");
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayName, "");
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToLocation, "");
                node.UpdateParameter(CodecSlotNodeIdentifiers.ConnectedToDisplayNameAndLocation, "");
            }
        }

        public bool HasCodecWithSipAddress(string sipAddress)
        {
            return HasPoolCodecWithSipAddress(sipAddress) || HasStudioCodecWithSipAddress(sipAddress);
        }

        private bool HasPoolCodecWithSipAddress(string sipAddress)
        {
            sipAddress = (sipAddress ?? string.Empty).Trim().ToLower();
            return (_poolCodecSipAddresses ?? new List<string>()).Select(s => s.ToLower()).Contains(sipAddress);
        }

        public bool HasStudioCodecWithSipAddress(string sipAddress)
        {
            return GetStudioCodecSlots().Any(cn => cn.HasStringParameterWithValue(CodecSlotNodeIdentifiers.SipId, sipAddress));
        }
        */
        #endregion

    }
}