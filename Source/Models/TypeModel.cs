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
        static class StaticCache<T>
        {
            public static TypeModel Instance { get; } = Get(typeof(T));
        }

        public static implicit operator Type([AllowNull] TypeModel model)
        {
            return model == null ? null : model.Type;
        }

        public static implicit operator TypeModel([AllowNull] Type type)
        {
            return type == null ? null : Get(type);
        }

        public static TypeModel Get(Type type)
        {
            return Cache.GetOrAdd(type, (input) => new TypeModel(input));
        }

        public static TypeModel Get<T>()
        {
            return StaticCache<T>.Instance;
        }

        static Cache<Type, TypeModel> Cache { get; } = new Cache<Type, TypeModel>();

        static ArrayHash<Type> HashTypes(ParameterInfo[] parameters)
        {
            return parameters.Select((current) => current.ParameterType).ToArray();
        }

        public Type Type { get; }

        [AllowNull]
        public TypeModel Base { get; }

        public IEnumerable<TypeModel> Inheritance
        {
            get
            {
                var current = this;
                while (current != null)
                {
                    yield return current;
                    current = current.Base;
                }
            }
        }

        public string Name => Type.Name;

        public IEnumerable<TypeModel> Interfaces => LazyInterfaces.Value;
        public IEnumerable<ConstructorModel> Constructors => LazyConstructors.Value;


        public bool IsGeneric => Type.IsGenericType;
        public bool IsGenericDefinition => Type.IsGenericTypeDefinition;
        public bool IsArray => Type.IsArray;
        public bool IsEnum => Type.IsEnum;
        public bool IsClass => Type.IsClass;
        public bool IsInterface => Type.IsInterface;
        public bool IsStruct => Type.IsValueType;
        public bool IsPrimitive => Type.IsPrimitive;
        public bool IsPublic => Type.IsPublic;
        public bool IsSealed => Type.IsSealed;
        public bool IsAbstract => Type.IsAbstract;

        public int GenericArgumentCount => GenericArguments.Length;

        public TypeModel GenericDefinition =>
            IsGeneric && !IsGenericDefinition
                ? Get(Type.GetGenericTypeDefinition())
                : this;

        TypeModel[] GenericArguments { get; }

        Cache<Type, Type> AssignableTypeCache { get; } = new Cache<Type, Type>();

        Lazy<TypeModel[]> LazyInterfaces { get; }
        Lazy<ConstructorModel[]> LazyConstructors { get; }

        Lazy<MethodModel[]> LazyLocalMethods { get; }
        Lazy<PropertyModel[]> LazyLocalProperties { get; }
        Lazy<FieldModel[]> LazyLocalFields { get; }
        Lazy<AccessorModel[]> LazyLocalAccessors { get; }
        Lazy<IndexerModel[]> LazyLocalIndexers { get; }

        Lazy<MethodModel[]> LazySurfaceMethods { get; }
        Lazy<PropertyModel[]> LazySurfaceProperties { get; }
        Lazy<FieldModel[]> LazySurfaceFields { get; }
        Lazy<IndexerModel[]> LazySurfaceIndexers { get; }

        Lazy<Dictionary<ArrayHash<Type>, ConstructorModel>> LazyConstructorMap { get; }
        Lazy<Dictionary<KeyValuePair<string, ArrayHash<Type>>, MethodModel>> LazyMethodMap { get; }
        Lazy<Dictionary<string, PropertyModel>> LazyPropertyMap { get; }
        Lazy<Dictionary<string, FieldModel>> LazyFieldMap { get; }
        Lazy<Dictionary<ArrayHash<Type>, IndexerModel>> LazyIndexerMap { get; }

        TypeModel(Type type)
        {
            Type = type;

            Base = type.BaseType == null ? null : Get(type.BaseType);

            GenericArguments = IsGeneric
                ? Type.GetGenericArguments().Select(Get).ToArray()
                : ArrayUtility<TypeModel>.Empty;

            LazyInterfaces = new Lazy<TypeModel[]>(() =>
                GetInterfaces(Type));

            LazyConstructors = new Lazy<ConstructorModel[]>(() =>
                GetConstructors(Type));

            LazyLocalMethods = new Lazy<MethodModel[]>(() =>
                GetMethods(Type, true));

            LazyLocalProperties = new Lazy<PropertyModel[]>(() =>
                GetProperties(Type, true));

            LazyLocalFields = new Lazy<FieldModel[]>(() =>
                GetFields(Type, true));

            LazyLocalIndexers = new Lazy<IndexerModel[]>(() =>
                GetIndexers(Type, true));

            LazyLocalAccessors = new Lazy<AccessorModel[]>(() =>
                LazyLocalProperties.Value
                    .Cast<AccessorModel>()
                    .Concat(LazyLocalFields.Value)
                    .ToArray());

            LazyLocalIndexers = new Lazy<IndexerModel[]>(() =>
                GetIndexers(Type, true));

            LazySurfaceMethods = new Lazy<MethodModel[]>(() =>
                GetSurfaceMembers(GetMethods(Type, false)));

            LazySurfaceProperties = new Lazy<PropertyModel[]>(() =>
                GetSurfaceMembers(GetProperties(Type, false)));

            LazySurfaceFields = new Lazy<FieldModel[]>(() =>
                GetSurfaceMembers(GetFields(Type, false)));

            LazySurfaceIndexers = new Lazy<IndexerModel[]>(() =>
                GetSurfaceMembers(GetIndexers(Type, false)));

            LazyConstructorMap = new Lazy<Dictionary<ArrayHash<Type>, ConstructorModel>>(() =>
                Constructors
                    .ToDictionaryByFirst((constructor) => HashTypes(constructor.Constructor.GetParameters())));

            LazyMethodMap = new Lazy<Dictionary<KeyValuePair<string, ArrayHash<Type>>, MethodModel>>(() =>
                Methods(MemberQuery.All)
                    .ToDictionaryByFirst((current) =>
                        new KeyValuePair<string, ArrayHash<Type>>(
                            current.Name,
                            HashTypes(current.Method.GetParameters()))));

            LazyPropertyMap = new Lazy<Dictionary<string, PropertyModel>>(() =>
                Properties(MemberQuery.All)
                    .ToDictionaryByFirst((current) => current.Name));

            LazyFieldMap = new Lazy<Dictionary<string, FieldModel>>(() =>
                Fields(MemberQuery.All)
                    .ToDictionaryByFirst((current) => current.Name));

            LazyIndexerMap = new Lazy<Dictionary<ArrayHash<Type>, IndexerModel>>(() =>
                Indexers(MemberQuery.All)
                    .ToDictionaryByFirst((current) => HashTypes(current.Property.GetIndexParameters())));
        }

        public override string ToString()
        {
            return $"{nameof(TypeModel)}({Type})";
        }

        public bool IsAssignableTo(Type target)
        {
            return GetAssignableType(target) != null;
        }

        [return: AllowNull]
        public Type GetAssignableType(Type target)
        {
            if (Type == target)
            {
                return Type;
            }

            if (AssignableTypeCache.TryGetValue(target, out var cached))
            {
                return cached;
            }

            target = target.IsGenericType
                ? target.GetGenericTypeDefinition()
                : target;

            foreach (var model in Inheritance.Concat(Interfaces))
            {
                var definition = model.Type.IsGenericType
                    ? model.Type.GetGenericTypeDefinition()
                    : model.Type;

                if (definition == target)
                {
                    return AssignableTypeCache[target] = model.Type;
                }
            }

            return AssignableTypeCache[target] = null;
        }

        public TypeModel GenericArgument(int index)
        {
            try
            {
                return GenericArguments[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
        }

        [return: AllowNull]
        public ConstructorModel GetConstructor(params Type[] types)
        {
            LazyConstructorMap.Value.TryGetValue(types, out var model);
            return model;
        }

        [return: AllowNull]
        public MethodModel GetMethod(string name, params Type[] types)
        {
            LazyMethodMap.Value.TryGetValue(new KeyValuePair<string, ArrayHash<Type>>(name, types), out var model);
            return model;
        }

        [return: AllowNull]
        public PropertyModel GetProperty(string name)
        {
            LazyPropertyMap.Value.TryGetValue(name, out var model);
            return model;
        }

        [return: AllowNull]
        public FieldModel GetField(string name)
        {
            LazyFieldMap.Value.TryGetValue(name, out var model);
            return model;
        }

        [return: AllowNull]
        public AccessorModel GetAccessor(string name)
        {
            LazyPropertyMap.Value.TryGetValue(name, out var property);
            if (property != null)
            {
                return property;
            }

            LazyFieldMap.Value.TryGetValue(name, out var field);
            return field;
        }

        public IndexerModel GetIndexer(params Type[] types)
        {
            LazyIndexerMap.Value.TryGetValue(types, out var model);
            return model;
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

        public IEnumerable<MethodModel> Methods(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceMethods.Value;
                case MemberQuery.Local:
                    return LazyLocalMethods.Value;
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalMethods.Value);
            }

            return Enumerable.Empty<MethodModel>();
        }

        public IEnumerable<FieldModel> Fields(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceFields.Value;
                case MemberQuery.Local:
                    return LazyLocalFields.Value;
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalFields.Value);
            }

            return Enumerable.Empty<FieldModel>();
        }

        public IEnumerable<PropertyModel> Properties(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceProperties.Value;
                case MemberQuery.Local:
                    return LazyLocalProperties.Value;
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalProperties.Value);
            }

            return Enumerable.Empty<PropertyModel>();
        }

        public IEnumerable<AccessorModel> Accessors(MemberQuery query = default(MemberQuery))
        {
            foreach (var model in Properties(query))
            {
                yield return model;
            }

            foreach (var model in Fields(query))
            {
                yield return model;
            }
        }

        public IEnumerable<IndexerModel> Indexers(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceIndexers.Value;
                case MemberQuery.Local:
                    return LazyLocalIndexers.Value;
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalIndexers.Value);
            }

            return Enumerable.Empty<IndexerModel>();
        }

        TypeModel[] GetInterfaces(Type type)
        {
            return type
                .Inheritance()
                .SelectMany((current) => current.GetInterfaces())
                .Select(Get)
                .ToArray();
        }

        ConstructorModel[] GetConstructors(Type type)
        {
            return type
                .GetConstructors(TypeUtility.InstanceBinding)
                .Where(CallableModel.CanCreateFrom)
                .Select((current) => new ConstructorModel(type, current))
                .ToArray();
        }

        MethodModel[] GetMethods(Type type, bool local)
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

                if (local)
                {
                    break;
                }
            }

            return models.ToArray();
        }

        PropertyModel[] GetProperties(Type type, bool local)
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

                if (local)
                {
                    break;
                }
            }

            return models.ToArray();
        }

        FieldModel[] GetFields(Type type, bool local)
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

                if (local)
                {
                    break;
                }
            }

            return models.ToArray();
        }

        IndexerModel[] GetIndexers(Type type, bool local)
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

                if (local)
                {
                    break;
                }
            }

            return models.ToArray();
        }

        T[] GetSurfaceMembers<T>(IEnumerable<T> models) where T : MemberModel
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