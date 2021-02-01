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
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace LarkspurEmberWebProvider.Hubs
{
    /// <summary>
    /// All the Websocket SignalR method's here
    /// </summary>
    public class LarkspurHub : Hub<ILarkspurHub>
    {
        private static LarkspurEmberEngine EmberEngine => LarkspurEmberEngine.SingleInstance;

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("user connected");
            await Clients.All.SystemStatus("Connected...");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }

        public void RequestInitialState()
        {
            EmberEngine.RequestInitialState();
        }

        public void ChangeEmberStringParameter(string path, string value)
        {
            EmberEngine.Set_StringParameter(path, value);
        }

        public void ChangeEmberNumberParameter(string path, int value)
        {
            EmberEngine.Set_NumberParameter(path, value);
        }

        public void ChangeEmberBooleanParameter(string path, bool value)
        {
            EmberEngine.Set_BooleanParameter(path, value);
        }

        public void PulseEmberBooleanParameter(string path)
        {
            EmberEngine.Set_PulseBooleanParameter(path);
        }
    }

    /// <summary>
    /// All Websocket SignalR event's here
    /// </summary>
    public interface ILarkspurHub
    {
        Task SystemStatus(string message);
        Task InitialEmberTree(Dictionary<string, ClientTreeParameterViewModel> obj);
        Task ChangesInEmberTree(string path, ClientTreeParameterViewModel obj);
        Task InitialEmberTreeMatrix(Dictionary<string, ClientMatrixViewModel> obj);
        Task ChangesInEmberTreeMatrix(string path, ClientMatrixSignalViewModel obj);
    }
}
