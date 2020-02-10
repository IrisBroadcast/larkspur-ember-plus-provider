using System;
using EmberLib.Framing;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Model;

namespace EmberPlusProviderClassLib
{
    /*public class EmberProvider : IDisposable
    {
        public Node ProviderRoot { get; protected set; }
        public Dispatcher Dispatcher { get; protected set; }

        protected GlowListener listener;

        public EmberProvider(int port, string identifier, string description)
        {
            int maxPackageLength = ProtocolParameters.MaximumPackageLength;
            Dispatcher = new Dispatcher { Root = Node.CreateRoot() };
            ProviderRoot = new Node(1, Dispatcher.Root, identifier) { Description = description };
            listener = new GlowListener(port, maxPackageLength, Dispatcher);
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

        public EmberNode AddChildNode(ValueType identifier)
        {
            return ProviderRoot.AddSubNode(identifier, this);
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }
        }
    }*/
}
