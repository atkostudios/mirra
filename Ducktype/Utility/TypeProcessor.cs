using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NullGuard;

using static System.Reflection.BindingFlags;

namespace Utility
{
    static class TypeProcessor
    {
        public const BindingFlags InstanceBinding = Instance | Public | NonPublic | DeclaredOnly;
        public const BindingFlags StaticBinding = Static | Public | NonPublic | DeclaredOnly;

        const string BackingFieldPrefix = "<";
        const string BackingFieldSuffix = ">k__BackingField";

        static ConcurrentDictionary<(Type, Type), Type> ImplementationCache { get; }
            = new ConcurrentDictionary<(Type, Type), Type>();

        static ConcurrentDictionary<(Type, bool, string), FieldInfo> FieldCache { get; } =
            new ConcurrentDictionary<(Type, bool, string), FieldInfo>();
        static ConcurrentDictionary<(Type, bool, string, ArrayHash<Type>), PropertyInfo> PropertyCache { get; } =
            new ConcurrentDictionary<(Type, bool, string, ArrayHash<Type>), PropertyInfo>();
        static ConcurrentDictionary<(Type, bool, string, ArrayHash<Type>), MethodInfo> MethodCache { get; } =
            new ConcurrentDictionary<(Type, bool, string, ArrayHash<Type>), MethodInfo>();

        static ConcurrentDictionary<(PropertyInfo, bool), FieldInfo> BackingFieldCache { get; } =
            new ConcurrentDictionary<(PropertyInfo, bool), FieldInfo>();

        static ConcurrentDictionary<(Type, bool, string), MemberInfo> GetFieldOrPropertyCache { get; } =
            new ConcurrentDictionary<(Type, bool, string), MemberInfo>();

        static BindingFlags GetBindings(bool instance)
        {
            return instance ? InstanceBinding : StaticBinding;
        }

        public static IEnumerable<Type> Inheritance(Type type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static bool Get([AllowNull] object instance, Type type, string name, [AllowNull] out object value)
        {
            var member = GetFieldOrProperty(type, instance != null, name);
            if (member == null)
            {
                value = null;
                return false;
            }

            return GetInternal(instance, member, out value);
        }

        public static bool Set([AllowNull] object instance, Type type, string name, object value)
        {
            var member = GetFieldOrProperty(type, instance != null, name);
            if (member == null)
            {
                return false;
            }

            return SetInternal(instance, member, value);
        }

        public static bool Call([AllowNull] object instance, Type type, string name, object[] arguments,
            [AllowNull] out object result)
        {
            var types = GetTypes(arguments);
            if (GetMethod(type, instance != null, name, types) is MethodInfo method)
            {
                result = method.Invoke(instance, arguments);
                return true;
            }

            result = null;
            return false;
        }

        public static bool GetElement([AllowNull] object instance, Type type, object[] index,
            [AllowNull] out object value)
        {
            var property = GetProperty(type, instance != null, "Item", GetTypes(index));
            if (property != null && property.CanRead)
            {
                value = property.GetValue(instance, index);
                return true;
            }

            value = null;
            return false;
        }

        public static bool SetElement([AllowNull] object instance, Type type, object[] index, [AllowNull] object value)
        {
            var property = GetProperty(type, instance != null, "Item", GetTypes(index));
            if (property != null && property.CanWrite)
            {
                property.SetValue(instance, value, index);
                return true;
            }

            return false;
        }

        [return: AllowNull]
        public static MemberInfo GetFieldOrProperty(Type type, bool instance, string name)
        {
            return GetFieldOrPropertyCache.GetOrAdd((type, instance, name), (input) =>
            {
                return GetFieldOrPropertyInternal(input.Item1, input.Item2, input.Item3);
            });
        }

        [return: AllowNull]
        public static FieldInfo GetField(Type type, bool instance, string name)
        {
            return FieldCache.GetOrAdd((type, instance, name), (input) =>
            {
                return GetFieldInternal(input.Item1, input.Item2, input.Item3);
            });
        }

        [return: AllowNull]
        public static PropertyInfo GetProperty(Type type, bool instance, string name, Type[] types = null)
        {
            return PropertyCache.GetOrAdd((type, instance, name, new ArrayHash<Type>(types ?? Array.Empty<Type>())),
                (input) =>
                {
                    return GetPropertyInternal(input.Item1, input.Item2, input.Item3, input.Item4.Array);
                });
        }

        [return: AllowNull]
        public static MethodInfo GetMethod(Type type, bool instance, string name, Type[] types)
        {
            return MethodCache.GetOrAdd((type, instance, name, new ArrayHash<Type>(types)), (input) =>
            {
                return GetMethodInternal(input.Item1, input.Item2, input.Item3, input.Item4.Array);
            });
        }

        [return: AllowNull]
        public static FieldInfo GetBackingField(PropertyInfo property, bool instance)
        {
            return BackingFieldCache.GetOrAdd((property, instance), (input) =>
            {
                return GetBackingFieldInternal(input.Item1, input.Item2);
            });
        }

        [return: AllowNull]
        public static Type GetImplementation(Type type, Type generic)
        {
            if (type == generic)
            {
                return type;
            }

            return ImplementationCache.GetOrAdd((type, generic), (input) =>
            {
                return GetImplementationInternal(input.Item1, input.Item2);
            });
        }

        static bool GetInternal([AllowNull] object instance, MemberInfo member, [AllowNull] out object value)
        {
            if (member is FieldInfo field)
            {
                value = field.GetValue(instance);
            }
            else if (member is PropertyInfo property && property.CanRead)
            {
                value = property.GetValue(instance);
            }
            else
            {
                value = null;
                return false;
            }

            return true;
        }

        static bool SetInternal([AllowNull] object instance, MemberInfo member, object value)
        {
            if (member is FieldInfo field)
            {
                field.SetValue(instance, value);
            }
            else if (member is PropertyInfo property)
            {
                if (property.CanWrite)
                {
                    property.SetValue(instance, value);
                }
                else
                {
                    GetBackingField(property, instance != null).SetValue(instance, value);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        [return: AllowNull]
        static MemberInfo GetFieldOrPropertyInternal(Type type, bool instance, string name)
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
        static FieldInfo GetBackingFieldInternal(PropertyInfo property, bool instance)
        {
            if (property.GetGetMethod(true).IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                var name = $"{BackingFieldPrefix}{property.Name}{BackingFieldSuffix}";
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

        static bool IsBackingField(this FieldInfo field)
        {
            return field.IsDefined(typeof(CompilerGeneratedAttribute)) &&
                   field.Name.StartsWith(BackingFieldPrefix) &&
                   field.Name.EndsWith(BackingFieldSuffix);
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

        static Type[] GetTypes(object[] objects)
        {
            var types = new Type[objects.Length];
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = objects[i]?.GetType();
            }

            return types;
        }
    }
}
