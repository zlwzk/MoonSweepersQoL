using System;
using System.Collections.Generic;
using System.Text;

namespace MoonSweepersMod.IO
{
    /// <summary>
    /// Minimal Valve Data Format (VDF) parser used to read the game's Steam Input
    /// action manifest and per-controller binding files. Supports nested braces
    /// whether the '{' is on the key line or the following line.
    /// </summary>
    public class VdfNode
    {
        public Dictionary<string, VdfNode> Children = new Dictionary<string, VdfNode>();
        public string Value;

        public bool IsValue => Value != null;
        public bool Has(string k) => Children.ContainsKey(k);
        public VdfNode this[string k] => Children.TryGetValue(k, out var n) ? n : null;
    }

    public static class VdfParser
    {
        public static VdfNode Parse(string text)
        {
            var root = new VdfNode();
            var stack = new Stack<VdfNode>();
            stack.Push(root);
            string pendingKey = null;

            var lines = text.Replace("\r\n", "\n").Split('\n');
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;

                if (line == "{")
                {
                    if (pendingKey != null && stack.Count > 0)
                    {
                        var child = new VdfNode();
                        stack.Peek().Children[pendingKey] = child;
                        stack.Push(child);
                        pendingKey = null;
                    }
                    continue;
                }
                if (line == "}")
                {
                    if (stack.Count > 1) stack.Pop();
                    continue;
                }

                // Read key
                if (line[0] != '"') continue;
                int i = 1;
                var sb = new StringBuilder();
                while (i < line.Length)
                {
                    char c = line[i++];
                    if (c == '\\' && i < line.Length) { sb.Append(line[i++]); continue; }
                    if (c == '"') break;
                    sb.Append(c);
                }
                var key = sb.ToString();

                while (i < line.Length && line[i] == ' ') i++;
                if (i < line.Length && line[i] == '{')
                {
                    var child = new VdfNode();
                    stack.Peek().Children[key] = child;
                    stack.Push(child);
                }
                else if (i < line.Length && line[i] == '"')
                {
                    var val = ReadQuoted(line, ref i);
                    stack.Peek().Children[key] = new VdfNode { Value = val };
                }
                else
                {
                    pendingKey = key; // opened on a following line
                }
            }
            return root;
        }

        static string ReadQuoted(string s, ref int i)
        {
            // s[i] == '"'
            i++;
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                char c = s[i++];
                if (c == '\\' && i < s.Length) { sb.Append(s[i++]); continue; }
                if (c == '"') break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static VdfNode Get(VdfNode n, params string[] path)
        {
            foreach (var p in path)
            {
                if (n == null) return null;
                n = n[p];
            }
            return n;
        }
    }
}
