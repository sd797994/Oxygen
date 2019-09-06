using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// 全局通用工具接口
    /// </summary>
    public interface IGlobalCommon
    {
        /// <summary>
        /// 获取可用端口号
        /// </summary>
        /// <returns></returns>
        int GetFreePort();
        /// <summary>
        /// 获取本机在局域网内的ip
        /// </summary>
        /// <returns></returns>
        IPAddress GetMachineIp();
    }
}
