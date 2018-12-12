using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;
using Source.Utility;

namespace Atko.Mirra.Images
{
    public class TypeImage
    {
        static class StaticCache<T>
        {
            public static TypeImage Instance { get; } = Get(typeof(T));
        }

        public static implicit operator Type([AllowNull] TypeImage image)
        {
            return image == null ? null : image.Type;
        }

        public static implicit operator TypeImage([AllowNull] Type type)
        {
            return type == null ? null : Get(type);
        }

        public static TypeImage Get(Type type)
        {
            return Cache.GetOrAdd(type, (input) => new TypeImage(input));
        }

        public static TypeImage Get<T>()
        {
            return StaticCache<T>.Instance;
        }

        static Cache<Type, TypeImage> Cache { get; } = new Cache<Type, TypeImage>();

        static ArrayHash<Type> HashTypes(ParameterInfo[] parameters)
        {
            return new ArrayHash<Type>(parameters.Select((current) => current.ParameterType).ToArray());
        }

        public Type Type { get; }

        [AllowNull]
        public TypeImage Base { get; }

        public IEnumerable<TypeImage> Inheritance
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

        public IEnumerable<TypeImage> Interfaces => LazyInterfaces.Value;
        public IEnumerable<ConstructorImage> Constructors => LazyConstructors.Value;

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

        public TypeImage GenericDefinition =>
            IsGeneric && !IsGenericDefinition
                ? Get(Type.GetGenericTypeDefinition())
                : this;

        TypeImage[] GenericArguments { get; }

        Cache<Type, Type> AssignableTypeCache { get; } = new Cache<Type, Type>();

        Lazy<TypeImage[]> LazyInterfaces { get; }
        Lazy<ConstructorImage[]> LazyConstructors { get; }

        Lazy<MethodImage[]> LazyLocalMethods { get; }
        Lazy<PropertyImage[]> LazyLocalProperties { get; }
        Lazy<FieldImage[]> LazyLocalFields { get; }
        Lazy<IndexerImage[]> LazyLocalIndexers { get; }

        Lazy<MethodImage[]> LazySurfaceMethods { get; }
        Lazy<PropertyImage[]> LazySurfaceProperties { get; }
        Lazy<FieldImage[]> LazySurfaceFields { get; }
        Lazy<IndexerImage[]> LazySurfaceIndexers { get; }

        Lazy<Dictionary<ArrayHash<Type>, ConstructorImage>> LazyConstructorMap { get; }
        Lazy<Dictionary<Pair<string, ArrayHash<Type>>, MethodImage>> LazyMethodMap { get; }
        Lazy<Dictionary<string, PropertyImage>> LazyPropertyMap { get; }
        Lazy<Dictionary<string, FieldImage>> LazyFieldMap { get; }
        Lazy<Dictionary<ArrayHash<Type>, IndexerImage>> LazyIndexerMap { get; }

        TypeImage(Type type)
        {
            Type = type;
            Base = type.BaseType == null ? null : Get(type.BaseType);

            GenericArguments = IsGeneric
                ? Type.GetGenericArguments().Select(Get).ToArray()
                : ArrayUtility<TypeImage>.Empty;

            LazyInterfaces = new Lazy<TypeImage[]>(GetInterfaces);
            LazyConstructors = new Lazy<ConstructorImage[]>(GetConstructors);

            LazyLocalMethods = new Lazy<MethodImage[]>(GetLocalMethods);
            LazyLocalProperties = new Lazy<PropertyImage[]>(GetLocalProperties);
            LazyLocalFields = new Lazy<FieldImage[]>(GetLocalFields);
            LazyLocalIndexers = new Lazy<IndexerImage[]>(GetLocalIndexers);

            LazySurfaceMethods = new Lazy<MethodImage[]>(() => GetSurfaceMembers(Methods(MemberQuery.All)));
            LazySurfaceProperties = new Lazy<PropertyImage[]>(() => GetSurfaceMembers(Properties(MemberQuery.All)));
            LazySurfaceFields = new Lazy<FieldImage[]>(() => GetSurfaceMembers(Fields(MemberQuery.All)));
            LazySurfaceIndexers = new Lazy<IndexerImage[]>(() => GetSurfaceMembers(Indexers(MemberQuery.All)));

            LazyConstructorMap = new Lazy<Dictionary<ArrayHash<Type>, ConstructorImage>>(() =>
                Constructors
                    .ToDictionaryByFirst((constructor) => HashTypes(constructor.Constructor.GetParameters())));

            LazyMethodMap = new Lazy<Dictionary<Pair<string, ArrayHash<Type>>, MethodImage>>(() =>
                Methods(MemberQuery.All)
                    .ToDictionaryByFirst((current) =>
                        new Pair<string, ArrayHash<Type>>(current.Name, HashTypes(current.Method.GetParameters()))));

            LazyPropertyMap = new Lazy<Dictionary<string, PropertyImage>>(() =>
                Properties(MemberQuery.All)
                    .ToDictionaryByFirst((current) => current.Name));

            LazyFieldMap = new Lazy<Dictionary<string, FieldImage>>(() =>
                Fields(MemberQuery.All)
                    .ToDictionaryByFirst((current) => current.Name));

            LazyIndexerMap = new Lazy<Dictionary<ArrayHash<Type>, IndexerImage>>(() =>
                Indexers(MemberQuery.All)
                    .ToDictionaryByFirst((current) => HashTypes(current.Property.GetIndexParameters())));
        }

        public override string ToString()
        {
            return $"{nameof(TypeImage)}({Type})";
        }

