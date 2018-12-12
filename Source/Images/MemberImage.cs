using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    public abstract class MemberImage
    {
        public string Name { get; }

        public abstract bool IsPublic { get; }
        public abstract bool IsStatic { get; }

        public bool IsCompilerGenerated { get; }

        public bool RequiresInstance => Member.MemberType != MemberTypes.Constructor && !IsStatic;

        public MemberInfo Member { get; }

        protected MemberImage(Type owner, MemberInfo member)
        {
            Member = member;
            Name = Member.Name.SubstringAfterLast(".");
            IsCompilerGenerated = Member.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Member})";
        }

        protected void AssertInstanceMatches(object instance)
        {
            if (instance == null && RequiresInstance)
            {
                const string message = "Instance cannot be null.";
                throw new MirraInvocationException(message);
            }

            if (instance != null && !RequiresInstance)
            {
                const string message = "Static member cannot be used with an instance object.";
                throw new MirraInvocationException(message);
            }
        }
    }
}