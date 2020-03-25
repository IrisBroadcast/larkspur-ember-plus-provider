using System;
using System.Diagnostics;
using EmberLib.Framing;
using EmberPlusProviderClassLib.EmberHelpers;
using EmberPlusProviderClassLib.Model;

namespace EmberPlusProviderClassLib
{
    public class EmberPlusProvider : IDisposable
    {
        public Node ProviderRoot { get; protected set; }
        public Dispatcher dispatcher { get; protected set; }

        protected GlowListener listener;

        /// <summary>
        /// Creates the actual EmBER+ provider tree
        /// </summary>
        /// <param name="port">EmBER+ provider port</param>
        /// <param name="identifier">EmBER+ root identifier</param>
        /// <param name="description">EmBER+ root description</param>
        public EmberPlusProvider(int port, string identifier, string description)
        {
            try
            {
                int maxPackageLength = ProtocolParameters.MaximumPackageLength;
                dispatcher = new Dispatcher {Root = Node.CreateRoot()};
                ProviderRoot = new Node(1, dispatcher.Root, identifier) { Description = description };
                listener = new GlowListener(port, maxPackageLength, dispatcher);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: EmberPlusProviderCLassLib / EmberPlusProvider: ", ex.Message);
            }

        }

        //public void CreateIdentityNode(int number, string product, string company, string version)
        //{
        //    var identity = new Node(number, ProviderRoot, "identity")
        //    {
        //        SchemaIdentifier = "de.l-s-b.emberplus.identity"
        //    };

        //    identity.AddStringParameter(1, "product", this, false, product);
        //    identity.AddStringParameter(2, "company", this, false, company);
        //    identity.AddStringParameter(3, "version", this, false, version);
        //}

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
    }
}
