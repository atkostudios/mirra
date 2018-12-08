using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NullGuard;
using Utility;

namespace Ducktype.Models
{
    public class TypeModel
    {
        static class StaticTypeAccessorCache<T>
        {
            public static TypeModel Instance { get; } = Get(typeof(T));
        }

        public static TypeModel Get(Type type)
        {
            return TypeAccessorCache.GetOrAdd(type, (input) => new TypeModel(input));
        }

        public static TypeModel Get<T>()
        {
            return StaticTypeAccessorCache<T>.Instance;
        }

        static Cache<Type, TypeModel> TypeAccessorCache { get; } = new Cache<Type, TypeModel>();

        public Type Type { get; }

        public IEnumerable<ConstructorModel> Constructors => ConstructorArray.Iterate();
        public IEnumerable<MethodModel> Methods => MethodArray.Iterate();
        public IEnumerable<PropertyModel> Properties => PropertyArray.Iterate();
        public IEnumerable<FieldModel> Fields => FieldArray.Iterate();
        public IEnumerable<AccessorModel> Accessors => AccessorArray.Iterate();

        ConstructorModel[] ConstructorArray { get; }
        MethodModel[] MethodArray { get; }
        PropertyModel[] PropertyArray { get; }
        FieldModel[] FieldArray { get; }
        AccessorModel[] AccessorArray { get; }

        Dictionary<ConstructorInfo, ConstructorModel> ConstructorMap { get; } =
            new Dictionary<ConstructorInfo, ConstructorModel>();
        Dictionary<MethodInfo, MethodModel> MethodMap { get; } = new Dictionary<MethodInfo, MethodModel>();
        Dictionary<string, PropertyModel> PropertyMap { get; } = new Dictionary<string, PropertyModel>();
        Dictionary<string, FieldModel> FieldMap { get; } = new Dictionary<string, FieldModel>();
        Dictionary<string, AccessorModel> AccessorMap { get; } = new Dictionary<string, AccessorModel>();

        TypeModel(Type type)
        {
            Type = type;

            ConstructorArray = GetConstructors(Type);
            MethodArray = GetMethods(Type);
            PropertyArray = GetProperties(Type);
            FieldArray = GetFields(Type);

            AccessorArray = PropertyArray.Cast<AccessorModel>().Concat(FieldArray).ToArray();

            foreach (var accessor in ConstructorArray)
            {
                ConstructorMap[accessor.Constructor] = accessor;
            }

            foreach (var accessor in MethodArray)
            {
                MethodMap[accessor.Method] = accessor;
            }

            foreach (var accessor in FieldArray)
            {
                FieldMap[accessor.Name] = accessor;
            }

            foreach (var accessor in PropertyArray)
            {
                PropertyMap[accessor.Name] = accessor;
            }

            foreach (var accessor in FieldArray.Cast<AccessorModel>().Concat(PropertyArray))
            {
                AccessorMap[accessor.Name] = accessor;
            }
        }

        [return: AllowNull]
        public ConstructorModel GetConstructor(params Type[] types)
        {
            var constructor = TypeUtility.GetConstructor(Type, types);
            if (constructor == null)
            {
                return null;
            }

            ConstructorMap.TryGetValue(constructor, out var model);
            return model;
        }

        public MethodModel GetMethod(string name, params Type[] types)
        {
            var method = TypeUtility.GetMethod(Type, true, name, types) ??
                         TypeUtility.GetMethod(Type, false, name, types);
            if (method == null)
            {
                return null;
            }

            MethodMap.TryGetValue(method, out var model);
            return model;
        }

        [return: AllowNull]
        public PropertyModel GetProperty(string name)
        {
            PropertyMap.TryGetValue(name, out var accessor);
            return accessor;
        }

        [return: AllowNull]
        public FieldModel GetField(string name)
        {
            FieldMap.TryGetValue(name, out var accessor);
            return accessor;
        }

