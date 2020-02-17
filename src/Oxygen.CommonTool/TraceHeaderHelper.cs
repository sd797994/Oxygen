using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.CommonTool
{
    public class TraceHeaderHelper
    {
        static string[] traceHeader = { "x-request-id", "x-b3-traceid", "x-b3-spanid", "x-b3-parentspanid", "x-b3-sampled", "x-b3-flags", "x-ot-span-context" };
        public static Dictionary<string, string> GetTraceHeaders(HttpHeaders headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var trace in traceHeader)
            {
                if (headers.TryGetAsString(new AsciiString(trace), out string val))
                {
                    result.Add(trace, val);
                }
            }
            return result;
        }
        public static Dictionary<string, string> GetTraceHeaders(IHeaderDictionary headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var header in traceHeader)
            {
                if (headers.TryGetValue(header, out StringValues val))
                {
                    result.Add(header, val.ToString());
                }
            }
            return result;
        }


        public static void BuildTraceHeader(HttpHeaders headers, Dictionary<string, string> traceObj = null)
        {
            if (traceObj != null && traceObj.Count > 0)
            {
                foreach (var obj in traceObj)
                {
                    headers.Set(new AsciiString(obj.Key), obj.Value);
                }
            }
        }
    }
}
