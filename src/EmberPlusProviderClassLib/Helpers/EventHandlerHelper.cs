using System;
using System.Threading.Tasks;
using NLog;

namespace EmberPlusProviderClassLib.Helpers
{
    public class EventHandlerHelper
    {
        // Fördröjer triggning av event med x ms.
        public static EventHandler ThrottledEventHandler(EventHandler handler, int throttleTimeInMilliseconds)
        {
            bool throttling = false;
            return (s, e) =>
            {
                if (!throttling)
                {
                    throttling = true;
                    Task.Delay(TimeSpan.FromMilliseconds(throttleTimeInMilliseconds)).ContinueWith(_ =>
                    {
                        throttling = false;
                        handler(s, e);
                    });
                }
                else
                    return;
            };
        }
    }
}