        [return: AllowNull]
        public AccessorModel GetAccessor(string name)
        {
            AccessorMap.TryGetValue(name, out var accessor);
            return accessor;
        }

        public ConstructorModel Constructor(params Type[] types)
        {
            return GetConstructor(types) ?? throw new DucktypeMissingMemberException();
        }

        public MethodModel Method(string name, params Type[] types)
        {
            return GetMethod(name, types) ?? throw new DucktypeMissingMemberException();
        }

        public PropertyModel Property(string name)
        {
            return GetProperty(name) ?? throw new DucktypeMissingMemberException();
        }

        public FieldModel Field(string name)
        {
            return GetField(name) ?? throw new DucktypeMissingMemberException();
        }

        public AccessorModel Accessor(string name)
        {
            return GetAccessor(name) ?? throw new DucktypeMissingMemberException();
        }

        ConstructorModel[] GetConstructors(Type type)
        {
            return type
                .GetConstructors(TypeUtility.InstanceBinding)
                .Select((current) => ConstructorModel.Create(type, current))
                .Where((current) => current != null)
                .ToArray();
        }

        MethodModel[] GetMethods(Type type)
        {
            var accessors = new List<MethodModel>();
            foreach (var ancestor in type.Inheritance().Reverse())
            {
                var instanceMembers = ancestor
                    .GetMethods(TypeUtility.InstanceBinding)
                    .Select((current) => MethodModel.Create(type, current))
                    .Where((current) => current != null);

                var staticMembers = ancestor
                    .GetMethods(TypeUtility.StaticBinding)
                    .Select((current) => MethodModel.Create(type, current))
                    .Where((current) => current != null);

                var interfaceMembers = ancestor.GetInterfaces()
                    .SelectMany((current) => current.GetMethods(TypeUtility.InstanceBinding))
                    .Select((current) => MethodModel.Create(type, current))
                    .Where((current) => current != null);

                accessors.AddRange(instanceMembers);
                accessors.AddRange(interfaceMembers);
                accessors.AddRange(staticMembers);
            }

            return GetUnique(accessors);
        }

        PropertyModel[] GetProperties(Type type)
        {
            var accessors = new List<PropertyModel>();
            foreach (var ancestor in type.Inheritance().Reverse())
            {
                var instanceMembers = ancestor
                    .GetProperties(TypeUtility.InstanceBinding)
                    .Select((current) => PropertyModel.Create(type, current))
                    .Where((current) => current != null);

                var staticMembers = ancestor
                    .GetProperties(TypeUtility.StaticBinding)
                    .Select((current) => PropertyModel.Create(type, current))
                    .Where((current) => current != null);

                var interfaceMembers = ancestor.GetInterfaces()
                    .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                    .Select((current) => PropertyModel.Create(type, current))
                    .Where((current) => current != null);

                accessors.AddRange(instanceMembers);
                accessors.AddRange(interfaceMembers);
                accessors.AddRange(staticMembers);
            }

            return GetUnique(accessors);
        }

        FieldModel[] GetFields(Type type)
        {
            var accessors = new List<FieldModel>();
            foreach (var ancestor in type.Inheritance())
            {
                var instanceMembers = ancestor
                    .GetFields(TypeUtility.InstanceBinding)
                    .Select((current) => FieldModel.Create(type, current));

                var staticMembers = ancestor
                    .GetFields(TypeUtility.StaticBinding)
                    .Select((current) => FieldModel.Create(type, current));

                accessors.AddRange(instanceMembers);
                accessors.AddRange(staticMembers);
            }

            return GetUnique(accessors);
        }

        T[] GetUnique<T>(IEnumerable<T> accessors) where T : MemberModel
        {
            var seen = new HashSet<string>();
            var unique = new List<T>();
            foreach (var accessor in accessors.Reverse())
            {
                if (seen.Add(accessor.Name))
                {
                    unique.Add(accessor);
                }
            }

            var array = unique.ToArray();
            Array.Reverse(array);
            return array;
        }
    }
}
