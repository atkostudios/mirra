using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Atko.Dodge.Utility;

namespace Atko.Dodge.Models
{
    public abstract class MemberModel
    {
        public string Name { get; }
        public bool IsStatic { get; }
        public bool IsCompilerGenerated { get; }

        public abstract bool IsPublic { get; }

        public bool RequiresInstance => Member.MemberType != MemberTypes.Constructor && !IsStatic;

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
            if (instance == null && RequiresInstance)
            {
                const string message = "Member instance cannot be null.";
                throw new DodgeInvocationException(message);
            }

            if (instance != null && !RequiresInstance)
            {
                const string message = "Static member cannot be used with an instance object.";
                throw new DodgeInvocationException(message);
            }
        }
    }
}