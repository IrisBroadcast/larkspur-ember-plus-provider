using System;
using System.Collections.Generic;
using System.Linq;
using EmberPlusProviderLib.EmberHelpers;
using EmberPlusProviderLib.Model.Parameters;
using NLog;

namespace EmberPlusProviderLib
{
    public class EmberTree : IDisposable
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EmberPlusProvider Provider;

        public EmberTree(int port, string emberTreeIdentifier, string emberTreeDescription)
        {
            Provider = new EmberPlusProvider(port, emberTreeIdentifier, emberTreeDescription);
        }

        protected T GetElement<T>(int[] path) where T : class
        {
            var element = Provider.dispatcher.Root.ResolveChild(path, out var dynamicPathHandler);
            return element as T;
        }

        protected T GetElement<T>(string identifierPath) where T : class
        {
            var element = Provider.dispatcher.Root.ResolveChild(identifierPath);
            return element as T;
        }

        public IList<ParameterBase> GetWritableParameters()
        {
            return Provider.ProviderRoot.GetWritableChildParameters().ToList();
        }

        public void Dispose()
        {
            if (Provider != null)
            {
                Provider.Dispose();
                Provider = null;
            }
        }

    }
}