using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.Shared
{
    public class NetworkObjectIntigraion
    {
        static NetworkObjectIntigraion()
        {
            EndpointBuffer = typeof(NetworkObjectIntigraion).GetMethod("EndpointAdapterCallerOneWay");
        }

        public static readonly MethodInfo EndpointBuffer;

        public T CreateClassProxy<T>() where T : class
        {
            //Check for interfaces

            var type = typeof(T);

            if (!type.IsInterface)
            {
                throw new ArgumentException("Type must be and Interface", "T");
            }

            var builder = GetTypeBuilder<T>();
            var constructor = builder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var mehtods = type.GetMethods();

            foreach (var item in mehtods)
            {
                CreateMethod(builder, item, type);
            }
            var createdType = builder.CreateType();
            return Activator.CreateInstance(createdType) as T;
        }

        public static void EndpointAdapterCallerOneWay(object caller, string mehtodName)
        {

        }

        private void CreateMethod(TypeBuilder builder, MethodInfo item, Type caller)
        {
            var methBuilder = builder.DefineMethod(item.Name, MethodAttributes.Public | MethodAttributes.Virtual, item.CallingConvention, item.ReturnType, item.GetParameters().Select(s => s.ParameterType).ToArray());
            var gen = methBuilder.GetILGenerator();
            var that = gen.DeclareLocal(caller);
            var thatName = gen.DeclareLocal(typeof(string));

            //get local GetType method and load it
            gen.Emit(OpCodes.Ldarg, that.LocalIndex);
            gen.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);

            //gen.Emit(OpCodes.Ldarg, thatName.LocalIndex);
            //gen.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);
            //gen.EmitCall(OpCodes.Call, typeof(object).GetMethod("ToString"), null);

            //gen.Emit(OpCodes.Ldarg, that.LocalIndex);
            //gen.Emit(OpCodes.Ldarg, thatName.LocalIndex);
            //gen.EmitCall(OpCodes.Call, EndpointBuffer, new Type[0]);

            gen.Emit(OpCodes.Ret);
        }

        private static TypeBuilder GetTypeBuilder<T>()
        {
            var typeSignature = "NetworkObjectIntigrationProxy_" + Guid.NewGuid();
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , null
                                , new[] { typeof(T) });
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
