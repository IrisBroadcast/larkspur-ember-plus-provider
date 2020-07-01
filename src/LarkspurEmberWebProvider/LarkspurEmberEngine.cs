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
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.Helpers;
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
                    _emberTree.CreateIdentityNode(
                        RootIdentifiers.Identity,
                        _configuration.EmberTree.Product,
                        _configuration.EmberTree.Company,
                        _configuration.EmberTree.Version);

                    // Get saved values
                    //var template = TemplateParserHelper.ParseTemplateJsonFile(_configuration.EmberTree.TreeTemplateFile);
                    _emberTree.InitializeAllNodes(RootIdentifiers.Utilities);

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
            _websocketHub.Clients.All.ChangesInEmberTree(identifierPath, new
            {
                Value = message,
                NumericPath = string.Join(".", path)
            });
            Debug.WriteLine("", message);

            // TODO: Persist tree
            //    return EventHandlerHelper.ThrottledEventHandler((sender, e) =>
            //    {
            //        // TODO: Persist tree
            //        Debug.WriteLine("You should save the tree");
            //    }, 200);
        }

        /// <summary>
        /// Returns a representation of the current parameter state of the tree
        /// </summary>
        private void EmberTreeInitialState()
        {
            Dictionary<string, dynamic> obj = new Dictionary<string, dynamic>();
            foreach (ParameterBase parameter in _emberTree.GetChildParameterElements())
            {
                log.Debug(parameter.IdentifierPath, parameter.GetValue().ToString());
                obj.Add(parameter.IdentifierPath, new
                {
                    Value = parameter.GetValue(),
                    NumericPath = string.Join(".", parameter.Path)
                });
            }

            _websocketHub.Clients.All.RawEmberTree(obj);
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
}
