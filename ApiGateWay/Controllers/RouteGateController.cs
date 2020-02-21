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
        public RouteGateController(IServerProxyFactory serverProxyFactory)
        {
            _serverProxyFactory = serverProxyFactory;
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
