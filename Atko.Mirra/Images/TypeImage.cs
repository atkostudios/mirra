using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="P:Atko.Mirra.Images.TypeImage.Type" /> that provides extended functionality and reflection performance.
    /// </summary>
    public class TypeImage : MemberImage
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
        /// Implicitly convert a type image into its associated system type.
        /// </summary>
        /// <param name="image">The type image to convert.</param>
        /// <returns>The associated type.</returns>
        [return: AllowNull]
        public static implicit operator Type([AllowNull] TypeImage image) => image == null ? null : image.Type;

        /// <summary>
        /// Implicitly convert a system type into its associated type image.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <returns>The associated type image.</returns>
        [return: AllowNull]
        public static implicit operator TypeImage([AllowNull] Type type) => type == null ? null : Get(type);

        /// <summary>
        /// Return the type image associated with the provided system type./>
        /// </summary>
        /// <param name="type">The system type to get the type image of.</param>
        /// <returns>The associated type image.</returns>
        public static TypeImage Get(Type type)
        {
            return Cache.GetOrAdd(type, (input) => new TypeImage(input));
        }

        /// <summary>
        /// Return the type image associated with the provided system type./>
        /// </summary>
        /// <typeparam name="T">The system type to get the type image of.</typeparam>
        /// <returns>The associated type image.</returns>
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
                .Where((current) => !current.IsGenericType || current.IsGenericTypeDefinition)
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
        /// The inner system type of the type image.
        /// </summary>
        public Type Type => (Type)Member;

        /// <summary>
        /// The assembly the type belongs to.
        /// </summary>
        public Assembly Assembly => Type.Assembly;

        /// <summary>
        /// The base class of the type. Can be null.
        /// </summary>
        [AllowNull]
        public TypeImage Base { get; }

        /// <summary>
        /// Ascend the inheritance tree, yielding all classes from this type up to <see cref="object"/>.
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
        /// Yield all classes that directly inherit from this class.
        /// </summary>
        public IEnumerable<TypeImage> Subclasses => LazySubclasses.Value;

        /// <summary>
        /// Yield all classes that directly or indirectly inherit from this class.
        /// </summary>
        public IEnumerable<TypeImage> Descendants => new TypeTreeEnumerable(this, true);

        /// <summary>
        /// Yield this type followed by all classes that directly or indirectly inherit from this class.
        /// </summary>
        public IEnumerable<TypeImage> Tree => new TypeTreeEnumerable(this, false);

        /// <summary>
        /// Ascend the inheritance tree, yielding all classes from the base class of this type up to
        /// <see cref="object"/>.
        /// </summary>
        public IEnumerable<TypeImage> Ancestors => Base?.Inheritance ?? Enumerable.Empty<TypeImage>();

        /// <inheritdoc/>
        public override bool IsPublic => Type.IsPublic;

        /// <inheritdoc/>
        public override bool IsStatic => Type.IsAbstract && Type.IsSealed;

        /// <summary>
        /// True if the type can be constructed directly. For this to be true it cannot be abstract class or an
        /// interface.
        /// </summary>
        public bool IsConstructable => !Type.IsAbstract && !Type.IsInterface;

        /// <summary>
        /// True if the type is generic or is a generic definition.
        /// </summary>
        public bool IsGeneric => Type.IsGenericType || Type.IsGenericTypeDefinition;

        /// <summary>
        /// True if the type is a generic definition.
        /// </summary>
        public bool IsGenericDefinition => Type.IsGenericTypeDefinition;

        /// <summary>
        /// True if the type is an array.
        /// </summary>
        public bool IsArray => Type.IsArray;

        /// <summary>
        /// True if the type is an enum.
        /// </summary>
        public bool IsEnum => Type.IsEnum;

        /// <summary>
        /// True if the type is a class.
        /// </summary>
        public bool IsClass => Type.IsClass;

        /// <summary>
        /// True if the type is an interface.
        /// </summary>
        public bool IsInterface => Type.IsInterface;

        /// <summary>
        /// True if the type is a struct.
        /// </summary>
        public bool IsStruct => Type.IsValueType;

        /// <summary>
        /// True if the type is a primitive.
        /// </summary>
        public bool IsPrimitive => Type.IsPrimitive;

        /// <summary>
        /// True if the type is a sealed class.
        /// </summary>
        public bool IsSealed => Type.IsSealed;

        /// <summary>
        /// True if the type is an abstract class.
        /// </summary>
        public bool IsAbstract => Type.IsAbstract;

        /// <summary>
        /// The number of generic arguments the type requires. This is zero if the type is not generic.
        /// </summary>
        public int GenericArgumentCount => LazyGenericArguments.Value.Length;

        /// <summary>
        /// The generic definition of the type.
        /// If the type is not a generic type or is already a generic definition, the same type will be returned.
        /// </summary>
        public TypeImage GenericDefinition =>
            IsGeneric && !IsGenericDefinition
                ? Get(Type.GetGenericTypeDefinition())
                : this;

        Cache<Type, TypeImage> AssignableTypeCache { get; } = new Cache<Type, TypeImage>();

        Lazy<TypeImage[]> LazyLocalInterfaces { get; }
        Lazy<ConstructorImage[]> LazyLocalConstructors { get; }
        Lazy<MethodImage[]> LazyLocalMethods { get; }
        Lazy<PropertyImage[]> LazyLocalProperties { get; }
        Lazy<FieldImage[]> LazyLocalFields { get; }
        Lazy<AccessorImage[]> LazyLocalAccessors { get; }
        Lazy<IndexerImage[]> LazyLocalIndexers { get; }

        Lazy<MethodImage[]> LazySurfaceMethods { get; }
        Lazy<PropertyImage[]> LazySurfaceProperties { get; }
        Lazy<FieldImage[]> LazySurfaceFields { get; }
        Lazy<AccessorImage[]> LazySurfaceAccessors { get; }
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
            }

            {
                LazyLocalInterfaces = new Lazy<TypeImage[]>(GetLocalInterfaces);
                LazyLocalConstructors = new Lazy<ConstructorImage[]>(GetLocalConstructors);
                LazyLocalMethods = new Lazy<MethodImage[]>(GetLocalMethods);
                LazyLocalProperties = new Lazy<PropertyImage[]>(GetLocalProperties);
                LazyLocalFields = new Lazy<FieldImage[]>(GetLocalFields);
                LazyLocalAccessors = new Lazy<AccessorImage[]>(GetLocalAccessors);
                LazyLocalIndexers = new Lazy<IndexerImage[]>(GetLocalIndexers);

                LazySurfaceMethods = new Lazy<MethodImage[]>(() => GetSurfaceMembers(GetAllMethods()));
                LazySurfaceProperties = new Lazy<PropertyImage[]>(() => GetSurfaceMembers(GetAllProperties()));
                LazySurfaceFields = new Lazy<FieldImage[]>(() => GetSurfaceMembers(GetAllFields()));
                LazySurfaceAccessors = new Lazy<AccessorImage[]>(() => GetSurfaceMembers(GetAllAccessors()));
                LazySurfaceIndexers = new Lazy<IndexerImage[]>(() => GetSurfaceIndexers(GetAllIndexers()));
            }

            {
                LazyConstructorMap = new Lazy<Dictionary<ArrayHash<Type>, ConstructorImage>>(() =>
                    Constructors(MemberQuery.All)
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
        /// Returns true if the type of the type is assignable to the specified type.
        /// </summary>
        /// <param name="target">The type to which assignability will be determined.</param>
        /// <returns>True if the type can be assigned.</returns>
        public bool IsAssignableTo(Type target)
        {
            return AssignableType(target) != null;
        }

        /// <summary>
        /// Returns the type this type has inherited from or implemented in order to make it
        /// assignable to the provided target type. Returns null if the type is not assignable.
        /// </summary>
        /// <param name="target">The type to which the assignable type will be determined.</param>
        /// <returns>The type of the assignable type, or null if it doesn't exist.</returns>
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

            foreach (var image in Inheritance.Concat(Interfaces(MemberQuery.Local)))
            {
                if (image.GenericDefinition == target)
                {
                    return AssignableTypeCache[target] = image.Type;
                }
            }

            return AssignableTypeCache[target] = null;
        }

        /// <summary>
        /// Returns the type of the generic argument at the provided index. Throws an
        /// <see cref="IndexOutOfRangeException"/> if the index is out of bounds.
        /// </summary>
        /// <param name="index">The index of the generic argument to retrieve.</param>
        /// <returns>The type of the generic argument.</returns>
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
        /// Convert the type, representing a generic definition, into the type
        /// of a real generic type.
        /// </summary>
        /// <param name="arguments">The generic arguments to use to create the generic type.</param>
        /// <returns>The type of the created generic type.</returns>
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
        /// Return all interfaces on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All interfaces matching the query.</returns>
        public IEnumerable<TypeImage> Interfaces(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                case MemberQuery.Local:
                    return LazyLocalInterfaces.Value.Iterate();
                case MemberQuery.All:
                    return Inheritance.SelectMany((current) => current.LazyLocalInterfaces.Value);
            }

            return Enumerable.Empty<TypeImage>();
        }

        /// <summary>
        /// Return all constructors on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All constructors matching the query.</returns>
        public IEnumerable<ConstructorImage> Constructors(MemberQuery query = default(MemberQuery))
        {
            return LazyLocalConstructors.Value.Iterate();
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
                    return GetAllMethods();
            }

            return Enumerable.Empty<MethodImage>();
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
                    return GetAllProperties();
            }

            return Enumerable.Empty<PropertyImage>();
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
                    return GetAllFields();
            }

            return Enumerable.Empty<FieldImage>();
        }

        /// <summary>
        /// Return all properties and fields on the type matching a provided <see cref="MemberQuery"/>.
        /// </summary>
        /// <param name="query">The query type.</param>
        /// <returns>All properties and fields matching the query.</returns>
        public IEnumerable<AccessorImage> Accessors(MemberQuery query = default(MemberQuery))
        {
            switch (query)
            {
                case MemberQuery.Surface:
                    return LazySurfaceAccessors.Value.Iterate();
                case MemberQuery.Local:
                    return LazyLocalAccessors.Value.Iterate();
                case MemberQuery.All:
                    return GetAllAccessors();
            }

            return Enumerable.Empty<AccessorImage>();
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
                    return GetAllIndexers();
            }

            return Enumerable.Empty<IndexerImage>();
        }

        IEnumerable<ConstructorImage> GetAllConstructors()
        {
            return GetLocalConstructors();
        }

        IEnumerable<TypeImage> GetAllInterfaces()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalInterfaces.Value);
        }

        IEnumerable<MethodImage> GetAllMethods()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalMethods.Value);
        }

        IEnumerable<PropertyImage> GetAllProperties()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalProperties.Value);
        }

        IEnumerable<FieldImage> GetAllFields()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalFields.Value);
        }

        IEnumerable<AccessorImage> GetAllAccessors()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalAccessors.Value);
        }

        IEnumerable<IndexerImage> GetAllIndexers()
        {
            return Inheritance.SelectMany((current) => current.LazyLocalIndexers.Value);
        }

        TypeImage[] GetLocalInterfaces()
        {
            return Type
                .GetInterfaces()
                .Select(Get)
                .ToArray();
        }

        ConstructorImage[] GetLocalConstructors()
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

        AccessorImage[] GetLocalAccessors()
        {
            return LazyLocalProperties
                .Value
                .Cast<AccessorImage>()
                .Concat(LazyLocalFields.Value)
                .ToArray();
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

        IndexerImage[] GetSurfaceIndexers(IEnumerable<IndexerImage> images)
        {
            var seen = new HashSet<ArrayHash<Type>>();
            var unique = new List<IndexerImage>();
            foreach (var image in images)
            {
                if (seen.Add(HashTypes(image.Property.GetIndexParameters())))
                {
                    unique.Add(image);
                }
            }

            return unique.ToArray();
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