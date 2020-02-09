using System;
using EmberPlusProviderLib.Model;

namespace EmberPlusProviderLib.EmberHelpers
{
    public class EmberNode : Node
    {
        protected readonly EmberPlusProvider Provider;

        public EmberNode(int number, Element parent, string identifier, EmberPlusProvider provider) : base(number, parent, identifier)
        {
            Provider = provider;
        }

        public EmberNode AddSubNode(ValueType emberTreeIdentifier)
        {
            return AddSubNode((int)emberTreeIdentifier, emberTreeIdentifier.ToString());
        }

        public EmberNode AddSubNode(int index, string identifier)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new EmberNode(index, this, identifier, Provider);
        }

        public void AddEnumParameter(ValueType emberTreeIdentifiers, Type enumType, bool isWriteable = false, string description = "")
        {
            this.AddIntegerParameter((int)emberTreeIdentifiers, emberTreeIdentifiers.ToString(), Provider, 0, Enum.GetValues(enumType).Length, isWriteable, 0, description);
        }

        public void AddStringParameter(ValueType identifier, string value = "", bool isWriteable = false, string description = "")
        {
            this.AddStringParameter((int)identifier, identifier.ToString(), Provider, isWriteable, value, description);
        }

        public void AddBooleanParameter(ValueType identifier, bool isWriteable = false, bool value = false, string description = "")
        {
            this.AddBooleanParameter((int)identifier, identifier.ToString(), Provider, isWriteable, value, description);
        }

        public void AddIntegerParameter(ValueType identifier, bool isWriteable = false, int min = 0, int max = 255, int value = 0, string description = "")
        {
            this.AddIntegerParameter((int)identifier, identifier.ToString(), Provider, min, max, isWriteable, value, description);
        }

        public override string ToString()
        {
            return IdentifierPath;
        }
    }
}