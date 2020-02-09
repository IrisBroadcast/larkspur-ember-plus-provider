using System;

namespace EmberPlusProviderClassLib.Model.Parameters
{
    public class BooleanParameter : Parameter<bool>
    {
        public BooleanParameter(int number, Element parent, string identifier, Dispatcher dispatcher, bool isWriteable)
        : base(number, parent, identifier, dispatcher, isWriteable)
        {
        }

        public override TResult Accept<TState, TResult>(IElementVisitor<TState, TResult> visitor, TState state)
        {
            return visitor.Visit(this, state);
        }

        public override void SetValue(object newValue)
        {
            try
            {
                bool b = Convert.ToBoolean(newValue);
                if (Value != b)
                {
                    Value = b;
                }
            }
            catch (Exception)
            {
                Log.Warn($"Failed to set parameter {Identifier} value to {newValue}");
            }
        }
    }
}