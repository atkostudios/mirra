using System.Reflection;
using NullGuard;

namespace Atko.Mirra.Images
{
    public abstract class MemberImage : BaseImage
    {
        public bool RequiresInstance => Member.MemberType != MemberTypes.Constructor && !IsStatic;

        protected MemberImage(MemberInfo member) : base(member)
        { }

        public override string ToString()
        {
            return $"{GetType().Name}({Member})";
        }

        protected void AssertInstanceMatches([AllowNull] object instance)
        {
            if ((instance != null) != RequiresInstance)
            {
                throw new MirraInvocationArgumentException();
            }
        }
    }
}