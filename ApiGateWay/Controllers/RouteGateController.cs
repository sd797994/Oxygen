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
        private readonly ISerialize _serialize;
        private IHttpContextAccessor _accessor;
        private CustomerIp _customerIp;
        public RouteGateController(IServerProxyFactory serverProxyFactory, ISerialize serialize, IHttpContextAccessor accessor, CustomerIp customerIp)
        {
            _serverProxyFactory = serverProxyFactory;
            _serialize = serialize;
            _accessor = accessor;
            _customerIp = customerIp;
        }
        // GET api/values
        [HttpPost]
        public async Task<IActionResult> Invoke(JObject input)
        {
            if (input != null)
            {
                var remoteProxy = await _serverProxyFactory.CreateProxy(Request.Path);
                if (remoteProxy != null)
                {
                    _customerIp.Ip = new System.Net.IPEndPoint(_accessor.HttpContext.Connection.RemoteIpAddress, _accessor.HttpContext.Connection.RemotePort);
                    var rempteResult = await remoteProxy.SendAsync(input.ToString());
                    if (rempteResult != null)
                    {
                        return new JsonResult(rempteResult);
                    }
                }
            }
            return Content("无返回值");
        }
    }
}
