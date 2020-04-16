using Autofac;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            foreach(var customer in OxygenSetting.CustomHeader)
            {
                if (headers.TryGetAsString(new AsciiString(customer), out string val))
                {
                    result.Add(customer, val);
                }
            }
            return result;
        }
        public static Dictionary<string, string> GetTraceHeaders(IHeaderDictionary headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var trace in traceHeader)
            {
                if (headers.TryGetValue(trace, out StringValues val))
                {
                    result.Add(trace, val.ToString());
                }
            }
            foreach (var customer in OxygenSetting.CustomHeader)
            {
                if (headers.TryGetValue(customer, out StringValues val))
                {
                    result.Add(customer, val);
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
        public static void BuildTraceHeader(System.Net.Http.Headers.HttpHeaders headers, Dictionary<string, string> traceObj = null)
        {
            if (traceObj != null && traceObj.Count > 0)
            {
                foreach (var obj in traceObj)
                {
                    headers.Add(obj.Key, obj.Value);
                }
            }
        }
    }
    /// <summary>
    /// 追踪头中间件
    /// </summary>
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILifetimeScope container;
        public RequestMiddleware(
            RequestDelegate next, ILifetimeScope container)
        {
            _next = next;
            this.container = container;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //每次请求将重新初始化全局容器确保容器唯一
            OxygenIocContainer.BuilderIocContainer(container);
            //为客户端信息添加追踪头
            OxygenIocContainer.Resolve<CustomerInfo>().SetTraceHeader(TraceHeaderHelper.GetTraceHeaders(httpContext.Request.Headers));
            await _next(httpContext);
        }
    }
}
