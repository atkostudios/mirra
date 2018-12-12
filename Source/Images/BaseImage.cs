using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    public abstract class BaseImage
    {
        public string Name => Member.Name;

        public string ShortName { get; }

        public bool IsCompilerGenerated { get; }

        [AllowNull]
        public TypeImage DeclaringType => Member.DeclaringType;

        public MemberInfo Member { get; }

        public abstract bool IsPublic { get; }
        public abstract bool IsStatic { get; }

        protected BaseImage(MemberInfo member)
        {
            Member = member;
            ShortName = Member.Name.SubstringAfterLast(".");
            IsCompilerGenerated = HasAttribute<CompilerGeneratedAttribute>();
        }

        public bool HasAttribute(Type type, bool inherit = true)
        {
            return Member.IsDefined(type, inherit);
        }

        public bool HasAttribute<T>(bool inherit = true) where T : class
        {
            return HasAttribute(typeof(T), inherit);
        }

        [return: AllowNull]
        public Attribute Attribute(Type type, bool inherit = true)
        {
            return Member.GetCustomAttribute(type, inherit);
        }

        [return: AllowNull]
        public T Attribute<T>(bool inherit = true) where T : class
        {
            return Attribute(typeof(T), inherit) as T;
        }

        public IEnumerable<Attribute> Attributes(Type type, bool inherit = true)
        {
            return CustomAttributeExtensions.GetCustomAttributes(type, inherit);
        }

        public IEnumerable<Attribute> Attributes(bool inherit = true)
        {
            return Attributes(typeof(Attribute), inherit);
        }

        public IEnumerable<Attribute> Attributes<T>(bool inherit = true) where T : class
        {
            return Attributes(typeof(T), inherit);
        }
    }
}