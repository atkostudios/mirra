using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="Type"/> that provides extended functionality and reflection performance.
    /// </summary>
    public class TypeImage : BaseImage
    {
        static class StaticCache<T>
        {
            public static TypeImage Instance { get; } = Get(typeof(T));
        }

        /// <summary>
        /// Iterate over every type from all assemblies in the current program.
        /// </summary>
        /// <returns>Every type from all assemblies in the current program.</returns>
        public static IEnumerable<TypeImage> All => LazyAll.Value.Iterate();

        static Lazy<TypeImage[]> LazyAll = new Lazy<TypeImage[]>(() => GetAllTypes().Select(Get).ToArray());

        static Lazy<Dictionary<Type, Type[]>> LazySubclassMap { get; } =
            new Lazy<Dictionary<Type, Type[]>>(GetSubclassMap);

        static Cache<Type, TypeImage> Cache { get; } = new Cache<Type, TypeImage>();

        /// <summary>
        /// Implicitly convert a <see cref="TypeImage"/> into its associated <see cref="Type"/>.
        /// </summary>
        /// <param name="image">The <see cref="TypeImage"/> to convert.</param>
        /// <returns>The associated <see cref="Type"/>.</returns>
        public static implicit operator Type([AllowNull] TypeImage image) => image == null ? null : image.Type;

        /// <summary>
        /// Implicitly convert a <see cref="Type"/> into its associated <see cref="TypeImage"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to convert.</param>
        /// <returns>The associated <see cref="TypeImage"/>.</returns>
        public static implicit operator TypeImage([AllowNull] Type type) => type == null ? null : Get(type);

        /// <summary>
        /// Return the <see cref="TypeImage"/> associated with the provided <see cref="Type"/>./>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get the <see cref="TypeImage"/> of.</param>
        /// <returns>The associated <see cref="TypeImage"/>.</returns>
        public static TypeImage Get(Type type)
        {
            return Cache.GetOrAdd(type, (input) => new TypeImage(input));
        }

        /// <summary>
        /// Return the <see cref="TypeImage"/> associated with the provided <see cref="Type"/>./>
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to get the <see cref="TypeImage"/> of.</typeparam>
        /// <returns>The associated <see cref="TypeImage"/>.</returns>
        public static TypeImage Get<T>()
        {
            return StaticCache<T>.Instance;
        }

        static ArrayHash<Type> HashTypes(ParameterInfo[] parameters)
        {
            return new ArrayHash<Type>(parameters.Select((current) => current.ParameterType).ToArray());
        }

        static Type[] GetAllTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where((current) => !current.IsDynamic)
                .SelectMany((current) => current.GetTypes())
                .ToArray();
        }

        static Dictionary<Type, Type[]> GetSubclassMap()
        {
            var types = GetAllTypes();
            var subclasses = types.ToDictionary(
                (current) => current,
                (current) => new List<Type>());

            foreach (var type in types)
            {
                var superclass = type.BaseType;
                if (superclass == null)
                {
                    continue;
                }

                subclasses.GetOrAdd(superclass, () => new List<Type>()).Add(type);

                if (superclass.IsGenericType)
                {
                    subclasses.GetOrAdd(superclass.GetGenericTypeDefinition(), () => new List<Type>()).Add(type);
                }
            }

            return subclasses.ToDictionary(
                (current) => current.Key,
                (current) => current.Value.Count == 0 ? ArrayUtility<Type>.Empty : current.Value.ToArray());
        }

        /// <summary>
        /// The inner <see cref="Type"/> of the <see cref="TypeImage"/>.
        /// </summary>
        public Type Type => (Type)Member;

        /// <summary>
        /// The <see cref="Assembly"/> the <see cref="TypeImage"/> belongs to.
        /// </summary>
        public Assembly Assembly => Type.Assembly;

        /// <summary>
        /// The base type of the <see cref="TypeImage"/>. Null if the current <see cref="TypeImage"/> is
        /// <see cref="object"/>.
        /// </summary>
        [AllowNull]
        public TypeImage Base { get; }

        /// <summary>
        /// Ascend the inheritance tree, yielding all classes from this <see cref="TypeImage"/> up to
        /// <see cref="object"/>.
        /// </summary>
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

        /// <summary>
        /// Yield all <see cref="TypeImage"/>s that directly inherit or implement this <see cref="TypeImage"/>.
        /// </summary>
        public IEnumerable<TypeImage> Subclasses => LazySubclasses.Value;

        /// <summary>
        /// Yield all <see cref="TypeImage"/>s that inherit or implement this <see cref="TypeImage"/>.
        /// </summary>
        public IEnumerable<TypeImage> Descendants => new TypeTreeEnumerable(this, true);

        /// <summary>
        /// Yield this <see cref="TypeImage"/> followed by all <see cref="TypeImage"/>s that inherit or implement
        /// this <see cref="TypeImage"/>.
        /// </summary>
        public IEnumerable<TypeImage> Tree => new TypeTreeEnumerable(this, false);

        /// <summary>
        /// Ascend the inheritance tree, yielding all classes from the base class of this <see cref="TypeImage"/> up to
        /// <see cref="object"/>.
        /// </summary>
        public IEnumerable<TypeImage> Ancestors => Base?.Inheritance ?? Enumerable.Empty<TypeImage>();

        /// <summary>
        /// Yield all interfaces implemented by this <see cref="TypeImage"/>.
        /// </summary>
        public IEnumerable<TypeImage> Interfaces => LazyInterfaces.Value;

        /// <summary>
        /// Yield all constructors available for this <see cref="TypeImage"/>.
        /// </summary>
        public IEnumerable<ConstructorImage> Constructors => LazyConstructors.Value;

        /// <summary>
        /// True if the <see cref="TypeImage"/> has public visibility.
        /// </summary>
        public override bool IsPublic => Type.IsPublic;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a static class.
        /// </summary>
        public override bool IsStatic => Type.IsAbstract && Type.IsSealed;

        /// <summary>
        /// True if the <see cref="TypeImage"/> can be constructed directly. For this to be true it cannot be abstract
        /// class or an interface.
        /// </summary>
        public bool IsConstructable => !Type.IsAbstract && !Type.IsInterface;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is generic, supporting generic parameters.
        /// </summary>
        public bool IsGeneric => Type.IsGenericType;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a generic definition.
        /// </summary>
        public bool IsGenericDefinition => Type.IsGenericTypeDefinition;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is an array.
        /// </summary>
        public bool IsArray => Type.IsArray;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is an enum.
        /// </summary>
        public bool IsEnum => Type.IsEnum;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a class.
        /// </summary>
        public bool IsClass => Type.IsClass;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is an interface.
        /// </summary>
        public bool IsInterface => Type.IsInterface;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a struct.
        /// </summary>
        public bool IsStruct => Type.IsValueType;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a primitive.
        /// </summary>
        public bool IsPrimitive => Type.IsPrimitive;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is a sealed class.
        /// </summary>
        public bool IsSealed => Type.IsSealed;

        /// <summary>
        /// True if the <see cref="TypeImage"/> is an abstract class.
        /// </summary>
        public bool IsAbstract => Type.IsAbstract;

        /// <summary>
        /// The number of generic arguments the <see cref="TypeImage"/> requires. This is zero if the
        /// <see cref="TypeImage"/> is not generic.
        /// </summary>
        public int GenericArgumentCount => LazyGenericArguments.Value.Length;

        /// <summary>
        /// The generic definition of the <see cref="TypeImage"/>.
        /// If the <see cref="TypeImage"/> is not a generic type or is already a generic definition, the same
        /// <see cref="TypeImage"/> will be returned.
        /// </summary>
        public TypeImage GenericDefinition =>
            IsGeneric && !IsGenericDefinition
                ? Get(Type.GetGenericTypeDefinition())
                : this;

        Cache<Type, TypeImage> AssignableTypeCache { get; } = new Cache<Type, TypeImage>();

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

        Lazy<TypeImage[]> LazySubclasses { get; }
        Lazy<TypeImage[]> LazyGenericArguments { get; }

        TypeImage(Type type) : base(type)
        {
            Base = Type.BaseType == null ? null : Get(Type.BaseType);

            LazyGenericArguments = new Lazy<TypeImage[]>(() =>
                IsGeneric
                    ? Type.GetGenericArguments().Select(Get).ToArray()
                    : ArrayUtility<TypeImage>.Empty);

            {
                LazySubclasses = new Lazy<TypeImage[]>(() =>
                {
                    LazySubclassMap.Value.TryGetValue(Type, out var subclasses);
                    return subclasses != null
                        ? subclasses.Select(Get).ToArray()
                        : ArrayUtility<TypeImage>.Empty;
                });

                LazyInterfaces = new Lazy<TypeImage[]>(GetInterfaces);
            }

            {
                LazyConstructors = new Lazy<ConstructorImage[]>(GetConstructors);

                LazyLocalMethods = new Lazy<MethodImage[]>(GetLocalMethods);
                LazyLocalProperties = new Lazy<PropertyImage[]>(GetLocalProperties);
                LazyLocalFields = new Lazy<FieldImage[]>(GetLocalFields);
                LazyLocalIndexers = new Lazy<IndexerImage[]>(GetLocalIndexers);

                LazySurfaceMethods = new Lazy<MethodImage[]>(() => GetSurfaceMembers(Methods(MemberQuery.All)));
                LazySurfaceProperties = new Lazy<PropertyImage[]>(() => GetSurfaceMembers(Properties(MemberQuery.All)));
                LazySurfaceFields = new Lazy<FieldImage[]>(() => GetSurfaceMembers(Fields(MemberQuery.All)));
                LazySurfaceIndexers = new Lazy<IndexerImage[]>(() => GetSurfaceMembers(Indexers(MemberQuery.All)));
            }

            {
                LazyConstructorMap = new Lazy<Dictionary<ArrayHash<Type>, ConstructorImage>>(() =>
                    Constructors
                        .ToDictionaryByFirst((current) => HashTypes(current.Constructor.GetParameters())));

                LazyMethodMap = new Lazy<Dictionary<Pair<string, ArrayHash<Type>>, MethodImage>>(() =>
                    Methods(MemberQuery.All)
                        .ToDictionaryByFirst((current) =>
                            new Pair<string, ArrayHash<Type>>(current.ShortName,
                                HashTypes(current.Method.GetParameters()))));

                LazyPropertyMap = new Lazy<Dictionary<string, PropertyImage>>(() =>
                    Properties(MemberQuery.All)
                        .ToDictionaryByFirst((current) => current.ShortName));

                LazyFieldMap = new Lazy<Dictionary<string, FieldImage>>(() =>
                    Fields(MemberQuery.All)
                        .ToDictionaryByFirst((current) => current.ShortName));

                LazyIndexerMap = new Lazy<Dictionary<ArrayHash<Type>, IndexerImage>>(() =>
                    Indexers(MemberQuery.All)
                        .ToDictionaryByFirst((current) => HashTypes(current.Property.GetIndexParameters())));
            }
        }

        public override string ToString()
        {
            return $"{nameof(TypeImage)}({Type})";
        }

        /// <summary>
        /// Returns true if the type of the <see cref="TypeImage"/> is assignable to the specified type.
        /// </summary>
        /// <param name="target">The type to which assignability will be determined.</param>
        /// <returns>True if the <see cref="TypeImage"/> can be assigned.</returns>
        public bool IsAssignableTo(Type target)
        {
            return AssignableType(target) != null;
        }

        /// <summary>
        /// Returns the type this <see cref="TypeImage"/> has inherited from or implemented in order to make it
        /// assignable to the provided target type. Returns null if the <see cref="TypeImage"/> is not assignable.
        /// </summary>
        /// <param name="target">The type to which the assignable type will be determined.</param>
        /// <returns>The <see cref="TypeImage"/> of the assignable type, or null if it doesn't exist.</returns>
        [return: AllowNull]
        public TypeImage AssignableType(Type target)
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
                if (image.GenericDefinition == target)
                {
                    return AssignableTypeCache[target] = image.Type;
                }
            }

            return AssignableTypeCache[target] = null;
        }

        /// <summary>
        /// Returns the <see cref="TypeImage"/> of the generic argument at the provided index. Throws an
        /// <see cref="IndexOutOfRangeException"/> if the index is out of bounds.
        /// </summary>
        /// <param name="index">The index of the generic argument to retrieve.</param>
        /// <returns>The <see cref="TypeImage"/> of the generic argument.</returns>
        public TypeImage GenericArgument(int index)
        {
            try
            {
                return LazyGenericArguments.Value[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Convert the <see cref="TypeImage"/>, representing a generic definition, into the <see cref="TypeImage"/>
        /// of a real generic type.
        /// </summary>
        /// <param name="arguments">The generic arguments to use to create the generic type.</param>
        /// <returns>The <see cref="TypeImage"/> of the created generic type.</returns>
        public TypeImage CreateGeneric(params Type[] arguments)
        {
            AssertIsGenericDefinition();

            try
            {
                return Type.MakeGenericType(arguments);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("The provided generic arguments do not match the generic type definition.");
            }
        }

        /// <summary>
        /// Get the constructor with the provided parameter types. Returns null if not found.
        /// </summary>
        /// <param name="types">The parameter types to match.</param>
        /// <returns>The constructor or null if it does not exist.</returns>
        [return: AllowNull]
        public ConstructorImage Constructor(params Type[] types)
        {
            var key = new ArrayHash<Type>(types);
            LazyConstructorMap.Value.TryGetValue(key, out var image);
            return image;
        }

        /// <summary>
        /// Get the method with the provided name and parameter types. Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the method to find.</param>
        /// <param name="types">The parameter types to match.</param>
        /// <returns>The method or null if it does not exist.</returns>
        [return: AllowNull]
        public MethodImage Method(string name, params Type[] types)
        {
            var key = new Pair<string, ArrayHash<Type>>(name, new ArrayHash<Type>(types.CopyArray()));
            LazyMethodMap.Value.TryGetValue(key, out var image);
            return image;
        }

        /// <summary>
        /// Get the property with the provided name. Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the property to find.</param>
        /// <returns>The property or null if it does not exist.</returns>
        [return: AllowNull]
        public PropertyImage Property(string name)
        {
            LazyPropertyMap.Value.TryGetValue(name, out var image);
            return image;
        }

        /// <summary>
        /// Get the field with the provided name. Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the field to find.</param>
        /// <returns>The field or null if it does not exist.</returns>
        [return: AllowNull]
        public FieldImage Field(string name)
        {
            LazyFieldMap.Value.TryGetValue(name, out var image);
            return image;
        }

        /// <summary>
        /// Get the property or field with the provided name. Returns null if not found.
        /// </summary>
        /// <param name="name">The name of the property or field to find.</param>
        /// <returns>The property, field or null if it does not exist.</returns>
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

        /// <summary>
        /// Get the indexer with the provided parameter type(s). Returns null if not found.
        /// </summary>
        /// <param name="types">The parameter types to match.</param>
        /// <returns>The indexer or null if it does not exist.</returns>
        [return: AllowNull]
        public IndexerImage Indexer(params Type[] types)
        {
            LazyIndexerMap.Value.TryGetValue(new ArrayHash<Type>(types), out var indexer);
            return indexer;
        }

        /// <summary>
        /// Return all methods on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All methods matching the query.</returns>
        public IEnumerable<MethodImage> Methods(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceMethods.Value.Iterate();
                case MemberQuery.Local:
                    return LazyLocalMethods.Value.Iterate();
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalMethods.Value);
            }

            return Enumerable.Empty<MethodImage>();
        }

        /// <summary>
        /// Return all fields on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All fields matching the query.</returns>
        public IEnumerable<FieldImage> Fields(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceFields.Value.Iterate();
                case MemberQuery.Local:
                    return LazyLocalFields.Value.Iterate();
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalFields.Value);
            }

            return Enumerable.Empty<FieldImage>();
        }

        /// <summary>
        /// Return all properties on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All properties matching the query.</returns>
        public IEnumerable<PropertyImage> Properties(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceProperties.Value.Iterate();
                case MemberQuery.Local:
                    return LazyLocalProperties.Value.Iterate();
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalProperties.Value);
            }

            return Enumerable.Empty<PropertyImage>();
        }

        /// <summary>
        /// Return all properties and fields on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All properties and fields matching the query.</returns>
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

        /// <summary>
        /// Return all indexers on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All indexers matching the query.</returns>
        public IEnumerable<IndexerImage> Indexers(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceIndexers.Value.Iterate();
                case MemberQuery.Local:
                    return LazyLocalIndexers.Value.Iterate();
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalIndexers.Value);
            }

            return Enumerable.Empty<IndexerImage>();
        }

        TypeImage[] GetInterfaces()
        {
            return TypeUtility.Inheritance(Type)
                .SelectMany((current) => current.GetInterfaces())
                .Select(Get)
                .ToArray();
        }

        ConstructorImage[] GetConstructors()
        {
            return Type
                .GetConstructors(TypeUtility.InstanceBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new ConstructorImage(current))
                .ToArray();
        }

        MethodImage[] GetLocalMethods()
        {
            var images = new List<MethodImage>();
            var instanceMembers = Type
                .GetMethods(TypeUtility.InstanceBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(current));

            var staticMembers = Type
                .GetMethods(TypeUtility.StaticBinding)
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetMethods(TypeUtility.InstanceBinding))
                .Where(CallableImage.CanCreateFrom)
                .Select((current) => new MethodImage(current));

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
                .Select((current) => new PropertyImage(current));

            var staticMembers = Type
                .GetProperties(TypeUtility.StaticBinding)
                .Where(PropertyImage.CanCreateFrom)
                .Select((current) => new PropertyImage(current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                .Where(PropertyImage.CanCreateFrom)
                .Select((current) => new PropertyImage(current));

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
                .Select((current) => new FieldImage(current));

            var staticMembers = Type
                .GetFields(TypeUtility.StaticBinding)
                .Select((current) => new FieldImage(current));

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
                .Select((current) => new IndexerImage(current));

            var interfaceMembers = Type
                .GetInterfaces()
                .SelectMany((current) => current.GetProperties(TypeUtility.InstanceBinding))
                .Where(IndexerImage.CanCreateFrom)
                .Select((current) => new IndexerImage(current));

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
                if (seen.Add(image.ShortName))
                {
                    unique.Add(image);
                }
            }

            return unique.ToArray();
        }

        void AssertIsGenericDefinition()
        {
            if (Type.IsGenericTypeDefinition)
            {
                return;
            }

            throw new InvalidOperationException("Type must be generic.");
        }
    }
}