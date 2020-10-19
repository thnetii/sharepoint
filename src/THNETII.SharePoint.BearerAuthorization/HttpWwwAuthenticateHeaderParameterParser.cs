using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace THNETII.SharePoint.BearerAuthorization
{
    internal static class HttpWwwAuthenticateHeaderParameterParser
    {
        private static readonly char[] invalidTokenChars =
            Enumerable.Range(0, 32).Select(i => (char)i).Concat(new[]
            {
                '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '/', '[',
                ']', '?', '=', '{', '}', (char)127, ' ', '\t'
            }).Distinct().ToArray();

        public static Dictionary<string, string> Parse(string param)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var buffer = param.AsSpan();
            for (var token = ReadToken(ref buffer); !token.IsEmpty; token = ReadToken(ref buffer))
            {
                if (!ReadDelim('=', ref buffer))
                    return result;
                result[token.ToString()] = ReadString(ref buffer);
                if (!ReadDelim(',', ref buffer))
                    return result;
            }

            return result;
        }


        private static ReadOnlySpan<char> ReadToken(ref ReadOnlySpan<char> buffer)
        {
            int end = buffer.IndexOfAny(invalidTokenChars);
            if (end < 0)
                return ReadOnlySpan<char>.Empty;
            var result = buffer.Slice(0, end);
            buffer = buffer.Slice(end);
            return result;
        }

        private static bool ReadDelim(char ch, ref ReadOnlySpan<char> buffer)
        {
            buffer = buffer.TrimStart();
            if (buffer.IsEmpty || buffer[0] != ch)
                return false;
            buffer = buffer.Slice(1).TrimStart();
            return true;
        }

        private static string ReadString(ref ReadOnlySpan<char> buffer)
        {
            if (buffer.IsEmpty || buffer[0] != '"')
                return ReadToken(ref buffer).ToString();

            var stringBuilder = new StringBuilder(buffer.Length);
            int i;
            for (i = 1; i < buffer.Length; i++)
            {
                switch (buffer[i])
                {
                    case '\\':
                        i++;
                        goto default;
                    case '"':
                        buffer = buffer.Slice(i + 1);
                        return stringBuilder.ToString();
                    default:
                        stringBuilder.Append(buffer[i]);
                        break;
                }
            }

            buffer = ReadOnlySpan<char>.Empty;
            return stringBuilder.ToString();
        }
    }
}