        public bool IsAssignableTo(Type target)
        {
            return AssignableType(target) != null;
        }

        [return: AllowNull]
        public Type AssignableType(Type target)
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

            foreach (var image in Inheritance.Concat(Interfaces))
            {
                var definition = image.Type.IsGenericType
                    ? image.Type.GetGenericTypeDefinition()
                    : image.Type;

                if (definition == target)
                {
                    return AssignableTypeCache[target] = image.Type;
                }
            }

            return AssignableTypeCache[target] = null;
        }

        public TypeImage GenericArgument(int index)
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
        public ConstructorImage Constructor(params Type[] types)
        {
            var key = new ArrayHash<Type>(types);
            LazyConstructorMap.Value.TryGetValue(key, out var image);
            return image;
        }

        [return: AllowNull]
        public MethodImage Method(string name, params Type[] types)
        {
            var key = new Pair<string, ArrayHash<Type>>(name, new ArrayHash<Type>(types.CopyArray()));
            LazyMethodMap.Value.TryGetValue(key, out var image);
            return image;
        }

        [return: AllowNull]
        public PropertyImage Property(string name)
        {
            LazyPropertyMap.Value.TryGetValue(name, out var image);
            return image;
        }

        [return: AllowNull]
        public FieldImage Field(string name)
        {
            LazyFieldMap.Value.TryGetValue(name, out var image);
            return image;
        }

        [return: AllowNull]
        public AccessorImage Accessor(string name)
        {
            LazyPropertyMap.Value.TryGetValue(name, out var property);
            if (property != null)
            {
                return property;
            }

            LazyFieldMap.Value.TryGetValue(name, out var field);
            return field;
        }

        public IEnumerable<MethodImage> Methods(MemberQuery query = default(MemberQuery))
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

            return Enumerable.Empty<MethodImage>();
        }

        public IEnumerable<FieldImage> Fields(MemberQuery query = default(MemberQuery))
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

            return Enumerable.Empty<FieldImage>();
        }

        public IEnumerable<PropertyImage> Properties(MemberQuery query = default(MemberQuery))
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

            return Enumerable.Empty<PropertyImage>();
        }

        public IEnumerable<AccessorImage> Accessors(MemberQuery query = default(MemberQuery))
        {
            foreach (var image in Properties(query))
            {
                yield return image;
            }

            foreach (var image in Fields(query))
            {
                yield return image;
            }
        }

        public IEnumerable<IndexerImage> Indexers(MemberQuery query = default(MemberQuery))
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

            return Enumerable.Empty<IndexerImage>();
        }

        TypeImage[] GetInterfaces()
        {
            return Type
                .Inheritance()
                .SelectMany((current) => current.GetInterfaces())
                .Select(Get)
                .ToArray();
        }

        ConstructorImage[] GetConstructors()
        {
            return Type
                .GetConstructors(TypeUtility.InstanceBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new ConstructorImage(Type, current))
                .ToArray();
        }

        MethodImage[] GetLocalMethods()
        {
            var images = new List<MethodImage>();
            var instanceMembers = Type
                .GetMethods(TypeUtility.InstanceBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(Type, current));

            var staticMembers = Type
                .GetMethods(TypeUtility.StaticBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(Type, current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetMethods(TypeUtility.InstanceBinding))
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(Type, current));

            images.AddRange(instanceMembers);
            images.AddRange(interfaceMembers);
            images.AddRange(staticMembers);

            return images.ToArray();
        }

        PropertyImage[] GetLocalProperties()
        {
            var images = new List<PropertyImage>();
            var instanceMembers = Type
                .GetProperties(TypeUtility.InstanceBinding)
                .Where(PropertyImage.CanCreateFrom)
                .Select((current) => new PropertyImage(Type, current));

            var staticMembers = Type
                .GetProperties(TypeUtility.StaticBinding)
                .Where(PropertyImage.CanCreateFrom)
                .Select((current) => new PropertyImage(Type, current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                .Where(PropertyImage.CanCreateFrom)
                .Select((current) => new PropertyImage(Type, current));

            images.AddRange(instanceMembers);
            images.AddRange(interfaceMembers);
            images.AddRange(staticMembers);

            return images.ToArray();
        }

        FieldImage[] GetLocalFields()
        {
            var images = new List<FieldImage>();
            var instanceMembers = Type
                .GetFields(TypeUtility.InstanceBinding)
                .Select((current) => FieldImage.Create(Type, current));

            var staticMembers = Type
                .GetFields(TypeUtility.StaticBinding)
                .Select((current) => FieldImage.Create(Type, current));

            images.AddRange(instanceMembers);
            images.AddRange(staticMembers);

            return images.ToArray();
        }

        IndexerImage[] GetLocalIndexers()
        {
            var images = new List<IndexerImage>();
            var instanceMembers = Type
                .GetProperties(TypeUtility.InstanceBinding)
                .Where(IndexerImage.CanCreateFrom)
                .Select((current) => new IndexerImage(Type, current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                .Where(IndexerImage.CanCreateFrom)
                .Select((current) => new IndexerImage(Type, current));

            images.AddRange(instanceMembers);
            images.AddRange(interfaceMembers);

            return images.ToArray();
        }

        T[] GetSurfaceMembers<T>(IEnumerable<T> images) where T : MemberImage
        {
            var seen = new HashSet<string>();
            var unique = new List<T>();
            foreach (var image in images)
            {
                if (seen.Add(image.Name))
                {
                    unique.Add(image);
                }
            }

            return unique.ToArray();
        }
    }
}