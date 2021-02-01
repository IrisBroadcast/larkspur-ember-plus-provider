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
using System.Security.Cryptography.X509Certificates;
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
        public delegate void ChangedTreeUpdateEventHandler(string identifierPath, dynamic value, int[] path);
        public event ChangedTreeUpdateEventHandler ChangedTreeEvent;

        /// <summary>
        /// Trigger for changes in an Matrix
        /// </summary>
        public delegate void MatrixConnectionEventHandler(string identifierPath, GlowConnection connection, int[] path);
        public event MatrixConnectionEventHandler MatrixConnectionEvent;

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
                dispatcher = new Dispatcher(); // { Root = Node.CreateRoot() };
                ProviderRoot = new Node(1, dispatcher.Root, identifier) { Description = description };
                listener = new GlowListener(port, maxPackageLength, dispatcher);

                string message = $"EmberPlusProvider: Initializing the EmBER+ provider on port: {port}, identifier: {identifier}, description: {description}";
                //Console.WriteLine(message);
                //Debug.WriteLine(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EmberPlusProvider: Exception: ", ex.Message);
            }
        }

        public void SetUpFinalListeners()
        {
            Debug.WriteLine($"EmberPlusProvider: Setting up final listeners");
            dispatcher.GlowRootReady += OnEmberTreeChanged;
        }

        protected void OnEmberTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            // Triggered on EmBER+ tree change
            Debug.WriteLine("EmberPlusProvider: OnEmberTreeChanged");
            try
            {
                switch(e.Root.FirstOrDefault())
                {
                    case GlowQualifiedParameter gqp:
                        ParameterBase changedParameter = GetElement<ParameterBase>(gqp?.Path);

                        Debug.WriteLine($"EmberPlusProvider: EmberTree node {gqp.Value.ToString()} //IdentifierPath changed. {changedParameter?.IdentifierPath}");
                        Task.Run(async () =>
                        {
                            await OnHandleValuesChanged(changedParameter);

                            // TODO: Add event for saving tree
                        });
                        break;

                    case GlowQualifiedMatrix gqm:
                        Element changedElement = GetElement<Element>(gqm?.Path);

                        Debug.WriteLine($"EmberPlusProvider: EmberTree node {changedElement?.Identifier} //IdentifierPath changed. {changedElement?.IdentifierPath}");
                        Task.Run(async () =>
                        {
                            foreach (GlowConnection connection in gqm.TypedConnections)
                            {
                                MatrixConnectionEvent?.Invoke(changedElement.IdentifierPath, connection, changedElement.Path); 
                            }
                            
                            // TODO: Add event for saving tree
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EmberPlusProvider: ERROR parsing tree");
                Debug.Write(ex);
            }
        }

        private async Task OnHandleValuesChanged(ParameterBase parameter)
        {
            // Path
            var identifierPath = parameter.IdentifierPath;

            // Check if it is string parameter
            if (parameter is StringParameter stringParameter)
            {
                ChangedTreeEvent?.Invoke(identifierPath, stringParameter.Value, stringParameter.Path);
            }

            if (parameter is BooleanParameter boolParameter)
            {
                ChangedTreeEvent?.Invoke(identifierPath, boolParameter.Value, boolParameter.Path);
            }

            if (parameter is IntegerParameter intParameter)
            {
                ChangedTreeEvent?.Invoke(identifierPath, (int)intParameter.Value, intParameter.Path);
            }
        }

        public void CreateIdentityNode(ValueType number, string product, string company, string version = "0.0.0", string serial = "0F", string role = "unknown")
        {
            var identity = new Node((int)number, ProviderRoot, "identity")
            {
                SchemaIdentifier = "de.l-s-b.emberplus.identity"
            };

            identity.AddStringParameter(1, "product", this, false, product);
            identity.AddStringParameter(2, "company", this, false, company);
            identity.AddStringParameter(3, "serial", this, false, serial);
            identity.AddStringParameter(4, "version", this, false, version);
            identity.AddStringParameter(5, "role", this, false, role);
        }

        public enum MockEnumParameter
        {
            FirstEnum = 0,
            SecondEnum,
            ThirdEnum,
            ForthEnum
        }

        public EmberNode AddChildNode(int index, string identifier)
        {
            return ProviderRoot.AddSubNode(index, identifier, this);
        }

        public EmberNode AddChildNode(ValueType identifier)
        {
            return ProviderRoot.AddSubNode((int)identifier, identifier.ToString().Replace("_", " "), this);
        }

        public T GetElement<T>(int[] path) where T : class
        {
            var element = dispatcher.Root.ResolveChild(path, out var dynamicPathDummyHandler);
            return element as T;
        }

        public T GetElement<T>(string identifierPath) where T : class
        {
            var element = dispatcher.Root.ResolveChild(identifierPath);
            return element as T;
        }

        public IList<ParameterBase> GetWritableParameters()
        {
            return dispatcher.Root.GetWritableChildParameters().ToList();
        }

        public IList<ParameterBase> GetChildParameterElements()
        {
            return dispatcher.Root.GetAllChildParameters().ToList();
        }

        public IList<Matrix> GetChildMatrixElements()
        {
            return dispatcher.Root.GetAllChildMatrices().ToList();
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
