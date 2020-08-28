using Autofac;
using Dapr.Actors;
using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oxygen.CommonTool;
using Oxygen.CommonTool.Logger;
using Oxygen.DaprActorProvider.StateManage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxygen.DaprActorProvider
{
    public class ActorServiceBuilder
    {
        /// <summary>
        /// 注册actor到容器并提供中介者用于自动保存
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorToContainer(ContainerBuilder builder)
        {
            builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(ActorStateSubscriber).GetTypeInfo().Assembly).AsClosedTypesOf(typeof(INotificationHandler<>)).AsImplementedInterfaces();
            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
        /// <summary>
        /// 通过actorruntime 注册actor
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterActorMiddleware(object builder, ILifetimeScope container)
        {
            ((IWebHostBuilder)builder).UseActors(x => RegisterActor(x, container));
        }
        /// <summary>
        /// 注册所有的actor服务
        /// </summary>
        /// <param name="runtime"></param>
        static void RegisterActor(ActorRuntime runtime, ILifetimeScope container)
        {
            var remote = RpcInterfaceType.ActorTypes.Value;
            if (remote != null && remote.Any())
            {
                foreach (var type in remote.Where(x => x.classType != null))
                {
                    Func<ActorTypeInformation, ActorService> createFunc = (info) => new ActorService(info, (actorService, actorId) =>
                    {
                        var actorInstance = ActorDelegateDir[type.classType](new object[] { actorService, actorId, container }) as OxygenActorBase;
                        actorInstance.AutoSave = type.autoSave;
                        return actorInstance;
                    });
                    ActorDelegateDir.Add(type.classType, GetActorDelegate(type.classType));
                    typeof(ActorRuntime).GetMethod("RegisterActor").MakeGenericMethod(type.classType).Invoke(runtime, new object[] { createFunc });
                }
            }
        }
        #region actor委托字典创建actor对象
        static Dictionary<Type, Func<object[], object>> ActorDelegateDir = new Dictionary<Type, Func<object[], object>>();
        static Func<object[], object> GetActorDelegate(Type type)
        {
            var ctor = type.GetConstructors()[0];
            ParameterInfo[] paramsInfo = ctor.GetParameters();
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }
            NewExpression newExp = Expression.New(ctor, argsExp);
            var lambda =
                Expression.Lambda<Func<object[], object>>(newExp, param);
            return lambda.Compile();
        }
        #endregion
    }
}