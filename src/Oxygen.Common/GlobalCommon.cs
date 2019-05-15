using Oxygen.Common.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Oxygen.Common
{
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
    /// <summary>
    /// 一些通用的帮助类
    /// </summary>
    public class GlobalCommon: IGlobalCommon
    {
        private IOxygenLogger _logger;
        public GlobalCommon(IOxygenLogger logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 获取可用端口号
        /// </summary>
        /// <returns></returns>
        public int GetFreePort()
        {
            //检查指定端口是否已用
            bool PortIsAvailable(int port)
            {
                bool isAvailable = true;
                IList portUsed = PortIsUsed();
                foreach (int p in portUsed)
                {
                    if (p == port)
                    {
                        isAvailable = false; break;
                    }
                }
                return isAvailable;
            }
            //获取操作系统已用的端口号
            IList PortIsUsed()
            {
                //获取本地计算机的网络连接和通信统计数据的信息
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                //返回本地计算机上的所有Tcp监听程序
                IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
                //返回本地计算机上的所有UDP监听程序
                IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
                //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
                IList allPorts = new ArrayList();
                foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
                foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
                foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);
                return allPorts;
            }
            int MAX_PORT = 65535; //系统tcp/udp端口数最大是65535            
            int BEGIN_PORT = 5000;//从这个端口开始检测
            var usePort = new List<int>();
            while (true)
            {
                var randomPort = new Random(Guid.NewGuid().GetHashCode()).Next(BEGIN_PORT, MAX_PORT);
                if (PortIsAvailable(randomPort))
                {
                    return randomPort;
                }
                else
                {
                    usePort.Add(randomPort);
                }
                if (usePort.Count == MAX_PORT - BEGIN_PORT)
                {
                    break;
                }
            }
            _logger.LogError("没有找到可用端口号");
            return -1;
        }

        public IPAddress GetMachineIp()
        {
            IPAddress result = default(IPAddress);
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties ipxx = adapter.GetIPProperties();
                    UnicastIPAddressInformationCollection ipCollection = ipxx.UnicastAddresses;
                    foreach (UnicastIPAddressInformation ipadd in ipCollection)
                    {
                        if (ipadd.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            result = ipadd.Address;
                        }
                    }
                }
            }
            return result;
        }
    }
}
