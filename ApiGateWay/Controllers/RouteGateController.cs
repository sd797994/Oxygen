using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Oxygen.CommonTool;
using Oxygen.IServerProxyFactory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiGateWay.Controllers
{
    [Route("api/{*service}")]
    [ApiController]
    public class RouteGateController : ControllerBase
    {
        private readonly IServerProxyFactory _serverProxyFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly CustomerInfo customerInfo;
        public RouteGateController(IServerProxyFactory serverProxyFactory, IHttpContextAccessor httpContextAccessor, CustomerInfo customerInfo)
        {
            _serverProxyFactory = serverProxyFactory;
            this.httpContextAccessor = httpContextAccessor;
            this.customerInfo = customerInfo;
        }
        // GET api/values
        [HttpPost]
        public async Task<IActionResult> Invoke(Dictionary<object, object> input)
        {
            if (input != null)
            {
                var remoteProxy =  _serverProxyFactory.CreateProxy(Request.Path);
                if (remoteProxy != null)
                {
                    //为客户端信息添加追踪头
                    customerInfo.TraceHeaders = TraceHeaderHelper.GetTraceHeaders(httpContextAccessor.HttpContext.Request.Headers);
                    var rempteResult = await remoteProxy.SendAsync(input);
                    if (rempteResult != null)
                    {
                        return new JsonResult(rempteResult);
                    }
                }
                else
                {
                    return Content("创建代理失败");
                }
            }
            return Content("无返回值");
        }
    }
}
