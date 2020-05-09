#region copyright
/*
 * Larkspur Ember Plus Provider
 *
 * Copyright (c) 2020 Roger Sandholm & Fredrik Bergholtz, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

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
                dispatcher = new Dispatcher { Root = Node.CreateRoot() };
                ProviderRoot = new Node(1, dispatcher.Root, identifier) { Description = description };
                listener = new GlowListener(port, maxPackageLength, dispatcher);

                string message = $"Initializing the EmBER+ provider on port: {port}, identifier: {identifier}, description: {description}";
                Console.WriteLine(message);
                Debug.WriteLine(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: EmberPlusProviderCLassLib / EmberPlusProvider: ", ex.Message);
            }
        }

        public void SetUpFinalListeners()
        {
            string message = $"Setting up final listeners";
            Console.WriteLine(message);
            Debug.WriteLine(message);
            dispatcher.GlowRootReady += OnEmberTreeChanged;
        }

        private async Task OnHandleUtilitiesChanged(ParameterBase parameter)
        {
            //var regExp = new Regex(string.Format("(?<studio>[^/]*)/CodecSlots/(?<slot>[^/]*)/{0}$", RootIdentifiers.Utilities));
            //var match = regExp.Match(parameter.IdentifierPath);

            //if (match.Success)
            //{
            //    var slotNode = parameter.Parent as Node;
            //    await Task.Delay(100).ContinueWith(t =>
            //    {
            //        //var slotInfo = SlotInfo.CreateFromNode(slotNode);
            //        //log.Debug("Slot IsInCall for {0}/{1} changed to \"{2}\"", slotInfo.StudioNodeIdentifier, slotInfo.Slot, slotInfo.IsInCall);
            //        //CodecSlotChanged?.Invoke(slotInfo);

            //    });

            //}

            GpioChanged?.Invoke("MUHAHA");
        }

        protected void OnEmberTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            // Triggered on EmBER+ tree change
            Console.WriteLine("OnEmberTreeChanged");
            try
            {
                TreeChanged?.Invoke(this, new EventArgs()); // TODO: should this be done twice
                var test = e.Root.FirstOrDefault() as GlowQualifiedParameter;
                Console.WriteLine($"HAS VALIDATE {e.Root.HasValidationErrors}");






                //if (test != null)
                //{
                //    Console.WriteLine("YES it issss");
                //    Console.WriteLine($"{test.Path.ToString()}");
                //    var elem = GetElement<ParameterBase>(test.Path);


                //    var element = ProviderRoot.ResolveChild(test.Path, out var dynamicPathHandler);
                //    Console.WriteLine(dynamicPathHandler.ToString());

                //    var slotNode = (Node)element.Parent;
                //    var sipAddress = slotNode.GetBoooleanParameterValue("gpio");
                //        Console.WriteLine($"Muhaha {sipAddress.ToString()}");

                //}
                //ParameterBase changedParameter = e.Root.FirstOrDefault() is GlowQualifiedParameter glowParameter
                //    ? GetElement<ParameterBase>(glowParameter.Path)
                //    : null;

                GlowQualifiedParameter glowParameter = e.Root.FirstOrDefault() as GlowQualifiedParameter;

                ParameterBase changedParameter = GetElement<ParameterBase>(glowParameter?.Path);

                Console.WriteLine("=======");
                if (glowParameter != null)
                {
                    Console.WriteLine($"EmberTree node {glowParameter.Value.ToString()} //IdentifierPath changed. {changedParameter.IdentifierPath}");
                    Debug.WriteLine($"INFO {glowParameter.GetType().ToString()}");
                    Task.Run(async () =>
                    {
                        Console.WriteLine($"EmberTree node {glowParameter.Value.ToString()} //IdentifierPath changed. {changedParameter.IdentifierPath}");
                        await OnHandleUtilitiesChanged(changedParameter);
                        TreeChanged?.Invoke(this, new EventArgs());
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR parsing tree");
                Debug.WriteLine(ex, "ERR");
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
            //var node = new Node((int) number, ProviderRoot, "Utilities");
            var node = AddChildNode(number);
            node.AddBooleanParameter(1, "gpio", this, true);
            node.AddStringParameter(2, "hall", this, true, "default");
            node.AddStringParameter(3, "next", this, true, "default");
        }

        public EmberNode AddChildNode(ValueType identifier)
        {
            return ProviderRoot.AddSubNode(identifier, this);
        }

        public T GetElement<T>(int[] path) where T : class
        {
            var element = ProviderRoot.ResolveChild(path, out var dynamicPathDummyHandler);
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
