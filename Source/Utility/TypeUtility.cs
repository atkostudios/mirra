using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;

namespace Atko.Dodge.Utility
{
    static class TypeUtility
    {
        public const BindingFlags InstanceBinding = BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.DeclaredOnly;

        public const BindingFlags StaticBinding = BindingFlags.Static |
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.DeclaredOnly;

        const string BackingFieldPrefix = "<";
        const string BackingFieldSuffix = ">k__BackingField";

        static Cache<(Type, Type), Type> ImplementationCache { get; }
            = new Cache<(Type, Type), Type>();

        static Cache<(Type, bool, string), MemberInfo> AccessorCache { get; } =
            new Cache<(Type, bool, string), MemberInfo>();

        static Cache<(Type, bool, string), FieldInfo> FieldCache { get; } =
            new Cache<(Type, bool, string), FieldInfo>();

        static Cache<(Type, bool, string, ArrayHash<Type>), PropertyInfo> PropertyCache { get; } =
            new Cache<(Type, bool, string, ArrayHash<Type>), PropertyInfo>();

        static Cache<(Type, bool, string, ArrayHash<Type>), MethodInfo> MethodCache { get; } =
            new Cache<(Type, bool, string, ArrayHash<Type>), MethodInfo>();

        static Cache<(Type, ArrayHash<Type>), ConstructorInfo> ConstructorCache { get; } =
            new Cache<(Type, ArrayHash<Type>), ConstructorInfo>();

        static Cache<(PropertyInfo, bool), FieldInfo> BackingFieldCache { get; } =
            new Cache<(PropertyInfo, bool), FieldInfo>();

        static BindingFlags GetBindings(bool instance)
        {
            return instance ? InstanceBinding : StaticBinding;
        }

