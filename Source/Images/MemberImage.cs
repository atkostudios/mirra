using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Atko.Mirra.Utility;

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