using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace ModForge.Core
{
    /// <summary>
    /// The template language. Values: {{Name}} and {{Name|filter|filter}}.
    /// Blocks: {{#if expr}} {{#elif expr}} {{#else}} {{/if}}.
    /// Anything else in double braces is left exactly as written. An unknown name is an error, never an empty string.
    /// </summary>
    public static class Renderer
    {
        static readonly Regex TagRx = new Regex(@"\{\{[ \t]*([#/]?)([^{}\r\n]*?)[ \t]*\}\}", RegexOptions.Compiled);
        static readonly Regex VarRx = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*(\|[a-z]+)*$", RegexOptions.Compiled);
        static readonly Regex WordRx = new Regex(@"[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.Compiled);

        enum Kind { Var, Literal, If, Elif, Else, End }

        sealed class Tag
        {
            public Kind Kind;
            public string Body;
            public int Start;
            public int End;
        }

        sealed class Frame
        {
            public bool ParentActive;
            public bool Taken;
            public bool Active;
            public bool SawElse;
        }

        public static string Render(string text, IDictionary<string, string> vars, string where)
        {
            if (text.IndexOf("{{", StringComparison.Ordinal) < 0) return text;

            var tags = new List<Tag>();
            foreach (Match m in TagRx.Matches(text))
            {
                string prefix = m.Groups[1].Value;
                string body = m.Groups[2].Value.Trim();
                var tag = new Tag { Start = m.Index, End = m.Index + m.Length };
                if (prefix == "#")
                {
                    if (body.StartsWith("if ", StringComparison.Ordinal)) { tag.Kind = Kind.If; tag.Body = body.Substring(3); }
                    else if (body.StartsWith("elif ", StringComparison.Ordinal)) { tag.Kind = Kind.Elif; tag.Body = body.Substring(5); }
                    else if (body == "else") { tag.Kind = Kind.Else; }
                    else throw new ForgeException(where + ": unknown block tag '" + m.Value + "'");
                    Standalone(text, tag);
                }
                else if (prefix == "/")
                {
                    if (body != "if") throw new ForgeException(where + ": unknown block tag '" + m.Value + "'");
                    tag.Kind = Kind.End;
                    Standalone(text, tag);
                }
                else if (VarRx.IsMatch(body))
                {
                    tag.Kind = Kind.Var;
                    tag.Body = body;
                }
                else if (body.StartsWith("raw ", StringComparison.Ordinal))
                {
                    // {{raw word}} writes {{word}}, for files that need double braces of their own (SMAPI, Content Patcher).
                    tag.Kind = Kind.Literal;
                    tag.Body = "{{" + body.Substring(4).Trim() + "}}";
                }
                else continue; // not ours: stays in the output as written
                tags.Add(tag);
            }

            var sb = new StringBuilder(text.Length + 64);
            var stack = new Stack<Frame>();
            bool active = true;
            int pos = 0;
            foreach (var tag in tags)
            {
                if (tag.Start < pos) throw new ForgeException(where + ": two block tags share one line; put each on its own line");
                if (active) sb.Append(text, pos, tag.Start - pos);
                pos = tag.End;
                switch (tag.Kind)
                {
                    case Kind.Var:
                        if (active) sb.Append(Value(tag.Body, vars, where));
                        break;
                    case Kind.Literal:
                        if (active) sb.Append(tag.Body);
                        break;
                    case Kind.If:
                    {
                        var f = new Frame { ParentActive = active };
                        f.Active = active && Eval(tag.Body, vars, where);
                        f.Taken = f.Active;
                        stack.Push(f);
                        active = f.Active;
                        break;
                    }
                    case Kind.Elif:
                    {
                        if (stack.Count == 0) throw new ForgeException(where + ": {{#elif}} without {{#if}}");
                        var f = stack.Peek();
                        if (f.SawElse) throw new ForgeException(where + ": {{#elif}} after {{#else}}");
                        f.Active = f.ParentActive && !f.Taken && Eval(tag.Body, vars, where);
                        if (f.Active) f.Taken = true;
                        active = f.Active;
                        break;
                    }
                    case Kind.Else:
                    {
                        if (stack.Count == 0) throw new ForgeException(where + ": {{#else}} without {{#if}}");
                        var f = stack.Peek();
                        f.SawElse = true;
                        f.Active = f.ParentActive && !f.Taken;
                        f.Taken = true;
                        active = f.Active;
                        break;
                    }
                    case Kind.End:
                    {
                        if (stack.Count == 0) throw new ForgeException(where + ": {{/if}} without {{#if}}");
                        active = stack.Pop().ParentActive;
                        break;
                    }
                }
            }
            if (stack.Count != 0) throw new ForgeException(where + ": {{#if}} is never closed");
            sb.Append(text, pos, text.Length - pos);
            return sb.ToString();
        }

        /// <summary>A block tag alone on its line takes the whole line with it, so it leaves no blank line behind.</summary>
        static void Standalone(string text, Tag tag)
        {
            int s = tag.Start;
            while (s > 0 && (text[s - 1] == ' ' || text[s - 1] == '\t')) s--;
            if (s > 0 && text[s - 1] != '\n') return;
            int e = tag.End;
            while (e < text.Length && (text[e] == ' ' || text[e] == '\t' || text[e] == '\r')) e++;
            if (e < text.Length && text[e] != '\n') return;
            if (e < text.Length) e++;
            tag.Start = s;
            tag.End = e;
        }

        static string Value(string body, IDictionary<string, string> vars, string where)
        {
            var parts = body.Split('|');
            string v;
            if (!vars.TryGetValue(parts[0], out v))
                throw new ForgeException(where + ": unknown name {{" + parts[0] + "}}");
            v = v ?? "";
            for (int i = 1; i < parts.Length; i++) v = Filter(v, parts[i], where);
            return v;
        }

        public static IEnumerable<string> Words(string s)
        {
            return WordRx.Matches(s ?? "").Cast<Match>().Select(m => m.Value);
        }

        static string Cap(string w) { return w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w.Substring(1); }

        public static string Filter(string v, string name, string where)
        {
            switch (name)
            {
                case "pascal":
                {
                    var r = string.Concat(Words(v).Select(Cap));
                    return r.Length > 0 && char.IsDigit(r[0]) ? "_" + r : r;
                }
                case "camel":
                {
                    var w = Words(v).ToList();
                    if (w.Count == 0) return "";
                    var r = w[0].ToLowerInvariant() + string.Concat(w.Skip(1).Select(Cap));
                    return char.IsDigit(r[0]) ? "_" + r : r;
                }
                case "snake": return string.Join("_", Words(v).Select(w => w.ToLowerInvariant()));
                case "kebab": return string.Join("-", Words(v).Select(w => w.ToLowerInvariant()));
                case "flat": return string.Concat(Words(v).Select(w => w.ToLowerInvariant()));
                case "lower": return v.ToLowerInvariant();
                case "upper": return v.ToUpperInvariant();
                case "trim": return v.Trim();
                case "nospace": return Regex.Replace(v, @"\s+", "");
                case "ident":
                {
                    var r = Regex.Replace(v, @"[^A-Za-z0-9_]", "");
                    return r.Length > 0 && char.IsDigit(r[0]) ? "_" + r : r;
                }
                case "json": return Json.Escape(v);
                case "xml": return SecurityElement.Escape(v) ?? "";
                // Lua and C-family string escapes: backslash and both quote kinds.
                case "str": return v.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
                case "fwd": return v.Replace('\\', '/');
                case "back": return v.Replace('/', '\\');
                default: throw new ForgeException(where + ": unknown filter '|" + name + "'");
            }
        }

        // ---- expressions: name, !name, name == value, name != value, &&, ||, ( ) ----

        public static bool Truthy(string v)
        {
            return !string.IsNullOrEmpty(v) && !string.Equals(v, "false", StringComparison.OrdinalIgnoreCase);
        }

        public static bool Eval(string expr, IDictionary<string, string> vars, string where)
        {
            var toks = Lex(expr, where);
            int i = 0;
            bool r = Or(toks, ref i, vars, where, expr);
            if (i != toks.Count) throw new ForgeException(where + ": cannot read condition '" + expr + "'");
            return r;
        }

        static List<string> Lex(string s, string where)
        {
            var toks = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '(' || c == ')') { toks.Add(c.ToString()); i++; continue; }
                if (i + 1 < s.Length)
                {
                    string two = s.Substring(i, 2);
                    if (two == "&&" || two == "||" || two == "==" || two == "!=") { toks.Add(two); i += 2; continue; }
                }
                if (c == '!') { toks.Add("!"); i++; continue; }
                if (c == '\'' || c == '"')
                {
                    int j = s.IndexOf(c, i + 1);
                    if (j < 0) throw new ForgeException(where + ": unclosed quote in condition '" + s + "'");
                    toks.Add("'" + s.Substring(i + 1, j - i - 1));
                    i = j + 1;
                    continue;
                }
                int k = i;
                // A word runs until whitespace or an operator, so literals like ~mods or 4.27 need no quotes.
                while (k < s.Length && !char.IsWhiteSpace(s[k]) && "()!&|=\"'".IndexOf(s[k]) < 0) k++;
                if (k == i) throw new ForgeException(where + ": cannot read condition '" + s + "'");
                toks.Add(s.Substring(i, k - i));
                i = k;
            }
            return toks;
        }

        static bool Or(List<string> t, ref int i, IDictionary<string, string> vars, string where, string expr)
        {
            bool r = And(t, ref i, vars, where, expr);
            while (i < t.Count && t[i] == "||") { i++; bool b = And(t, ref i, vars, where, expr); r = r || b; }
            return r;
        }

        static bool And(List<string> t, ref int i, IDictionary<string, string> vars, string where, string expr)
        {
            bool r = Unary(t, ref i, vars, where, expr);
            while (i < t.Count && t[i] == "&&") { i++; bool b = Unary(t, ref i, vars, where, expr); r = r && b; }
            return r;
        }

        static bool Unary(List<string> t, ref int i, IDictionary<string, string> vars, string where, string expr)
        {
            if (i >= t.Count) throw new ForgeException(where + ": condition '" + expr + "' ends early");
            if (t[i] == "!") { i++; return !Unary(t, ref i, vars, where, expr); }
            if (t[i] == "(")
            {
                i++;
                bool r = Or(t, ref i, vars, where, expr);
                if (i >= t.Count || t[i] != ")") throw new ForgeException(where + ": missing ) in condition '" + expr + "'");
                i++;
                return r;
            }
            string name = t[i++];
            string v;
            if (!vars.TryGetValue(name, out v))
                throw new ForgeException(where + ": unknown name '" + name + "' in condition '" + expr + "'");
            v = v ?? "";
            if (i < t.Count && (t[i] == "==" || t[i] == "!="))
            {
                bool eq = t[i] == "==";
                i++;
                if (i >= t.Count) throw new ForgeException(where + ": condition '" + expr + "' ends early");
                string lit = t[i++];
                if (lit.StartsWith("'", StringComparison.Ordinal)) lit = lit.Substring(1);
                bool same = string.Equals(v, lit, StringComparison.OrdinalIgnoreCase);
                return eq ? same : !same;
            }
            return Truthy(v);
        }
    }
}
