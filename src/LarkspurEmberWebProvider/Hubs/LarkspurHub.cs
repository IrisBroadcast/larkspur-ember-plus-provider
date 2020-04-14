using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

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
            await Clients.All.ChangesInEmberTree("Connected...");
            //return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Task.CompletedTask;
        }

        public async Task SetGpio()
        {
            EmberEngine.Engine_SetGpio();
            await Clients.All.ChangesInEmberTree("ReceiveMessage");
        }
    }

    /// <summary>
    /// All Websocket SignalR event's here
    /// </summary>
    public interface ILarkspurHub
    {
        Task ChangesInEmberTree(string message);
    }
}
