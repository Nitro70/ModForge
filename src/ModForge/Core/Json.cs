using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace ModForge.Core
{
    /// <summary>Thin helpers over the JSON reader that ships with .NET Framework, so the exe needs no extra dll.</summary>
    public static class Json
    {
        public static object Parse(string text)
        {
            var ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 256 };
            return ser.DeserializeObject(text);
        }

        public static Dictionary<string, object> ParseObject(string text)
        {
            var d = Parse(text) as Dictionary<string, object>;
            if (d == null) throw new ForgeException("expected a JSON object");
            return d;
        }

        public static Dictionary<string, object> Obj(Dictionary<string, object> d, string key)
        {
            object v;
            return d != null && d.TryGetValue(key, out v) ? v as Dictionary<string, object> : null;
        }

        public static IEnumerable<object> Arr(Dictionary<string, object> d, string key)
        {
            object v;
            if (d == null || !d.TryGetValue(key, out v) || v == null) yield break;
            var e = v as IEnumerable;
            if (e == null || v is string) yield break;
            foreach (var item in e) yield return item;
        }

        public static string Str(Dictionary<string, object> d, string key, string def = "")
        {
            object v;
            if (d == null || !d.TryGetValue(key, out v) || v == null) return def;
            if (v is bool) return (bool)v ? "true" : "";
            return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool Bool(Dictionary<string, object> d, string key, bool def = false)
        {
            object v;
            if (d == null || !d.TryGetValue(key, out v) || v == null) return def;
            if (v is bool) return (bool)v;
            var s = Convert.ToString(v);
            return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static int Int(Dictionary<string, object> d, string key, int def = 0)
        {
            object v;
            if (d == null || !d.TryGetValue(key, out v) || v == null) return def;
            int n;
            return int.TryParse(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture), out n) ? n : def;
        }

        /// <summary>Writes a flat string map as readable JSON. Enough for the settings file.</summary>
        public static string WriteMap(IDictionary<string, string> map)
        {
            var sb = new StringBuilder("{\r\n");
            int i = 0;
            foreach (var kv in map)
            {
                sb.Append("  ").Append(Quote(kv.Key)).Append(": ").Append(Quote(kv.Value ?? ""));
                sb.Append(++i < map.Count ? ",\r\n" : "\r\n");
            }
            return sb.Append("}\r\n").ToString();
        }

        public static string Quote(string s)
        {
            return "\"" + Escape(s) + "\"";
        }

        public static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
