using System;
using System.Reflection;
using NullGuard;
using Utility;

namespace Ducktype
{
    public abstract class MemberModel
    {
        public string Name => Member.Name;
        public bool IsInstance { get; }

        public MemberInfo Member { get; }

        protected MemberModel(MemberInfo member, bool instance)
        {
            Member = member;
            IsInstance = instance;
        }
    }

    public abstract class KeyModel : MemberModel
    {
        Func<object, object> InstanceGetterDelegate { get; }
        Action<object, object> InstanceSetterDelegate { get; }

        Func<object> StaticGetterDelegate { get; }
        Action<object> StaticSetterDelegate { get; }

        protected KeyModel(PropertyInfo property, bool instance) : base(property, instance) { }
        protected KeyModel(FieldInfo property, bool instance) : base(property, instance) { }

        KeyModel(MemberInfo property, bool instance) : base(property, instance)
        {
            if (IsInstance)
            {
                InstanceGetterDelegate = DelegateCreator.CreateInstanceGetterDelegate(property);
                InstanceSetterDelegate = DelegateCreator.CreateInstanceSetterDelegate(property);
            }
            else
            {
                StaticGetterDelegate = DelegateCreator.CreateStaticGetterDelegate(property);
                StaticSetterDelegate = DelegateCreator.CreateStaticSetterDelegate(property);
            }
        }
    }

    public class PropertyModel : KeyModel {
        public PropertyModel(PropertyInfo property, bool instance) : base(property, instance) { }
    }

    public class FieldModel : KeyModel {
        public FieldModel(FieldInfo property, bool instance) : base(property, instance) { }
    }

    public class MethodModel : MemberModel
    {
        Func<object, object[], object> InstanceDelegate { get; }
        Func<object[], object> StaticDelegate { get; }

        public MethodModel(MethodInfo method, bool instance) : base(method, instance)
        {
            throw new NotImplementedException();
        }

        public object Call([AllowNull] object instance, params object[] arguments)
        {
            return IsInstance
                ? InstanceDelegate.Invoke(instance, arguments)
                : StaticDelegate.Invoke(arguments);
        }
    }
}