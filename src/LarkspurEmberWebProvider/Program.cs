using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmberLib.Glow;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LarkspurEmberWebProvider
{
    public class Program
    {
        //private static LarkspurEmberEngine _emberEngine;


        private static int Port = 9003;
        private static int MaxPackageLength = EmberVersion.ProtocolMaximumPackageLength;
        private static string SourceGroup = "sXX-sto";
        private static string Name = "sXX-core-sto";


        private const double DELTA_TIME = 0.1; // In seconds

        private static Dispatcher dispatcher;
        private static Dictionary<string, Action<ParameterBase>> parameterChangedAction = new Dictionary<string, Action<ParameterBase>>();


        public static void Main(string[] args)
        {
            Console.WriteLine("Ruby Ember+ Dummy v{0}.{1} (GlowDTD v{2} - EmBER v{3})",
                typeof(Program).Assembly.GetName().Version.Major,
                typeof(Program).Assembly.GetName().Version.Minor,
                EmberVersion.GlowDtdVersion,
                EmberVersion.EmberEncodingVersion);

            dispatcher = new Dispatcher();
            dispatcher.Root = CreateTree(dispatcher);
            dispatcher.GlowRootReady += OnTreeChanged;

            //timer = new Timer(OnTimer, null, 0, (int)(DELTA_TIME * 1000));

            using (var listener = new GlowListener(Port, MaxPackageLength, dispatcher))
            {
                Console.WriteLine();
                Console.WriteLine("Name:         {0}", Name);
                Console.WriteLine("Source group: {0}", SourceGroup);
                Console.WriteLine("Port:         {0}", Port);
                Console.WriteLine("\nPress Enter to quit...");
                Console.ReadLine();
            }



            //_emberEngine = new LarkspurEmberEngine();



            //CreateHostBuilder(args).Build().Run();

            Console.ReadLine();
            Console.WriteLine("NG Ember Provider - Exiting...");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHostedService<Worker>();
                })
                .UseWindowsService(); // INFO: Makes it possible to run as a Windows Service



        static Node CreateTree(Dispatcher dispatcher)
        {
            var root = Node.CreateRoot();
            var router = new Node(1, root, "Ruby");
            CreateIdentity(router, 1, dispatcher);
            CreateConfiguration(router, 2, dispatcher);
            return root;
        }


        static void CreateConfiguration(Node router, int nodeNumber, Dispatcher dispatcher)
        {
            var node = new Node(nodeNumber, router, "Configuration");
            //TODO: Add more parameters to complete the dummy. Not required for WebMixer
        }

        static void CreateIdentity(Node router, int nodeNumber, Dispatcher dispatcher)
        {
            var identity = new Node(nodeNumber, router, "identity");
            var product = new StringParameter(1, identity, "product", dispatcher, false) { Value = "PowerCore" };
            var company = new StringParameter(2, identity, "company", dispatcher, false) { Value = "Lawo AG / DSA-Volgmann" };
            var serial = new StringParameter(3, identity, "serial", dispatcher, false) { Value = "00-54-01-82-29-29-F9-13" };
            var version = new StringParameter(4, identity, "version", dispatcher, false) { Value = "6.0.4880" };
            var role = new StringParameter(5, identity, "role", dispatcher, false) { Value = Name };
        }

        #region OnTreeChanged
        static void OnTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            GlowContainer root = e.Root;

            var p = root.FirstOrDefault() as GlowQualifiedParameter;
            if (p == null)
            {
                return;
            }

            IDynamicPathHandler dynamicPathHandler;
            var parameter = dispatcher.Root.ResolveChild(p.Path, out dynamicPathHandler) as ParameterBase;
            if (parameter == null)
            {
                return;
            }

            if (parameterChangedAction.ContainsKey(parameter.IdentifierPath))
            {
                parameterChangedAction[parameter.IdentifierPath](parameter);
            }
        }
        #endregion OnTreeChanged
    }
}
