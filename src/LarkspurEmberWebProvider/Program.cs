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
    class Program
    {
        private static LarkspurEmberEngine _emberEngine;

        #region Startup
        static void Main(string[] args)
        {
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in Program: {ex.Message}");
            }
        }
        #endregion Startup
    }
}
