using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NullGuard;
using Utility;

namespace Ducktype
{
    public class TypeAccessor<T>
    {
        public static TypeAccessor Instance { get; } = TypeAccessor.Get(typeof(T));
    }

    public class TypeAccessor
    {
        public static TypeAccessor Get(Type type)
        {
            return Accessors.GetOrAdd(type, (input) => new TypeAccessor(input));
        }

        static ConcurrentDictionary<Type, TypeAccessor> Accessors { get; }

        public Type Type { get; }

        public IEnumerable<MethodModel> Methods => MethodContainer.Select((current) => current);
        public IEnumerable<PropertyModel> Properties => PropertyContainer.Select((current) => current);
        public IEnumerable<FieldModel> Fields => FieldContainer.Select((current) => current);
        public IEnumerable<KeyModel> Keys => KeyContainer.Select((current) => current);

        MethodModel[] MethodContainer { get; }
        PropertyModel[] PropertyContainer { get; }
        FieldModel[] FieldContainer { get; }
        KeyModel[] KeyContainer { get; }

        TypeAccessor(Type type)
        {
            Type = type;
            MethodContainer = ReflectionUtility.GetMethods(Type);
            PropertyContainer = ReflectionUtility.GetProperties(Type);
            FieldContainer = ReflectionUtility.GetFields(Type);
            KeyContainer = PropertyContainer.Cast<KeyModel>().Concat(FieldContainer).ToArray();
        }
    }
}