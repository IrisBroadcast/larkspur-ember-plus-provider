using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LarkspurEmberWebProvider.Hubs
{
    /// <summary>
    /// Add all the HUB method's here
    /// </summary>
    public class LarkspurHub : Hub<ILarkspurHub>
    {
        private static LarkspurEmberEngine EmberEngine => LarkspurEmberEngine.SingleInstance;

        public async Task SetGpio()
        {
            EmberEngine.Engine_SetGpio();
            await Clients.All.ChangesInEmberTree("ReceiveMessage");
        }
    }

    /// <summary>
    /// Add all the HUB event's here
    /// </summary>
    public interface ILarkspurHub
    {
        Task ChangesInEmberTree(string message);
    }
}
