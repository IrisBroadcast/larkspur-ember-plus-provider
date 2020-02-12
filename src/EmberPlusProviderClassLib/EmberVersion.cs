using System;
using System.Collections.Generic;
using System.Text;
using EmberLib.Framing;
using EmberLib.Glow.Framing;

namespace EmberPlusProviderClassLib
{
    public static class EmberVersion
    {

        public static string GlowDtdVersion
        {
            get
            {
                return GlowReader.UshortVersionToString(EmberLib.Glow.GlowDtd.Version);
            }
        }
        public static string EmberEncodingVersion
        {
            get
            {
                return GlowReader.UshortVersionToString(EmberLib.EmberEncoding.Version);
            }
        }

        public static int ProtocolMaximumPackageLength
        {
            get
            {
                return ProtocolParameters.MaximumPackageLength;
            }
        }
    }
}
