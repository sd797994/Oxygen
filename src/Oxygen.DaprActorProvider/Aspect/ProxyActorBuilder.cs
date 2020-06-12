using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Oxygen.DaprActorProvider.Aspect
{
    public class ProxyActorBuilder
    {
        private readonly AssemblyBuilder _ab;
        private readonly ModuleBuilder _mb;
        private readonly TypeBuilder _tb;
        public ProxyActorBuilder(Type interfaceType, Type implType)
        {
            _ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ProxyBuilder"), AssemblyBuilderAccess.Run);
            _mb = _ab.DefineDynamicModule("ProxyModule");
            _tb = _mb.DefineType("Proxy_" + implType.Name, TypeAttributes.Public, implType);
            _tb.AddInterfaceImplementation(interfaceType);
            ConstructorInfo objCtor = implType.GetConstructors()[0];
            var args = objCtor.GetParameters().Select(x => x.ParameterType).ToArray();
            var constructorBuilder = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);
            var autofacContainerField = _tb.DefineField("container", typeof(ILifetimeScope), FieldAttributes.Private);
            ILGenerator ilOfCtor = constructorBuilder.GetILGenerator();
            ilOfCtor.Emit(OpCodes.Ldarg_0);
            for (var i = 0; i < args.Length; i++)
            {
                ilOfCtor.Emit(OpCodes.Ldarg, i + 1);
            }
            ilOfCtor.Emit(OpCodes.Call, objCtor);
            ilOfCtor.Emit(OpCodes.Ldarg_0);
            ilOfCtor.Emit(OpCodes.Ldarg, args.Length);
            ilOfCtor.Emit(OpCodes.Stfld, autofacContainerField);//将最后一个构造函数参数赋值给autofac contrainer field
            ilOfCtor.Emit(OpCodes.Ret);

            interfaceType.GetMethods().ToList().ForEach(inferfacemethod =>
            {
                var method = implType.GetMethod(inferfacemethod.Name, inferfacemethod.GetParameters().Select(x => x.ParameterType).ToArray());
                var methodbuilder = _tb.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray());
                var ilOfMethod = methodbuilder.GetILGenerator();
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                for (var i = 0; i < method.GetParameters().Length; i++)
                {
                    ilOfMethod.Emit(OpCodes.Ldarg, i + 1);
                }
                ilOfMethod.Emit(OpCodes.Call, method);
                //调用MediatR进行异步分发
                var loc = ilOfMethod.DeclareLocal(typeof(object));
                ilOfMethod.Emit(OpCodes.Stloc, loc);
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                ilOfMethod.Emit(OpCodes.Ldfld, autofacContainerField);
                ilOfMethod.Emit(OpCodes.Call, typeof(ActorStateSavePublisher).GetMethod("Publish", new Type[] { typeof(object), typeof(ILifetimeScope) }));
                ilOfMethod.Emit(OpCodes.Ldloc_0);
                ilOfMethod.Emit(OpCodes.Ret);
                _tb.DefineMethodOverride(methodbuilder, inferfacemethod);
            });
        }
        public Type CreateType()
        {
            return _tb.CreateTypeInfo()!.AsType();
        }
    }
}
