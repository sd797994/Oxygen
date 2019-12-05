using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Oxygen.CommonTool;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;
using Oxygen.ServerProxyFactory;

namespace ApiGateWay.Controllers
{
    [Route("api/{*service}")]
    [ApiController]
    public class RouteGateController : ControllerBase
    {
        private readonly IServerProxyFactory _serverProxyFactory;
        private IHttpContextAccessor _accessor;
        private CustomerInfo _customerInfo;
        public RouteGateController(IServerProxyFactory serverProxyFactory, IHttpContextAccessor accessor, CustomerInfo customerInfo)
        {
            _serverProxyFactory = serverProxyFactory;
            _accessor = accessor;
            _customerInfo = customerInfo;
        }
        // GET api/values
        [HttpPost]
        public async Task<IActionResult> Invoke(JObject input)
        {
            if (input != null)
            {
                var remoteProxy =  _serverProxyFactory.CreateProxy(Request.Path);
                if (remoteProxy != null)
                {
                    _customerInfo.Ip = new System.Net.IPEndPoint(_accessor.HttpContext.Connection.RemoteIpAddress, _accessor.HttpContext.Connection.RemotePort);
                    var rempteResult = await remoteProxy.SendAsync(input.ToString());
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
