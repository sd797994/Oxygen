using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.CommonTool
{
    public class OxygenIocContainer
    {
        private static ILifetimeScope _container;

        public static void BuilderIocContainer(ILifetimeScope container)
        {
            _container = container;
        }

        public static T Resolve<T>()
        {
            try
            {
                if (_container == null)
                {
                    throw new Exception("IOC实例化出错!");
                }
                else
                {
                    return _container.Resolve<T>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static object Resolve(Type type)
        {
            try
            {
                if (_container == null)
                {
                    throw new Exception("IOC实例化出错!");
                }
                else
                {
                    return _container.Resolve(type);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
