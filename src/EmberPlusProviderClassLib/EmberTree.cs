using System;
using System.Collections.Generic;
using System.Linq;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Model.Parameters;
using NLog;

namespace EmberPlusProviderClassLib
{
    /*public class EmberTree : IDisposable
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EmberProvider Provider;

        public EmberTree(int port = 9001,
            string emberTreeIdentifier = "Identifier",
            string emberTreeDescription = "Description",
            string emberTreePoductName = "Product Name",
            string emberTreeCompanyName = "Company Name",
            string emberTreeApplicationVersion = "1.0.0")
        {
            Provider = new EmberProvider(port, emberTreeIdentifier, emberTreeDescription);
            Provider.CreateIdentityNode(0, "NGEmberProvider", "Sveriges Radio", ApplicationSettings.Version);
        }

        protected T GetElement<T>(int[] path) where T : class
        {
            var element = Provider.Dispatcher.Root.ResolveChild(path, out var dynamicPathHandler);
            return element as T;
        }

        protected T GetElement<T>(string identifierPath) where T : class
        {
            var element = Provider.Dispatcher.Root.ResolveChild(identifierPath);
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

    }*/
}