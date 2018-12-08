using System;
using System.Reflection;
using Utility;

namespace Ducktype.Models
{
    public abstract class MemberModel
    {
        public string Name => Member.Name;

        public bool IsInstance => !IsStatic;
        public bool IsStatic { get; }

        public abstract bool IsPublic { get; }

        public MemberInfo Member { get; }

        protected MemberModel(Type owner, MemberInfo member)
        {
            Member = member;
            IsStatic = TypeUtility.IsStatic(member);
        }

        protected void AssertInstanceMatches(object instance)
        {
            if (instance == null && IsInstance)
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