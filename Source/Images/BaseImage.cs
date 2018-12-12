using System.Reflection;
using System.Runtime.CompilerServices;
using Atko.Mirra.Utility;
using NullGuard;

namespace Atko.Mirra.Images
{
    public abstract class BaseImage
    {
        public string Name { get; }

        public bool IsCompilerGenerated { get; }

        [AllowNull]
        public TypeImage DeclaringType => Member.DeclaringType;

        public MemberInfo Member { get; }

        public abstract bool IsPublic { get; }
        public abstract bool IsStatic { get; }

        protected BaseImage(MemberInfo member)
        {
            Member = member;
            Name = Member.Name.SubstringAfterLast(".");
            IsCompilerGenerated = Member.IsDefined(typeof(CompilerGeneratedAttribute));
        }
    }
}