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
using System.IO;

namespace LarkspurEmberWebProvider.Models
{
    public class ApplicationSettings
    {
        public string Name { get; set; }
        public string LogFolder { get; set; }
        public string ReleaseDate { get; set; } = new DateTime().ToString();
        public string Version { get; set; }
        public string Environment { get; set; }
        public string Server { get; set; }

        public ApplicationSettingsEmberTree EmberTree { get; set; } = new ApplicationSettingsEmberTree();
    }

    public class ApplicationSettingsEmberTree
    {
        public int Port { get; set; } = 9003;
        public string Identifier { get; set; } = "Larkspur";
        public string Description { get; set; } = "Larkspur";
        public string Product { get; set; } = "Larkspur EmBER+ Provider";
        public string Serial { get; set; } = "00-FF-00-FF-00-FF-00-FF";
        public string Role { get; set; } = "localhost-pc";
        public string Company { get; set; } = "IRIS Broadcast";
        public string Version { get; set; } = "0.0.1";
        public string TreeTemplateFile { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "treeTemplate.json");
    }
}
