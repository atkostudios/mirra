using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utility;

namespace Ducktype.Models
{
    public abstract class MemberModel
    {
        public string Name { get; }

        public bool IsStatic { get; }
        public bool IsCompilerGenerated { get; }

        public abstract bool IsPublic { get; }

        public MemberInfo Member { get; }

        protected MemberModel(Type owner, MemberInfo member)
        {
            Member = member;
            Name = Member.Name.SubstringAfterLast(".");
            IsStatic = TypeUtility.IsStatic(member);
            IsCompilerGenerated = Member.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        protected void AssertInstanceMatches(object instance)
        {
            if (instance == null && !IsStatic)
            {
                throw new DucktypeException("Attempted to use instance member with null instance.");
            }

            if (instance != null && IsStatic)
            {
                throw new DucktypeException("Attempted to use static member with instance.");
            }
        }
    }
}