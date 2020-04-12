using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmberLib.Framing;
using EmberLib.Glow;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;

namespace EmberPlusProviderClassLib
{
    public class EmberPlusProvider : IDisposable
    {
        public Node ProviderRoot { get; protected set; }
        public Dispatcher dispatcher { get; protected set; }

        protected GlowListener listener;

        /// <summary>
        /// Trigger if any parameter in the EmBER+ tree is changed
        /// </summary>
        public event EventHandler TreeChanged;
        public event Action<string> GpioChanged;

        /// <summary>
        /// Creates the actual EmBER+ provider tree
        /// </summary>
        /// <param name="port">EmBER+ provider port</param>
        /// <param name="identifier">EmBER+ root identifier</param>
        /// <param name="description">EmBER+ root description</param>
        public EmberPlusProvider(int port, string identifier, string description)
        {
            try
            {
                int maxPackageLength = ProtocolParameters.MaximumPackageLength;
                dispatcher = new Dispatcher {Root = Node.CreateRoot()};
                ProviderRoot = new Node(1, dispatcher.Root, identifier) { Description = description };
                listener = new GlowListener(port, maxPackageLength, dispatcher);

                dispatcher.GlowRootReady += OnEmberTreeChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: EmberPlusProviderCLassLib / EmberPlusProvider: ", ex.Message);
            }
        }

        private async Task OnHandleUtilitiesChanged(ParameterBase parameter)
        {
            //var regExp = new Regex(string.Format(SlotParameterPattern, CodecSlotNodeIdentifiers.IsInCall));
            //var match = regExp.Match(parameter.IdentifierPath);

            //if (match.Success)
            //{
            //    var slotNode = parameter.Parent as Node;
            //    await Task.Delay(100).ContinueWith(t =>
            //    {
            //        //var slotInfo = SlotInfo.CreateFromNode(slotNode);
            //        //log.Debug("Slot IsInCall for {0}/{1} changed to \"{2}\"", slotInfo.StudioNodeIdentifier, slotInfo.Slot, slotInfo.IsInCall);
            //        //CodecSlotChanged?.Invoke(slotInfo);
            //        
            //    });

            //}
            GpioChanged?.Invoke("MUHAHA");
        }

        protected void OnEmberTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            // Triggas d� Embertr�det �ndrats.

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
                        await OnHandleUtilitiesChanged(changedParameter);
                        TreeChanged?.Invoke(this, new EventArgs());
                    });
                }
            }
            catch (Exception ex)
            {
                //log.Warn(ex, "Exception when handling ember tree change");
            }
        }

        public void CreateIdentityNode(ValueType number, string product, string company, string version)
        {
            var identity = new Node((int)number, ProviderRoot, "identity")
            {
                SchemaIdentifier = "de.l-s-b.emberplus.identity"
            };

            identity.AddStringParameter(1, "product", this, false, product);
            identity.AddStringParameter(2, "company", this, false, company);
            identity.AddStringParameter(3, "version", this, false, version);
        }

        public void InitializeAllNodes(ValueType number)
        {
            var node = new Node((int) number, ProviderRoot, "Utilities");
            node.AddBooleanParameter(4, "gpio", this, true);
        }

        public EmberNode AddChildNode(ValueType identifier)
        {
            return ProviderRoot.AddSubNode(identifier, this);
        }

        public T GetElement<T>(int[] path) where T : class
        {
            var element = ProviderRoot.ResolveChild(path, out var dynamicPathHandler);
            return element as T;
        }

        public T GetElement<T>(string identifierPath) where T : class
        {
            var element = ProviderRoot.ResolveChild(identifierPath);
            return element as T;
        }

        public IList<ParameterBase> GetWritableParameters()
        {
            return ProviderRoot.GetWritableChildParameters().ToList();
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }
        }
    }
}
