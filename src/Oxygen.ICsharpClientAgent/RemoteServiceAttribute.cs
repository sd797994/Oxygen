using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.CsharpClientAgent
{
    /// <summary>
    /// 远程服务标记物
    /// </summary>
    public class RemoteServiceAttribute : Attribute
    {
        public RemoteServiceAttribute(string serverName)
        {
            ServerName = serverName;
        }
        public string ServerName { get; set; }
    }
}
