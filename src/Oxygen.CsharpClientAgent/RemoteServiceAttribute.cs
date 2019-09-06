using System;

namespace Oxygen.CsharpClientAgent
{
    /// <summary>
    /// 远程服务标记物
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class RemoteServiceAttribute : Attribute
    {
        public RemoteServiceAttribute(string serverName)
        {
            ServerName = serverName;
        }
        public string ServerName { get; set; }
    }
}
