using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Atko.Mirra.Utility;
using Microsoft.CSharp;
using NullGuard;

namespace Atko.Mirra.Images
{
    public class TypeImage : BaseImage
    {
        static class StaticCache<T>
        {
            public static TypeImage Instance { get; } = Get(typeof(T));
        }

        public static IEnumerable<TypeImage> All => LazyAll.Value.Iterate();

        static Lazy<TypeImage[]> LazyAll = new Lazy<TypeImage[]>(() => GetAllTypes().Select(Get).ToArray());

        static Cache<Type, TypeImage> Cache { get; } = new Cache<Type, TypeImage>();

        static Lazy<Dictionary<Type, Type[]>> LazySubclassMap { get; } =
            new Lazy<Dictionary<Type, Type[]>>(GetSubclassMap);

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

        public Type Type => (Type)Member;

        public Assembly Assembly => Type.Assembly;

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

        public IEnumerable<TypeImage> Ancestors => Base?.Inheritance ?? Enumerable.Empty<TypeImage>();
        public IEnumerable<TypeImage> Descendants => new TypeTreeEnumerable(this, true);
        public IEnumerable<TypeImage> Tree => new TypeTreeEnumerable(this, false);

        public IEnumerable<TypeImage> Interfaces => LazyInterfaces.Value;
        public IEnumerable<ConstructorImage> Constructors => LazyConstructors.Value;

        public override bool IsPublic => Type.IsPublic;
        public override bool IsStatic => Type.IsAbstract && Type.IsSealed;

        public bool IsConstructable => !Type.IsAbstract && !Type.IsInterface;

        public bool IsGeneric => Type.IsGenericType;
        public bool IsGenericDefinition => Type.IsGenericTypeDefinition;
        public bool IsArray => Type.IsArray;
        public bool IsEnum => Type.IsEnum;
        public bool IsClass => Type.IsClass;
        public bool IsInterface => Type.IsInterface;
        public bool IsStruct => Type.IsValueType;
        public bool IsPrimitive => Type.IsPrimitive;
        public bool IsSealed => Type.IsSealed;
        public bool IsAbstract => Type.IsAbstract;

        public int GenericArgumentCount => LazyGenericArguments.Value.Length;

        public TypeImage GenericDefinition =>
            IsGeneric && !IsGenericDefinition
                ? Get(Type.GetGenericTypeDefinition())
                : this;

        public string DisplayName => LazyDisplayName.Value;

        Cache<Type, Type> AssignableTypeCache { get; } = new Cache<Type, Type>();

        Lazy<string> LazyDisplayName { get; }

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

        public IEnumerable<TypeImage> Subclasses => LazySubclasses.Value;

        Lazy<TypeImage[]> LazySubclasses { get; }
        Lazy<TypeImage[]> LazyGenericArguments { get; }

        TypeImage(Type type) : base(type)
        {
            Base = Type.BaseType == null ? null : Get(Type.BaseType);

            LazyGenericArguments = new Lazy<TypeImage[]>(() =>
                IsGeneric
                    ? Type.GetGenericArguments().Select(Get).ToArray()
                    : ArrayUtility<TypeImage>.Empty);

            LazySubclasses = new Lazy<TypeImage[]>(() =>
            {
                LazySubclassMap.Value.TryGetValue(Type, out var subclasses);
                return subclasses != null
                    ? subclasses.Select(Get).ToArray()
                    : ArrayUtility<TypeImage>.Empty;
            });

            LazyDisplayName = new Lazy<string>(GetDisplayName);

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
                return LazyGenericArguments.Value[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
        }

        public TypeImage CreateGeneric(params Type[] genericArguments)
        {
            AssertIsGeneric();

            try
            {
                return Type.MakeGenericType(genericArguments);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("The provided generic arguments do not match the generic type definition.");
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

        string GetDisplayName()
        {
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder))
            {
                var expression = new CodeTypeReferenceExpression(Type);
                var provider = new CSharpCodeProvider();
                provider.GenerateCodeFromExpression(expression, writer, new CodeGeneratorOptions());
            }

            return builder.ToString().SubstringAfterLast(".");
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

        void AssertIsGeneric()
        {
            if (Type.IsGenericTypeDefinition)
            {
                return;
            }

            throw new InvalidOperationException("Type must be generic.");
        }
    }
}