using System.Reflection;
using NullGuard;

namespace Atko.Mirra.Images
{
    /// <summary>
    /// Wrapper class for <see cref="MemberInfo"/>s present on types that provides extended functionality and reflection
    /// performance.
    /// </summary>
    public abstract class TypeMemberImage : MemberImage
    {
        protected bool RequiresInstance => Member.MemberType != MemberTypes.Constructor && !IsStatic;

        public override string ToString()
        {
            return $"{GetType().Name}({Member})";
        }

        protected TypeMemberImage(MemberInfo member) : base(member)
        { }

        protected void AssertInstanceMatches([AllowNull] object instance)
        {
            if ((instance != null) != RequiresInstance)
            {
                throw new MirraInvocationArgumentException();
            }
        }
    }
}