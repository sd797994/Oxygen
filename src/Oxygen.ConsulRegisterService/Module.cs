using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Autofac;
using Oxygen.IMicroRegisterService;

namespace Oxygen.ConsulRegisterService
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsulCenterService>().As<IRegisterCenterService>().SingleInstance();
        }
    }
}
