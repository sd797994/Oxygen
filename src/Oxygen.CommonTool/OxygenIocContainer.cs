using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Oxygen.CommonTool
{
    public class OxygenIocContainer
    {
        private static AsyncLocal<ILifetimeScope> Current = new AsyncLocal<ILifetimeScope>();
        public static void BuilderIocContainer(ILifetimeScope container)
        {
            Current.Value = container;
        }
        public static void DisposeIocContainer()
        {
            Current.Value = null;
        }
        public static T Resolve<T>()
        {
            try
            {
                if (Current == null)
                {
                    throw new Exception("IOC实例化出错!");
                }
                else
                {
                    return Current.Value.Resolve<T>();
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
                if (Current == null)
                {
                    throw new Exception("IOC实例化出错!");
                }
                else
                {
                    return Current.Value.Resolve(type);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
