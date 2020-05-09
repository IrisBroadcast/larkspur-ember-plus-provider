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
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LarkspurEmberWebProvider.Models;

namespace LarkspurEmberWebProvider.Helpers
{
    public static class TemplateParserHelper
    {
        public static Dictionary<string, Dictionary<string, string>> ParseTemplateJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            using StreamReader r = new StreamReader(filePath);
            var json = r.ReadToEnd();

            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true // If JSON values contains empty string then ignore
            };

            var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json, options);

            var tree = new TemplateBase();

            foreach (KeyValuePair<string, Dictionary<string, string>> entry in dict)
            {
                // Base leafs
                Console.WriteLine($"> {entry.Key.ToString()} : {entry.Value.ToString()}");

                var baseNode = new TemplateBaseLeaf()
                {
                    Description = "",
                    Id = "",
                    Name = entry.Key,
                };
                
                foreach (KeyValuePair<string, string> inside in entry.Value)
                {
                    //var node = inside.Value.Split(",");
                    //baseNode.Nodes.Add(new TemplateNode()
                    //{
                    //    Description = "",
                    //    Id = "",
                    //    Name = inside.Key,
                    //    Value = node[0],
                    //    Writable = (node[1] == "true")
                    //});
                    Console.WriteLine($"--> {inside.Key.ToString()} : {inside.Value.ToString()}");
                }

                tree.BaseLeafs.Add(baseNode);
            }

            return dict;
        }
    }
}
