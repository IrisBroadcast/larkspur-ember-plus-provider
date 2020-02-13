using System;
using System.Threading.Tasks;

namespace EmberPlusProviderClassLib.Helpers
{
    public class EventHandlerHelper
    {
        /// <summary>
        /// Delays an event trigger with x ms.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="throttleTimeInMilliseconds"></param>
        /// <returns></returns>
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