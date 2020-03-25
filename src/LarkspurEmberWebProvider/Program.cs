using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EmberLib.Glow;
using EmberPlusProviderClassLib;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using System.Diagnostics;

namespace LarkspurEmberWebProvider
{

    class RampData
    {
        public RealParameter MotorDBValueParameter;
        public double TargetLevel;
        public double LevelStep;
    }
    class Program
    {
        private static LarkspurEmberEngine _emberEngine;

        protected enum TemplateType
        {
            Core,
            Io
        }

        private static int Port = 9003;
        private static int MaxPackageLength = EmberVersion.ProtocolMaximumPackageLength;
        private static string SourceGroup = "sXX-sto";
        private static string Name = "sXX-core-sto";
        private static TemplateType templateType = TemplateType.Core;

        private const double DELTA_TIME = 0.1; // In seconds

        private static List<RampData> ramps = new List<RampData>();
        private static Dictionary<string, RealParameter> motorDBValueParameter = new Dictionary<string, RealParameter>();

        private static Dispatcher dispatcher;
        private static Dictionary<string, Action<ParameterBase>> parameterChangedAction = new Dictionary<string, Action<ParameterBase>>();
        //private static Timer timer;

        #region Startup
        static void Main(string[] args)
        {
            //int port;
            //int maxPackageLength;

            try
            {
                _emberEngine = new LarkspurEmberEngine();

                Console.WriteLine("Ruby Ember+ Dummy v{0}.{1} (GlowDTD v{2} - EmBER v{3})",
                    typeof(Program).Assembly.GetName().Version.Major,
                    typeof(Program).Assembly.GetName().Version.Minor,
                    EmberVersion.GlowDtdVersion,
                    EmberVersion.EmberEncodingVersion);
                Console.WriteLine("\nPress Enter to quit...");
                Console.ReadLine();

                /*
                dispatcher = new Dispatcher();
                dispatcher.Root = CreateTree(dispatcher);
                dispatcher.GlowRootReady += OnTreeChanged;
                // TODO: Maybe see if we can have an error event here..

                //timer = new Timer(OnTimer, null, 0, (int)(DELTA_TIME * 1000));

                using (var listener = new GlowListener(Port, MaxPackageLength, dispatcher))
                {
                    Console.WriteLine();
                    Console.WriteLine("Name:         {0}", Name);
                    Console.WriteLine("Source group: {0}", SourceGroup);
                    Console.WriteLine("Type:         {0}", templateType);
                    Console.WriteLine("Port:         {0}", Port);
                    Console.WriteLine("\nPress Enter to quit...");
                    Console.ReadLine();
                }*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in Program: {ex.Message}");
            }
        }
        #endregion Startup

        //private static void OnTimer(object state)
        //{
        //    #region RampMotorFader
        //    for (int i = ramps.Count - 1; i >= 0; i--)
        //    {
        //        RampData data = ramps[i];
        //        data.MotorDBValueParameter.Value += data.LevelStep;
        //        if (data.LevelStep > 0 && data.MotorDBValueParameter.Value > data.TargetLevel
        //            || data.LevelStep < 0 && data.MotorDBValueParameter.Value < data.TargetLevel)
        //        {
        //            data.MotorDBValueParameter.Value = data.TargetLevel;
        //            ramps.RemoveAt(i);
        //        }
        //    }
        //    #endregion RampMotorFader
        //}

        #region CreateTree

        static Node CreateTree(Dispatcher dispatcher)
        {
            var root = Node.CreateRoot();
            var router = new Node(1, root, "Ruby");
            CreateSystem(router, 29, dispatcher);
            CreateIdentity(router, 30, dispatcher);
            CreateConfiguration(router, 31, dispatcher);
            return root;
        }


        static void CreateGPIOs(Node parent, int nodeNumber, Dispatcher dispatcher)
        {
            var gpios = new Node(nodeNumber, parent, "GPIOs");

            CreateExternalControlGeneral(gpios, 1, dispatcher);
        }

        private static void CreateExternalControlGeneral(Node parent, int nodeNumber, Dispatcher dispatcher)
        {
            var root = new Node(nodeNumber, parent, "E_General");
            var outputRegister = new IntegerParameter(1, root, "Output Register", dispatcher, 0, 0, false)
            {
                Value = 0
            };
            var outputSignals = new Node(2, root, "Output Signals");
            var isRemoted_Out = CreateGPIOStateNode(outputSignals, 1, "IsRemoted", dispatcher, false);

            var inputRegister = new IntegerParameter(3, root, "Input Register", dispatcher, 0, 0, false)
            {
                Value = 0
            };
            var inputSignals = new Node(4, root, "Input Signals");
            var isRemoted_In = CreateGPIOStateNode(inputSignals, 1, "IsRemoted", dispatcher, true);

            var outputLevelsString = new StringParameter(5, root, "Output Levels String", dispatcher, false)
            {
                Value = "380380380380380380380380"
            };
            var outputLevels = new Node(6, root, "OutputLevels");
            var inputLevelsString = new StringParameter(7, root, "Input Levels String", dispatcher, false)
            {
                Value = ""
            };
            var inputLevels = new Node(8, root, "Input Levels");

            parameterChangedAction.Add(isRemoted_In.IdentifierPath + "/State",
                parameter => { ReflectBooleanParameterOnGPO(parameter, isRemoted_Out); });
        }

  
        static Node CreateGPIOStateNode(Node parent, int nodeNumber, string identifier, Dispatcher dispatcher, bool isInput)
        {
            var node = new Node(nodeNumber, parent, identifier);
            var state = new BooleanParameter(1, node, "State", dispatcher, isInput);
            return node;
        }



        static void CreateSystem(Node router, int nodeNumber, Dispatcher dispatcher)
        {
            var node = new Node(nodeNumber, router, "System");
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

        static void CreateConfiguration(Node router, int nodeNumber, Dispatcher dispatcher)
        {
            var node = new Node(nodeNumber, router, "Configuration");
            //TODO: Add more parameters to complete the dummy. Not required for WebMixer
        }

        #endregion CreateTree

        private static void ReflectBooleanParameterOnGPO(ParameterBase parameter, Node gpoNode)
        {
            BooleanParameter input = (BooleanParameter)parameter;
            if (input != null)
            {
                BooleanParameter output = (BooleanParameter)gpoNode.Children.First();
                if (output != null)
                {
                    output.Value = input.Value;
                }
            }
        }

        private static void HandleTransmitterChange(BooleanParameter input, Node prepareOutNode, Node onairOutNode, Node offairOutNode)
        {
            if (input.Value == false)
            {
                return;
            }

            BooleanParameter prepareOut = (BooleanParameter)prepareOutNode.Children.First();
            BooleanParameter onairOut   = (BooleanParameter)onairOutNode.Children.First();
            BooleanParameter offairOut  = (BooleanParameter)offairOutNode.Children.First();
            string state = input.Parent.Identifier.Split(new char[]{'_',':'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            switch (state)
            {
                case "prepare":
                    if (prepareOut != null)
                    {
                        prepareOut.Value = true;
                    }
                    if (offairOut != null)
                    {
                        offairOut.Value = false;
                    }
                    break;
                case "onair":
                    if (onairOut != null)
                    {
                        onairOut.Value = true;
                    }
                    if (offairOut != null)
                    {
                        offairOut.Value = false;
                    }
                    break;
                case "offair":
                    if (prepareOut != null)
                    {
                        prepareOut.Value = false;
                    }
                    if (onairOut != null)
                    {
                        onairOut.Value = false;
                    }
                    if (offairOut != null)
                    {
                        offairOut.Value = true;
                    }
                    break;
                default:

                    break;
            }
        }

        private static GlowValue[] RampMotorFader(GlowValue[] args)
        {
            string sourceName = args[0].String;
            if (!motorDBValueParameter.ContainsKey(sourceName))
            {
                Console.WriteLine("Attempt to ramp a motor fader that we don't know about. (" + sourceName + ")");
                return new[] { new GlowValue(0) };
            }
            RampData data = new RampData();
            data.MotorDBValueParameter = motorDBValueParameter[sourceName];
            data.TargetLevel = args[1].Real;
            double time = args[2].Real;
            data.LevelStep = ((data.TargetLevel - data.MotorDBValueParameter.Value) / time) * DELTA_TIME;
            ramps.Add(data);

            return new[] { new GlowValue(0) };
        }

        #region OnTreeChanged
        static void OnTreeChanged(object sender, Dispatcher.GlowRootReadyArgs e)
        {
            Debug.WriteLine($"Info: GlowRootReady / OnTreeChanged");
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
