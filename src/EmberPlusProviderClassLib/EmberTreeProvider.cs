using System;
using System.Collections.Generic;
using System.Linq;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using EmberLib.Framing;
using NLog;

namespace EmberPlusProviderClassLib
{
    public class EmberTreeProvider : IDisposable
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //public EmberProvider Provider;

        public EmberTreeProvider Provider;
        public Node ProviderRoot { get; protected set; }
        public Dispatcher Dispatcher { get; protected set; }
        protected GlowListener listener;

        public EmberTreeProvider(int port = 9003,
            string emberTreeIdentifier = "Identifier",
            string emberTreeDescription = "Description",
            string emberTreePoductName = "Product Name",
            string emberTreeCompanyName = "Company Name",
            string emberTreeApplicationVersion = "1.0.0")
        {
            int maxPackageLength = ProtocolParameters.MaximumPackageLength;
            Dispatcher = new Dispatcher { Root = Node.CreateRoot() };
            ProviderRoot = new Node(1, Dispatcher.Root, emberTreeIdentifier) { Description = emberTreeDescription };
            listener = new GlowListener(port, maxPackageLength, Dispatcher);
            Provider.CreateIdentityNode(0, emberTreePoductName, emberTreeCompanyName, emberTreeApplicationVersion);
        }

        public void CreateIdentityNode(int number, string product, string company, string version)
        {
            var identity = new Node(number, ProviderRoot, "identity")
            {
                SchemaIdentifier = "de.l-s-b.emberplus.identity"
            };

            identity.AddStringParameter(1, "product", this, false, product);
            identity.AddStringParameter(2, "company", this, false, company);
            identity.AddStringParameter(3, "version", this, false, version);
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

        public EmberNode AddChildNode(ValueType identifier)
        {
            return ProviderRoot.AddSubNode(identifier, this);
        }

        public void Dispose()
        {
            if (Provider != null)
            {
                Provider.Dispose();
                Provider = null;
            }

            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }
        }
    }
}
