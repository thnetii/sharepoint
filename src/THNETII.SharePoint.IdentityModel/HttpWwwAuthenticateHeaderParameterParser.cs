using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THNETII.SharePoint.IdentityModel
{
    internal class HttpWwwAuthenticateHeaderParameterParser
    {
        private readonly string source;
        private int _i;
        private readonly StringBuilder stringBuilder;

        private HttpWwwAuthenticateHeaderParameterParser(string param)
        {
            source = param ?? string.Empty;
            stringBuilder = new StringBuilder(capacity: source.Length);
        }

        public static Dictionary<string, string> Parse(string param)
        {
            var state = new HttpWwwAuthenticateHeaderParameterParser(param);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var token = state.ReadToken();
            while (!string.IsNullOrEmpty(token))
            {
                if (!state.ReadDelim('='))
                    return result;
                result[token] = state.ReadString();
                if (!state.ReadDelim(','))
                    return result;
                token = state.ReadToken();
            }
            return result;
        }

        private string ReadToken()
        {
            var start = _i;
            while (_i < source.Length && ValidTokenChar(source[_i]))
                _i++;
            return
#if CSHARP_LANG_FEATURE_RANGE_INDEX
                source[start.._i]
#else
                source.Substring(start, _i - start)
#endif
                ;
        }

        private bool ReadDelim(char ch)
        {
            while (_i < source.Length && char.IsWhiteSpace(source[_i]))
                _i++;
            if (_i >= source.Length || source[_i] != ch)
                return false;
            _i++;
            while (_i < source.Length && char.IsWhiteSpace(source[_i]))
                _i++;
            return true;
        }

        private string ReadString()
        {
            if (_i < source.Length && source[_i] == '"')
            {
                var buffer = stringBuilder;
                buffer.Clear();
                _i++;
                while (_i < source.Length)
                {
                    if (source[_i] == '\\' && (_i + 1) < source.Length)
                    {
                        _i++;
                        buffer.Append(source[_i]);
                        _i++;
                    }
                    else if (source[_i] == '"')
                    {
                        _i++;
                        return buffer.ToString();
                    }
                    else
                    {
                        buffer.Append(source[_i]);
                        _i++;
                    }
                }
                return buffer.ToString();
            }
            else
            {
                return ReadToken();
            }
        }

        private static bool ValidTokenChar(char ch)
        {
            if (ch < 32)
                return false;
            if (ch == '(' || ch == ')' || ch == '<' || ch == '>' || ch == '@'
                || ch == ',' || ch == ';' || ch == ':' || ch == '\\' || ch == '"'
                || ch == '/' || ch == '[' || ch == ']' || ch == '?' || ch == '='
                || ch == '{' || ch == '}' || ch == 127 || ch == ' ' || ch == '\t')
                return false;
            return true;
        }
    }
}
