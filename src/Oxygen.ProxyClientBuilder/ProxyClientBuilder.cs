﻿using Autofac;
using Microsoft.Extensions.DependencyModel;
using Oxygen.CommonTool;
using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Oxygen.ProxyClientBuilder
{
    /// <summary>
    /// 代理服务创建类
    /// </summary>
    public class ProxyClientBuilder
    {
        /// <summary>
        /// 为远程服务构建代理类
        /// </summary>
        /// <param name="builder"></param>
        public static void CreateRemoteProxyClientInstance(ContainerBuilder builder)
        {
            var remote = RpcInterfaceType.Types.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote)
                {
                    builder.RegisterInstance(CreateTypeInstance(type)).As(type);
                }
            }
        }

        public static object CreateTypeInstance(Type interfaceType)
        {
            var targetType = typeof(RemoteProxyDecorator<>).MakeGenericType(interfaceType);
            return targetType.GetMethod("Create").Invoke(Activator.CreateInstance(targetType), null);
        }
    }
}