using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atko.Dodge.Utility;
using NullGuard;

namespace Atko.Dodge.Models
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
        public IEnumerable<IndexerModel> Indexers => IndexerArray.Iterate();

        ConstructorModel[] ConstructorArray { get; }
        MethodModel[] MethodArray { get; }
        PropertyModel[] PropertyArray { get; }
        FieldModel[] FieldArray { get; }
        AccessorModel[] AccessorArray { get; }
        IndexerModel[] IndexerArray { get; }

        Dictionary<ConstructorInfo, ConstructorModel> ConstructorMap { get; } =
            new Dictionary<ConstructorInfo, ConstructorModel>();

        Dictionary<MethodInfo, MethodModel> MethodMap { get; } = new Dictionary<MethodInfo, MethodModel>();
        Dictionary<string, PropertyModel> PropertyMap { get; } = new Dictionary<string, PropertyModel>();
        Dictionary<string, FieldModel> FieldMap { get; } = new Dictionary<string, FieldModel>();
        Dictionary<string, AccessorModel> AccessorMap { get; } = new Dictionary<string, AccessorModel>();
        Dictionary<PropertyInfo, IndexerModel> IndexerMap { get; } = new Dictionary<PropertyInfo, IndexerModel>();

        TypeModel(Type type)
        {
            ConstructorArray = GetConstructors(Type);
            MethodArray = GetMethods(Type);
            PropertyArray = GetProperties(Type);
            FieldArray = GetFields(Type);
            AccessorArray = PropertyArray.Cast<AccessorModel>().Concat(FieldArray).ToArray();
            IndexerArray = GetIndexers(Type);

            foreach (var model in ConstructorArray)
            {
                ConstructorMap[model.Constructor] = model;
            }

            foreach (var model in MethodArray)
            {
                MethodMap[model.Method] = model;
            }

            foreach (var model in FieldArray)
            {
                FieldMap[model.Name] = model;
            }

            foreach (var model in PropertyArray)
            {
                PropertyMap[model.Name] = model;
            }

            foreach (var model in FieldArray.Cast<AccessorModel>().Concat(PropertyArray))
            {
                AccessorMap[model.Name] = model;
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

        [return: AllowNull]
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
            PropertyMap.TryGetValue(name, out var model);
            return model;
        }

        [return: AllowNull]
        public FieldModel GetField(string name)
        {
            FieldMap.TryGetValue(name, out var model);
            return model;
        }

        [return: AllowNull]
        public AccessorModel GetAccessor(string name)
        {
            AccessorMap.TryGetValue(name, out var model);
            return model;
        }

        public IndexerModel GetIndexer(params Type[] types)
        {
            var property = TypeUtility.GetProperty(Type, true, "Item", types);
            if (property == null)
            {
                return null;
            }

            IndexerMap.TryGetValue(property, out var indexer);
            return indexer;
        }

        public ConstructorModel Constructor(params Type[] types)
        {
            return GetConstructor(types) ?? throw new DodgeMissingMemberException();
        }

        public MethodModel Method(string name, params Type[] types)
        {
            return GetMethod(name, types) ?? throw new DodgeMissingMemberException();
        }

        public PropertyModel Property(string name)
        {
            return GetProperty(name) ?? throw new DodgeMissingMemberException();
        }

        public FieldModel Field(string name)
        {
            return GetField(name) ?? throw new DodgeMissingMemberException();
        }

        public AccessorModel Accessor(string name)
        {
            return GetAccessor(name) ?? throw new DodgeMissingMemberException();
        }

        public IndexerModel Indexer(params Type[] types)
        {
            return GetIndexer(types) ?? throw new DodgeMissingMemberException();
        }

        ConstructorModel[] GetConstructors(Type type)
        {
            return type
                .GetConstructors(TypeUtility.InstanceBinding)
                .Where(CallableModel.CanCreateFrom)
                .Select((current) => new ConstructorModel(type, current))
                .ToArray();
        }

        MethodModel[] GetMethods(Type type)
        {
            var models = new List<MethodModel>();
            foreach (var ancestor in type.Inheritance())
            {
                var instanceMembers = ancestor
                    .GetMethods(TypeUtility.InstanceBinding)
                    .Where(CallableModel.CanCreateFrom)
                    .Select((current) => new MethodModel(type, current));

                var staticMembers = ancestor
                    .GetMethods(TypeUtility.StaticBinding)
                    .Where(CallableModel.CanCreateFrom)
                    .Select((current) => new MethodModel(type, current));

                var interfaceMembers = ancestor
                    .GetInterfaces()
                    .SelectMany((current) => current.GetMethods(TypeUtility.InstanceBinding))
                    .Where(CallableModel.CanCreateFrom)
                    .Select((current) => new MethodModel(type, current));

                models.AddRange(instanceMembers);
                models.AddRange(interfaceMembers);
                models.AddRange(staticMembers);
            }

            return GetUnique(models);
        }

        PropertyModel[] GetProperties(Type type)
        {
            var models = new List<PropertyModel>();
            foreach (var ancestor in type.Inheritance())
            {
                var instanceMembers = ancestor
                    .GetProperties(TypeUtility.InstanceBinding)
                    .Where(PropertyModel.CanCreateFrom)
                    .Select((current) => new PropertyModel(type, current));

                var staticMembers = ancestor
                    .GetProperties(TypeUtility.StaticBinding)
                    .Where(PropertyModel.CanCreateFrom)
                    .Select((current) => new PropertyModel(type, current));

                var interfaceMembers = ancestor
                    .GetInterfaces()
                    .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                    .Where(PropertyModel.CanCreateFrom)
                    .Select((current) => new PropertyModel(type, current));

                models.AddRange(instanceMembers);
                models.AddRange(interfaceMembers);
                models.AddRange(staticMembers);
            }

            return GetUnique(models);
        }

        FieldModel[] GetFields(Type type)
        {
            var models = new List<FieldModel>();
            foreach (var ancestor in type.Inheritance())
            {
                var instanceMembers = ancestor
                    .GetFields(TypeUtility.InstanceBinding)
                    .Select((current) => FieldModel.Create(type, current));

                var staticMembers = ancestor
                    .GetFields(TypeUtility.StaticBinding)
                    .Select((current) => FieldModel.Create(type, current));

                models.AddRange(instanceMembers);
                models.AddRange(staticMembers);
            }

            return GetUnique(models);
        }

        IndexerModel[] GetIndexers(Type type)
        {
            var models = new List<IndexerModel>();
            foreach (var ancestor in type.Inheritance())
            {
                var instanceMembers = ancestor
                    .GetProperties(TypeUtility.InstanceBinding)
                    .Where(IndexerModel.CanCreateFrom)
                    .Select((current) => new IndexerModel(type, current));

                var interfaceMembers = ancestor
                    .GetInterfaces()
                    .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                    .Where(IndexerModel.CanCreateFrom)
                    .Select((current) => new IndexerModel(type, current));


                models.AddRange(instanceMembers);
                models.AddRange(interfaceMembers);
            }

            return GetUnique(models);
        }

        T[] GetUnique<T>(IEnumerable<T> models) where T : MemberModel
        {
            var seen = new HashSet<string>();
            var unique = new List<T>();
            foreach (var model in models)
            {
                if (seen.Add(model.Name))
                {
                    unique.Add(model);
                }
            }

            return unique.ToArray();
        }
    }
}