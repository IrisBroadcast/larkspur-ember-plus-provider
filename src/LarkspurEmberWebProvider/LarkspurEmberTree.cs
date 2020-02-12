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
    public class LarkspurEmberTree : EmberPlusProviderClassLib.EmberTree
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
        public LarkspurEmberTree(int port)
            : base(port)
        {

            // Identity node
            Provider.CreateIdentityNode(0, "NGEmberProvider", "Sveriges Radio", "0.0.1");

            //var child = Provider.AddChildNode(RootIdentifiers.CodecPools);
            //var codecPoolNode = child.AddSubNode(0, $"CodecPool_");
            //codecPoolNode.AddStringParameter(CodecPoolNodeIdentifiers.DisplayName, "me");

            // Utility node
            CreateUtilitiesNode();

            // Everything went well
            log.Debug("Ember provider created and listening on port {0}", port);

            //Provider.dispatcher.GlowRootReady += OnEmberTreeChanged;
        }
        #endregion

        #region Creation & Initiation methods
        private void CreateUtilitiesNode()
        {
            var utilNode = Provider.AddChildNode(RootIdentifiers.Utilities);
            utilNode.AddStringParameter(UtilitiesIdentifiers.Server, Environment.MachineName);
            utilNode.AddStringParameter(UtilitiesIdentifiers.StartTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //Provider.AddChildNode(RootIdentifiers.Studios);
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
                //ParameterBase changedParameter = e.Root.FirstOrDefault() is GlowQualifiedParameter glowParameter
                //        ? GetElement<ParameterBase>(glowParameter.Path)
                //        : null;

                //if (changedParameter != null)
                //{
                //    //log.Debug("EmberTree node {0} changed", changedParameter.IdentifierPath);

                //    TreeChanged?.Invoke(this, new EventArgs());

                //    //Task.Run( () =>
                //    //{
                //    //    /*await HandleSlotSipIdUpdate(changedParameter);
                //    //    await HandleSlotIsOnAirUpdate(changedParameter);
                //    //    await HandleSlotIsInCallUpdate(changedParameter);
                //    //    await HandleSlotConnectedToSipIdUpdate(changedParameter);
                //    //    await HandleLoggedInUserUpdate(changedParameter);
                //    //    await HandleStudioUpdate(changedParameter);
                //    //    await HandleTxUpdate(changedParameter);
                //    //    await HandleModifyRegionUpdate(changedParameter);*/
                //    //    TreeChanged?.Invoke(this, new EventArgs());
                //    //});
                //}
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when handling ember tree change");
            }
        }
        #endregion
    }
}