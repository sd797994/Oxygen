using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Oxygen.ISerializeService;
using Oxygen.IServerProxyFactory;

namespace ApiGateWay.Controllers
{
    [Route("api/{*service}")]
    [ApiController]
    public class RouteGateController : ControllerBase
    {
        private readonly IServerProxyFactory _serverProxyFactory;
        private readonly ISerialize _serialize;
        public RouteGateController(IServerProxyFactory serverProxyFactory, ISerialize serialize)
        {
            _serverProxyFactory = serverProxyFactory;
            _serialize = serialize;
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
