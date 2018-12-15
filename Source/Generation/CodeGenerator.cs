using System.Reflection;

namespace Atko.Mirra.Generation
{
    abstract class CodeGenerator
    {
        public abstract StaticGetInvoker StaticGetter(MemberInfo accessor);
        public abstract InstanceGetInvoker InstanceGetter(MemberInfo accessor);
        public abstract StaticSetInvoker StaticSetter(MemberInfo accessor);
        public abstract InstanceSetInvoker InstanceSetter(MemberInfo accessor);
        public abstract StaticMethodInvoker StaticMethod(MethodInfo method, int argumentCount);
        public abstract InstanceMethodInvoker InstanceMethod(MethodInfo method, int argumentCount);
        public abstract StaticMethodInvoker Constructor(ConstructorInfo constructor, int argumentCount);
        public abstract IndexerGetInvoker InstanceIndexGetter(PropertyInfo property, int argumentCount);
        public abstract IndexerSetInvoker InstanceIndexSetter(PropertyInfo property, int argumentCount);

        protected static bool IsAccessor(MemberInfo member)
        {
            return (member.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0;
        }
    }
}