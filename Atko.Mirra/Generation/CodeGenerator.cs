using System.Reflection;

namespace Atko.Mirra.Generation
{
    abstract class CodeGenerator
    {
        const bool UseDynamic = true;

        static CodeGenerator Reflection { get; } = new ReflectionGenerator();

#if HAVE_DYNAMIC
        static CodeGenerator Dynamic { get; } = new DynamicGenerator();
#endif

#if HAVE_DYNAMIC
        public static CodeGenerator Instance { get; } = UseDynamic ? Dynamic : Reflection;
#else
        public static CodeGenerator Instance { get; } = Reflection;
#endif

        public abstract StaticGetInvoker StaticGetter(MemberInfo accessor);
        public abstract InstanceGetInvoker InstanceGetter(MemberInfo accessor);
        public abstract StaticSetInvoker StaticSetter(MemberInfo accessor);
        public abstract InstanceSetInvoker InstanceSetter(MemberInfo accessor);
        public abstract StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount);
        public abstract InstanceMethodInvoker InstanceMethod(MethodInfo method, int argumentCount);
        public abstract StaticMethodInvoker Constructor(ConstructorInfo constructor, int argumentCount);
        public abstract InstanceIndexerGetInvoker InstanceIndexGetter(PropertyInfo property, int argumentCount);
        public abstract InstanceIndexerSetInvoker InstanceIndexSetter(PropertyInfo property, int argumentCount);

        protected static bool IsAccessor(MemberInfo member)
        {
            return (member.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0;
        }
    }
}