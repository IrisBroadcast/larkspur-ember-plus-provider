#region copyright
/*
 * Larkspur Ember Plus Provider
 *
 * Copyright (c) 2020 Roger Sandholm & Fredrik Bergholtz, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

using System;
using System.Linq;
using System.Threading.Tasks;
using EmberLib.Glow;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using System.Diagnostics;
using System.Collections.Generic;

namespace EmberPlusProviderClassLib.EmberHelpers
{
    public static class EmberNodeExtensions
    {
        public static EmberNode AddSubNode(this Node node, ValueType identifier, EmberPlusProvider provider)
        {
            return AddSubNode(node, (int)identifier, identifier.ToString().Replace("_", " "), provider);
        }

        public static EmberNode AddSubNode(this Node node, int index, string identifier, EmberPlusProvider provider)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new EmberNode(index, node, identifier, provider);
        }

        public static StringParameter AddStringParameter(this Node node, ValueType identifier, EmberPlusProvider provider, bool isWritable, string value = "", string description = "")
        {
            return AddStringParameter(node, (int)identifier, identifier.ToString().Replace("_", " "), provider, isWritable, value, description);
        }

        public static StringParameter AddStringParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, string value = "", string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new StringParameter(index, node, identifier, provider.dispatcher, isWritable) { Value = value, Description = description };
        }

        public static BooleanParameter AddBooleanParameter(this Node node, ValueType identifier, EmberPlusProvider provider, bool isWritable, bool value = false, string description = "")
        {
            return AddBooleanParameter(node, (int)identifier, identifier.ToString().Replace("_", " "), provider, isWritable, value, description);
        }

        public static BooleanParameter AddBooleanParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, bool value = false, string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new BooleanParameter(index, node, identifier, provider.dispatcher, isWritable) { Value = value, Description = description };
        }

        public static IntegerParameter AddIntegerParameter(this Node node, ValueType identifier, EmberPlusProvider provider, bool isWritable, int value = 0, int min = 0, int max = 255, string description = "")
        {
            return AddIntegerParameter(node, (int) identifier, identifier.ToString().Replace("_", " "), provider, isWritable, value, min, max, description);
        }

        public static IntegerParameter AddIntegerParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, int value = 0, int min = 0, int max = 255, string description = "")
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new IntegerParameter(index, node, identifier, provider.dispatcher, min, max, isWritable) { Value = value, Description = description };
        }

        //public static void AddEnumParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable = false, Type enumType = null, int value = 0, string description = "")
        //{
        //    NodeAsserter.AssertIdentifierValid(identifier);
        //    if(enumType != null)
        //    {
        //        new EnumParameter(index, node, identifier, provider.dispatcher, 0, Enum.GetValues(enumType).Length, isWritable) { Value = value, Description = description };
        //    }
        //}

        public static void AddFunction(this Node node, ValueType identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            node.AddFunction((int)identifier, identifier.ToString().Replace("_", " "), arguments, result, coreFunc);
        }

        public static void AddFunction(this Node node, int index, string identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new Function(index, node, identifier, arguments, result, coreFunc);
        }

        public static OneToNMatrix AddMatrixOneToN(this Node node, ValueType identifier, string[] sourceNames, string[] targetNames, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {
            return AddMatrixOneToN(node, (int)identifier, identifier.ToString().Replace("_", " "), sourceNames, targetNames, provider, isWritable, description, matrixIdentifier);
        }

        public static OneToNMatrix AddMatrixOneToN(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {
            
            var oneToN = new Node(index, node, identifier )
            {
                Description = description,
            };

            var labels = new Node(1, oneToN, "labels")
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.labels"
            };

            var targetLabels = new Node(1, labels, "targets");
            var sourceLabels = new Node(2, labels, "sources");

            var targets = new List<Signal>();
            var sources = new List<Signal>();

            for (int number = 0; number < sourceNames.Length; number++)
            {
                var sourceParameter = new StringParameter(number, sourceLabels, $"s-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = sourceNames[number]
                };

                sources.Add(new Signal(number, sourceParameter));
            }
            for (int number = 0; number < targetNames.Length; number++)
            {
                var targetParameter = new StringParameter(number, targetLabels, $"t-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = targetNames[number]
                };

                targets.Add(new Signal(number, targetParameter));
            }
            var matrix = new OneToNMatrix(
               2,
               oneToN,
               matrixIdentifier,
               provider.dispatcher,
               targets,
               sources,
               labels,
               isWritable)
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.oneToN"
            };

            //foreach (var target in matrix.Targets)
            //    matrix.Connect(target, new[] { matrix.GetSource(target.Number) }, null);
            return matrix;
        }


        public static OneToNBlindSourceMatrix AddMatrixOneToNBlindSource(this Node node, ValueType identifier, string[] sourceNames, string[] targetNames, string blindSourceName, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {
            return AddMatrixOneToNBlindSource(node, (int)identifier, identifier.ToString().Replace("_", " "), sourceNames, targetNames, blindSourceName, provider, isWritable, description, matrixIdentifier);
        }

        public static OneToNBlindSourceMatrix AddMatrixOneToNBlindSource(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, string blindSourceName, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {

            var oneToN = new Node(index, node, identifier)
            {
                Description = description,
            };

            var labels = new Node(1, oneToN, "labels")
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.labels"
            };

            var targetLabels = new Node(1, labels, "targets");
            var sourceLabels = new Node(2, labels, "sources");

            var targets = new List<Signal>();
            var sources = new List<Signal>();

            // Add the blind source
            var blindIndex = 0;
            var blindParameter = new StringParameter(blindIndex, sourceLabels, $"b-{blindIndex}", provider.dispatcher, isWritable: true)
            {
                Value = blindSourceName
            };
            var blindSignal = new Signal(blindIndex, blindParameter);
            sources.Add(blindSignal);

            // Add sources
            var numberOfBlinds = sources.Count();
            for (int number = 0; number < sourceNames.Length; number++)
            {
                var sourceParameter = new StringParameter(number + numberOfBlinds, sourceLabels, $"s-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = sourceNames[number]
                };

                sources.Add(new Signal(number + numberOfBlinds, sourceParameter));
            }

            // Add targets
            for (int number = 0; number < targetNames.Length; number++)
            {
                var targetParameter = new StringParameter(number, targetLabels, $"t-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = targetNames[number]
                };

                targets.Add(new Signal(number, targetParameter));
            }

            var matrix = new OneToNBlindSourceMatrix(
               2,
               oneToN,
               matrixIdentifier,
               provider.dispatcher,
               targets,
               sources,
               blindSignal,
               labels,
               isWritable)
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.oneToN"
            };

            //foreach (var target in matrix.Targets)
            //    matrix.Connect(target, new[] { matrix.BlindSource }, null);

            return matrix;
        }

        public static T GetParameter<T>(this Node node, int index) where T : ParameterBase
        {
            IDynamicPathHandler dph;
            return node.ResolveChild(new int[] { index }, out dph) as T;
        }

        /// <summary>
        /// Updates a string value node
        /// </summary>
        public static bool UpdateParameter(this Node node, ValueType identifier, string newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, string newValue)
        {
            var p = node.GetParameter<StringParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates a boolean value node
        /// </summary>
        public static bool UpdateParameter(this Node node, ValueType identifier, bool newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, bool newValue)
        {
            var p = node.GetParameter<BooleanParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates a long value node
        /// </summary>
        public static bool UpdateParameter(this Node node, ValueType identifier, long newValue)
        {
            return node.UpdateParameter((int)identifier, newValue);
        }

        public static bool UpdateParameter(this Node node, int index, long newValue)
        {
            var p = node.GetParameter<IntegerParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
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