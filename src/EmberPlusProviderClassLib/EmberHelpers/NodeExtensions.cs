using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using EmberLib.Glow;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;

namespace EmberPlusProviderClassLib.EmberHelpers
{
    public static class NodeExtensions
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static EmberNode AddSubNode(this Node node, ValueType identifier, EmberPlusProvider provider)
        {
            return AddSubNode(node, (int)identifier, identifier.ToString(), provider);
        }

        public static EmberNode AddSubNode(this Node node, int index, string identifier, EmberPlusProvider provider)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new EmberNode(index, node, identifier, provider);
        }

        public static void AddStringParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWriteable, string value = "", string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new StringParameter(index, node, identifier, provider.dispatcher, isWriteable) { Value = value, Description = description};
        }

        public static void AddIntegerParameter(this Node node, int index, string identifier, EmberPlusProvider provider, int min, int max, bool isWriteable, int value = 0, string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new IntegerParameter(index, node, identifier, provider.dispatcher, min, max, isWriteable) { Value = value, Description = description };
        }

        public static void AddBooleanParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWriteable, bool value = false, string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new BooleanParameter(index, node, identifier, provider.dispatcher, isWriteable) { Value = value, Description = description };
        }

        public static void AddFunction(this Node node, ValueType identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            node.AddFunction((int)identifier, identifier.ToString(), arguments, result, coreFunc);
        }

        public static void AddFunction(this Node node, int index, string identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new Function(index, node, identifier, arguments, result, coreFunc);
        }

        public static T GetParameter<T>(this Node node, int index) where T : ParameterBase
        {
            IDynamicPathHandler dph;
            return node.ResolveChild(new int[] { index }, out dph) as T;
        }

        public static bool UpdateParameter(this Node node, ValueType identifier, string newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, string newValue)
        {
            var p = node.GetParameter<StringParameter>(index);
            if (p != null && p.Value != newValue)
            {
                log.Info("Setting node {0} to \"{1}\"", p.IdentifierPath, newValue);
                p.Value = newValue;
                return true;
            }
            return false;
        }

        public static bool UpdateParameter(this Node node, ValueType identifier, bool newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, bool newValue)
        {
            var p = node.GetParameter<BooleanParameter>(index);
            if (p != null && p.Value != newValue)
            {
                log.Info("Setting node {0} to {1}", p.IdentifierPath, newValue);
                p.Value = newValue;
                return true;
            }
            return false;
        }

        public static bool UpdateParameter(this Node node, ValueType identifier, long newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, long newValue)
        {
            var p = node.GetParameter<IntegerParameter>(index);
            if (p != null && p.Value != newValue)
            {
                log.Info("Setting node {0} to {1}", p.IdentifierPath, newValue);
                p.Value = newValue;
                return true;
            }
            return false;
        }

        public static bool HasStringParameterWithValue(this Node node, ValueType identifier, string value)
        {
            value = (value ?? string.Empty).Trim().ToLower();
            var parameterValue = node?.GetStringParameterValue(identifier, string.Empty);
            return parameterValue?.ToLower() == value;
        }

        public static bool HasIntegerParameterWithValue(this Node node, ValueType identifier, long value)
        {
            long? parameterValue = node?.GetIntegerParameterValue(identifier);
            return parameterValue.HasValue && parameterValue.Value == value;
        }

        public static string GetStringParameterValue(this Node node, ValueType identifier, string defaultValue = "")
        {
            return node.GetStringParameterValue(identifier.ToString(), defaultValue);
        }

        public static string GetStringParameterValue(this Node node, string name, string defaultValue = "")
        {
            var parameter = node.Children.FirstOrDefault(child => child.Identifier == name) as StringParameter;
            return parameter == null ? defaultValue : parameter.Value;
        }

        public static bool GetBoooleanParameterValue(this Node node, ValueType identifier)
        {
            return node.GetBoooleanParameterValue(identifier.ToString());
        }

        public static bool GetBoooleanParameterValue(this Node node, string name)
        {
            var parameter = node.Children.FirstOrDefault(child => child.Identifier == name) as BooleanParameter;
            return parameter?.Value ?? false;
        }

        public static long GetIntegerParameterValue(this Node node, ValueType identifier)
        {
            return node.GetIntegerParameterValue(identifier.ToString());
        }

        public static long GetIntegerParameterValue(this Node node, string name)
        {
            var parameter = node.Children.FirstOrDefault(child => child.Identifier == name) as IntegerParameter;
            return parameter?.Value ?? 0;
        }

    }
}