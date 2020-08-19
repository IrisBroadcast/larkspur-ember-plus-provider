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
using System.Threading;
using System.Threading.Tasks;
using EmberLib.Glow;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Helpers;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using LarkspurEmberWebProvider.Hubs;
using NLog;
using LarkspurEmberWebProvider.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace LarkspurEmberWebProvider
{
    public class LarkspurEmberEngine : BackgroundService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static LarkspurEmberEngine SingleInstance { get; private set; }
        private static EmberPlusProvider _emberTree;
        private static OneToNBlindSourceMatrix _gpioMatrix;

        private readonly ApplicationSettings _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<LarkspurHub, ILarkspurHub> _websocketHub;

        public bool EmberTreeState = false;

        public LarkspurEmberEngine(
            IOptions<ApplicationSettings> configuration,
            IServiceProvider serviceProvider,
            IHubContext<LarkspurHub, ILarkspurHub> websocketHub)
        {
            _configuration = configuration.Value;
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
                    Debug.WriteLine("Initializing EmBER+ tree");

                    // Initiate EmBER+ tree
                    _emberTree = new EmberPlusProvider(
                        _configuration.EmberTree.Port,
                        _configuration.EmberTree.Identifier,
                        _configuration.EmberTree.Description);
                    
                    _emberTree.ChangedTreeEvent += EmberTreeOnTreeDataAsync;
                    _emberTree.MatrixConnectionEvent += ReceivedMatrixConnectionEvent;

                    _emberTree.CreateIdentityNode(
                        RootIdentifiers.Identity,
                        _configuration.EmberTree.Product,
                        _configuration.EmberTree.Company,
                        _configuration.EmberTree.Version);

                    // Get saved values
                    //var template = TemplateParserHelper.ParseTemplateJsonFile(_configuration.EmberTree.TreeTemplateFile);

                    // Initialize the actual tree
                    var node = _emberTree.AddChildNode(RootIdentifiers.Utilities);
                    node.AddBooleanParameter(1, "booleanParam", _emberTree, true, true);
                    node.AddStringParameter(2, "stringParam", _emberTree, true, "default");
                    node.AddIntegerParameter(3, "integerParam", _emberTree, true, 125, 0, 255);
                    //node.AddEnumParameter(4, "enumParameter", this, true, typeof(MockEnumParameter), 0, "");

                    string[] sourceNames = { "On" };
                    string blindSourceName = "Off";
                    string[] targetNames = { "T1", "T2" };
                    _gpioMatrix = _emberTree.ProviderRoot.AddMatrixOneToNBlindSource(RootIdentifiers.Matrix, sourceNames, targetNames, blindSourceName, _emberTree, true, "", "GPO");
                    
                    _emberTree.SetUpFinalListeners();
                    EmberTreeState = true;
                    log.Info("EmBER+ tree initiated");
                    done = true;
                }
                catch (Exception ex)
                {
                    EmberTreeState = false;
                    log.Error(ex, "Exception when initializing EmBER+ tree");
                    Thread.Sleep(4000);
                }
            }
        }

        public void TeardownEmberTree()
        {
            log.Info("Tearing down current EmBER+ tree");

            if (_emberTree != null)
            {
                _emberTree.ChangedTreeEvent -= EmberTreeOnTreeDataAsync;
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
        private void EmberTreeOnTreeDataAsync(string identifierPath, dynamic message, int[] path)
        {
            _websocketHub.Clients.All.ChangesInEmberTree(identifierPath, new ClientTreeParameterViewModel()
            {
                Type = message.GetType().ToString(),
                Value = message,
                NumericPath = string.Join(".", path)
            });
            Debug.WriteLine("Ember ", message);

            // TODO: Persist tree
            //    return EventHandlerHelper.ThrottledEventHandler((sender, e) =>
            //    {
            //        // TODO: Persist tree
            //        Debug.WriteLine("You should save the tree");
            //    }, 200);
        }

        /// <summary>
        /// EmBER+ tree matrix event changes
        /// </summary>
        private void ReceivedMatrixConnectionEvent(string identifierPath, GlowConnection connection, int[] path)
        {
            Debug.WriteLine($"Received Matrix Connection {identifierPath} , Operation: {(ConnectOperation)connection.Operation}, Target: {connection.Target}, First source: {connection.Sources.FirstOrDefault()}");

            // Send out the target state
            var signal = new ClientMatrixSignalViewModel()
            {
                Index = connection.Target,
                ConnectedSources = connection.Sources
            };

            _websocketHub.Clients.All.ChangesInEmberTreeMatrix(identifierPath, signal);
        }

        /// <summary>
        /// Returns a representation of the current parameter state of the tree
        /// </summary>
        private void EmberTreeInitialState()
        {
            // Send out the regular nodes/parameters from the tree
            Dictionary<string, ClientTreeParameterViewModel> tree = new Dictionary<string, ClientTreeParameterViewModel>();
            foreach (ParameterBase parameter in _emberTree.GetChildParameterElements())
            {
                log.Debug(parameter.IdentifierPath, parameter.GetValue().ToString());
                tree.Add(parameter.IdentifierPath, new ClientTreeParameterViewModel() {
                    Type = parameter.GetType().ToString(),
                    Value = parameter.GetValue(),
                    NumericPath = string.Join(".", parameter.Path)
                });
            }

            _websocketHub.Clients.All.InitialEmberTree(tree);

            // Send out the matrices in the tree
            Dictionary<string, ClientMatrixViewModel> matrices = new Dictionary<string, ClientMatrixViewModel>();
            foreach (Matrix treeMatrix in _emberTree.GetChildMatrixElements())
            {
                ClientMatrixViewModel matrix = new ClientMatrixViewModel()
                {
                    NumericPath = string.Join(".", treeMatrix.Path)
                };

                foreach (var target in treeMatrix.Targets)
                {
                    var item = new ClientMatrixSignalViewModel()
                    {
                        Index = target.Number,
                        Name = target.LabelParameter.Value,
                        ConnectedSources = target.ConnectedSources.Select((x) => x.Number).ToArray()
                    };
                    matrix.Targets.Add(item);
                }

                foreach (var source in treeMatrix.Sources)
                {
                    var item = new ClientMatrixSignalViewModel()
                    {
                        Index = source.Number,
                        Name = source.LabelParameter.Value
                    };
                    matrix.Sources.Add(item);
                }

                matrices.Add(treeMatrix.IdentifierPath, matrix);
            }

            _websocketHub.Clients.All.InitialEmberTreeMatrix(matrices);
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

        public void RequestInitialState()
        {
            EmberTreeInitialState();
        }

        public void Set_StringParameter(string path, string value)
        {
            if (_emberTree != null)
            {
                string[] str_arr = path.Split(".").ToArray();
                int[] int_arr = Array.ConvertAll(str_arr, Int32.Parse);
                var item = _emberTree.GetElement<StringParameter>(int_arr);
                item.SetValue(value);
            }
        }
        
        public void Set_NumberParameter(string path, int value)
        {
            if (_emberTree != null)
            {
                string[] str_arr = path.Split(".").ToArray();
                int[] int_arr = Array.ConvertAll(str_arr, Int32.Parse);
                var item = _emberTree.GetElement<IntegerParameter>(int_arr);
                item.SetValue(value);
            }
        }

        public void Set_BooleanParameter(string path, bool value)
        {
            if (_emberTree != null)
            {
                string[] str_arr = path.Split(".").ToArray();
                int[] int_arr = Array.ConvertAll(str_arr, Int32.Parse);
                var item = _emberTree.GetElement<BooleanParameter>(int_arr);
                item.SetValue(value);
            }
        }

        //public void Engine_SetGpio()
        //{
        //    Debug.WriteLine("Setting GPIO...");
        //}

    }

    public class ClientTreeParameterViewModel
    {
        public string Type { get; set; }
        public dynamic Value { get; set; }
        public string NumericPath { get; set; }
    }

    public class ClientMatrixViewModel
    {
        public string NumericPath { get; set; }
        public List<ClientMatrixSignalViewModel> Targets { get; set; } = new List<ClientMatrixSignalViewModel>();
        public List<ClientMatrixSignalViewModel> Sources { get; set; } = new List<ClientMatrixSignalViewModel>();
    }

    public class ClientMatrixSignalViewModel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int[] ConnectedSources { get; set; }
    }
}