        public static IEnumerable<Type> Inheritance(this Type type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        [return: AllowNull]
        public static MemberInfo GetAccessor(Type type, bool instance, string name)
        {
            return AccessorCache.GetOrAdd((type, instance, name),
                (input) => GetAccessorInternal(input.Item1, input.Item2, input.Item3));
        }

        [return: AllowNull]
        public static FieldInfo GetField(Type type, bool instance, string name)
        {
            return FieldCache.GetOrAdd((type, instance, name),
                (input) => GetFieldInternal(input.Item1, input.Item2, input.Item3));
        }

        [return: AllowNull]
        public static PropertyInfo GetProperty(Type type, bool instance, string name, Type[] types = null)
        {
            return PropertyCache.GetOrAdd((type, instance, name, new ArrayHash<Type>(types ?? Array.Empty<Type>())),
                (input) => GetPropertyInternal(input.Item1, input.Item2, input.Item3, input.Item4.Array));
        }

        [return: AllowNull]
        public static MethodInfo GetMethod(Type type, bool instance, string name, Type[] types)
        {
            return MethodCache.GetOrAdd((type, instance, name, new ArrayHash<Type>(types)),
                (input) => GetMethodInternal(input.Item1, input.Item2, input.Item3, input.Item4.Array));
        }

        [return: AllowNull]
        public static ConstructorInfo GetConstructor(Type type, Type[] types)
        {
            return ConstructorCache.GetOrAdd((type, new ArrayHash<Type>(types)),
                (input) => GetConstructorInternal(input.Item1, input.Item2.Array));
        }

        [return: AllowNull]
        public static FieldInfo GetBackingField(PropertyInfo property, bool instance)
        {
            return BackingFieldCache.GetOrAdd((property, instance),
                (input) => GetBackingFieldInternal(input.Item1, input.Item2));
        }

        [return: AllowNull]
        public static Type GetImplementation(Type type, Type generic)
        {
            if (type == generic)
            {
                return type;
            }

            return ImplementationCache.GetOrAdd((type, generic),
                (input) => GetImplementationInternal(input.Item1, input.Item2));
        }

        public static string GetBackingFieldName(PropertyInfo property)
        {
            return $"{BackingFieldPrefix}{property.Name}{BackingFieldSuffix}";
        }

        public static bool IsBackingField(this FieldInfo field)
        {
            return field.IsDefined(typeof(CompilerGeneratedAttribute)) &&
                   field.Name.StartsWith(BackingFieldPrefix) &&
                   field.Name.EndsWith(BackingFieldSuffix);
        }

        [return: AllowNull]
        static MemberInfo GetAccessorInternal(Type type, bool instance, string name)
        {
            var field = GetField(type, instance, name);
            if (field != null)
            {
                return field;
            }

            var property = GetProperty(type, instance, name);
            var backing = GetBackingField(property, instance);
            if (backing != null)
            {
                return backing;
            }

            return property;
        }

        [return: AllowNull]
        static FieldInfo GetFieldInternal(Type type, bool instance, string name)
        {
            if (!instance)
            {
                return type.GetField(name, StaticBinding);
            }

            foreach (var ancestor in Inheritance(type))
            {
                var field = ancestor.GetField(name, InstanceBinding);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        [return: AllowNull]
        static PropertyInfo GetPropertyInternal(Type type, bool instance, string name, Type[] types = null)
        {
            if (!instance)
            {
                return type.GetProperty(name, StaticBinding, null, null, types, null);
            }

            foreach (var ancestor in Inheritance(type))
            {
                var property = ancestor.GetProperty(name, InstanceBinding, null, null, types, null);
                if (property != null)
                {
                    return property;
                }

                foreach (var implemented in ancestor.GetInterfaces())
                {
                    property = implemented.GetProperty(name, InstanceBinding, null, null, types, null);
                    if (property != null)
                    {
                        return property;
                    }
                }
            }

            return null;
        }

        [return: AllowNull]
        static MethodInfo GetMethodInternal(Type type, bool instance, string name, Type[] types)
        {
            if (!instance)
            {
                return type.GetMethod(name, StaticBinding, null, types, null);
            }

            foreach (var ancestor in Inheritance(type))
            {
                var method = ancestor.GetMethod(name, InstanceBinding, null, types, null);
                if (method != null)
                {
                    return method;
                }

                foreach (var implemented in ancestor.GetInterfaces())
                {
                    method = implemented.GetMethod(name, InstanceBinding, null, types, null);
                    if (method != null)
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        [return: AllowNull]
        static ConstructorInfo GetConstructorInternal(Type type, Type[] types)
        {
            foreach (var ancestor in Inheritance(type))
            {
                var method = ancestor.GetConstructor(InstanceBinding, null, types, null);
                if (method != null)
                {
                    return method;
                }
            }

            return null;
        }

        [return: AllowNull]
        static FieldInfo GetBackingFieldInternal(PropertyInfo property, bool instance)
        {
            if (property.GetGetMethod(true).IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                var name = GetBackingFieldName(property);
                var type = property.DeclaringType;
                if (type == null)
                {
                    return null;
                }

                var field = GetField(type, instance, name);
                if (field == null)
                {
                    return null;
                }

                if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    return field;
                }

                return null;
            }

            return null;
        }

        static Type GetImplementationInternal(Type type, Type generic)
        {
            if (!generic.IsGenericType || !generic.IsGenericTypeDefinition)
            {
                return generic.IsAssignableFrom(type) ? generic : null;
            }

            if (generic.IsInterface)
            {
                foreach (var ancestor in Inheritance(type))
                {
                    var interfaces = ancestor.GetInterfaces();
                    foreach (var implemented in interfaces)
                    {
                        if (implemented.IsGenericType &&
                            implemented.GetGenericTypeDefinition() == generic)
                        {
                            return implemented;
                        }
                    }
                }
            }
            else
            {
                foreach (var ancestor in Inheritance(type))
                {
                    if (ancestor.IsGenericType &&
                        ancestor.GetGenericTypeDefinition() == generic)
                    {
                        return ancestor;
                    }
                }
            }

            return null;
        }

        public static Type[] GetTypes(object[] objects)
        {
            var types = new Type[objects.Length];
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = objects[i]?.GetType();
            }

            return types;
        }

        [return: AllowNull]
        public static Type GetReturnType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) member).PropertyType;
                case MemberTypes.Method:
                    return ((MethodInfo) member).ReturnType;
            }

            return null;
        }

        public static bool IsStatic(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    return false;
                case MemberTypes.Property:
                    var property = ((PropertyInfo) member);
                    if (property.CanRead)
                    {
                        return property.GetGetMethod(true).IsStatic;
                    }

                    return property.GetSetMethod(true).IsStatic;
                case MemberTypes.Field:
                    return ((FieldInfo) member).IsStatic;
                case MemberTypes.Method:
                    return ((MethodInfo) member).IsStatic;
            }

            throw new ArgumentException(nameof(member));
        }
    }
}